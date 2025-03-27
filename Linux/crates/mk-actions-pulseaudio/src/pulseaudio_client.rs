use std::sync::LazyLock;
use std::sync::Mutex;
use std::thread;
use std::time::Duration;

use crossbeam_channel::unbounded;
use crossbeam_channel::Receiver;
use crossbeam_channel::Sender;
use libpulse_binding::context::Context;
use libpulse_binding::context::FlagSet;
use libpulse_binding::mainloop::standard::IterateResult;
use libpulse_binding::mainloop::standard::Mainloop;
use libpulse_binding::volume::ChannelVolumes;
use libpulse_binding::volume::Volume;
//use mk_core::target_log;
use mk_core::types::AnalogValue;
use mk_core::util::exponential_backoff::ExponentialBackoff;



//target_log!("pulseaudio");


static INSTANCE: LazyLock<Mutex<PulseAudioClient>> = LazyLock::new(|| Mutex::new(PulseAudioClient::new()));


pub struct PulseAudioClient
{
    worker: PulseAudioWorker
}


impl PulseAudioClient
{
    pub fn call<F>(callback: F) where F : FnOnce(&PulseAudioClient)
    {
        match INSTANCE.lock()
        {
            Ok(i) => callback(&i),
            Err(_) => log::error!(target: "pulseaudio", "Failed to lock PulseAudioClient instance")
        }
    }

    fn new() -> Self
    {
        Self
        {
            worker: PulseAudioWorker::new()
        }
    }



    pub fn set_volume(&self, value: AnalogValue)
    {
        self.worker.set_volume(value);
    }
}



pub struct PulseAudioWorker
{
    in_sender: Sender<PulseAudioWorkerMessage>
}


enum PulseAudioWorkerMessage
{
    Quit,
    SetVolume { value: AnalogValue }
}


impl PulseAudioWorker
{
    pub fn new() -> Self
    {
        let (in_sender, in_receiver) = unbounded();

        thread::spawn(move ||
        {
            Self::run(in_receiver);
        });

        Self
        {
            in_sender
        }
    }


    fn run(in_receiver: Receiver<PulseAudioWorkerMessage>)
    {
        let mut backoff = ExponentialBackoff::new(Duration::from_secs(1), Duration::from_secs(8));
        let mut connection: Option<PulseAudioConnection> = None;

        log::debug!(target: "pulseaudio", "PulseAudio worker started");

        'worker: loop
        {
            // Connect to PulseAudio server if required
            if connection.is_none() && backoff.allowed()
            {
                log::debug!(target: "pulseaudio", "Connecting to PulseAudio server...");

                connection = match Self::try_connect()
                {
                    Some(mut new_connection) =>
                    {
                        log::debug!(target: "pulseaudio", "Waiting for PulseAudio state changes");

                        let mut last_state = new_connection.context.get_state();
                        log::debug!(target: "pulseaudio", "Current PulseAudio state: {:?}", last_state);

                        // Wait while the connection is being established
                        while match last_state
                        {
                            libpulse_binding::context::State::Unconnected
                            | libpulse_binding::context::State::Ready
                            | libpulse_binding::context::State::Failed
                            | libpulse_binding::context::State::Terminated => false,

                            libpulse_binding::context::State::Connecting
                            | libpulse_binding::context::State::Authorizing
                            | libpulse_binding::context::State::SettingName => true
                        }
                        {
                            Self::run_mainloop(&mut new_connection);

                            let new_state = new_connection.context.get_state();
                            if new_state != last_state
                            {
                                log::debug!(target: "pulseaudio", "PulseAudio state changed to: {:?}", last_state);
                                last_state = new_state;
                            }
                        }

                        match new_connection.context.get_state()
                        {
                            libpulse_binding::context::State::Ready =>
                            {
                                log::info!(target: "pulseaudio", "Connected to PulseAudio server");
                                backoff.clear();
                                Some(new_connection)
                            },
                            libpulse_binding::context::State::Failed =>
                            {
                                log::error!(target: "pulseaudio", "Failed to connect to PulseAudio server: {}", new_connection.context.errno());
                                backoff.fail();
                                None
                            },

                            state =>
                            {
                                log::error!(target: "pulseaudio", "PulseAudio state not expected: {:?}", state);
                                backoff.fail();
                                None

                            }
                        }
                    },

                    None =>
                    {
                        backoff.fail();
                        None
                    }
                };
            }


            // Check for incoming requests
            match in_receiver.recv_timeout(Duration::ZERO)
            {
                Ok(msg) =>
                    if let Some(connection) = &connection
                    {
                        if !Self::handle_message(msg, connection)
                        {
                            break 'worker;
                        }
                    },

                Err(crossbeam_channel::RecvTimeoutError::Timeout) => {},
                Err(_) => todo!()
            }


            // Run the PulseAudio main loop
            if let Some(connection) = &mut connection
            {
                Self::run_mainloop(connection);
            }

            thread::sleep(Duration::from_millis(10));
        }
    }

    fn set_volume(&self, value: AnalogValue)
    {
        // TODO verbose logging
        _ = self.in_sender.send(PulseAudioWorkerMessage::SetVolume { value });
    }


    fn try_connect() -> Option<PulseAudioConnection>
    {
        match Mainloop::new()
        {
            Some(mainloop) =>
            {
                match Context::new(&mainloop, "MassiveKnob")
                {
                    Some(mut context) =>
                    {
                        match context.connect(None, FlagSet::NOFLAGS, None)
                        {
                            Ok(_) => Some(PulseAudioConnection { mainloop, context }),
                            Err(_) =>
                            {
                                log::warn!(target: "pulseaudio", "Failed to connect PulseAudio Context");
                                None
                            },
                        }
                    }
                    None =>
                    {
                        log::warn!(target: "pulseaudio", "Failed to construct PulseAudio Context");
                        None
                    },
                }
            },

            None =>
            {
                log::warn!(target: "pulseaudio", "Failed to construct PulseAudio Mainloop");
                None
            },
        }
    }


    fn handle_message(msg: PulseAudioWorkerMessage, connection: &PulseAudioConnection) -> bool
    {
        match msg
        {
            PulseAudioWorkerMessage::Quit => false,
            PulseAudioWorkerMessage::SetVolume { value } =>
            {
                let mut volume = ChannelVolumes::default();
                let percentage: u8 = value.into();
                volume.set(ChannelVolumes::CHANNELS_MAX, Self::percentage_to_volume(percentage as f64));

                // TODO support specific device
                // TODO callback for logging / awaiting?
                connection.context.introspect().set_sink_volume_by_name("@DEFAULT_SINK@", &volume, None);

                true
            },
        }
    }


    fn percentage_to_volume(factor: f64) -> Volume
    {
        let range = Volume::NORMAL.0 as f64 - Volume::MUTED.0 as f64;
        Volume((Volume::MUTED.0 as f64 + factor * range / 100.0) as u32)
    }


    fn run_mainloop(connection: &mut PulseAudioConnection)
    {
        match connection.mainloop.iterate(false)
        {
            IterateResult::Success(_) => {},
            IterateResult::Quit(_) =>
            {
                log::debug!("PulseAudio server quit");
                // TODO disconnect
            },
            IterateResult::Err(e) =>
            {
                log::error!("PulseAudio error: {}", e)
            },
        }
    }
}


impl Drop for PulseAudioWorker
{
    fn drop(&mut self)
    {
    }
}


struct PulseAudioConnection
{
    pub mainloop: Mainloop,
    pub context: Context
}


/*
    fn connect(mainloop: Mainloop) -> Option<PulseAudioConnection>
    {
        match Context::new(&mainloop, "MassiveKnob")
        {
            Some(context) =>
            {
                if let Err(e) = context.connect(None, libpulse_binding::context::FlagSet::NOFLAGS, None)
                {
                    log::warn!(target: "pulseaudio", "Failed to connect to PulseAudio server: {}", e);
                    return None;
                }

                run_until(main_loop, |_main_loop| {
                    let state = context.get_state();
                    log::debug!("Context state: {:?}", state);
                    match state {
                        State::Ready => true,
                        State::Failed => true,
                        State::Unconnected => true,
                        State::Terminated => true,
                        State::Connecting => false,
                        State::Authorizing => false,
                        State::SettingName => false,
                    }
                })
                .map_err(|e| log::error!("Error in PulseAudio main loop: {e}"))?;

                // Check the end state to see if we connected successfully.
                let state = context.get_state();
                match state {
                    State::Ready => (),
                    State::Failed => {
                        log::error!("Failed to connect to PulseAudio server: {}", context.errno());
                        return Err(());
                    },
                    | State::Unconnected
                    | State::Terminated
                    | State::Connecting
                    | State::Authorizing
                    | State::SettingName => {
                        log::error!("PulseAudio context in unexpected state: {state:?}");
                        log::error!("Last error: {}", context.errno());
                        return Err(());
                    }
                }
                Ok(context)
            },

            None =>
            {
                log::warn!(target: "pulseaudio", "Failed to initialize PulseAudio context");
                None
            }
        }
    }
     */