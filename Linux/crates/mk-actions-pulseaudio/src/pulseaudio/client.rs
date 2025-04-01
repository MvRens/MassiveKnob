use std::sync::LazyLock;
use std::sync::Mutex;
use std::thread;
use std::time::Duration;

use crossbeam_channel::unbounded;
use crossbeam_channel::Receiver;
use crossbeam_channel::Sender;
use libpulse_binding::callbacks::ListResult;
use libpulse_binding::context::Context;
use libpulse_binding::context::FlagSet;
use libpulse_binding::mainloop::standard::IterateResult;
use libpulse_binding::mainloop::standard::Mainloop;
use libpulse_binding::volume::ChannelVolumes;
use mk_core::types::AnalogValue;
use mk_core::util::exponential_backoff::ExponentialBackoff;

use crate::pulseaudio::helpers;
use crate::shared_oneshot;
use crate::shared_oneshot::SharedSender;
use crate::PulseAudioDevice;



static INSTANCE: LazyLock<Mutex<PulseAudioClient>> = LazyLock::new(|| Mutex::new(PulseAudioClient::new()));


pub struct PulseAudioClient
{
    worker: PulseAudioWorker
}


impl PulseAudioClient
{
    pub async fn available_output_devices() -> Vec<PulseAudioDevice>
    {
        let (sender, receiver) = shared_oneshot::channel::<Vec<PulseAudioDevice>>();

        Self::call(|p|
        {
            p.worker.available_output_devices(sender);
        });

        receiver.await.unwrap_or_default()
    }


    pub fn set_volume(device_name: &str, value: AnalogValue)
    {
        Self::call(|p|
        {
            p.worker.set_volume(device_name, value);
        });
    }


    fn call<F>(callback: F) where F : FnOnce(&PulseAudioClient)
    {
        match INSTANCE.lock()
        {
            Ok(i) => callback(&i),
            Err(_) => log::error!("Failed to lock PulseAudioClient instance")
        }
    }


    fn new() -> Self
    {
        Self
        {
            worker: PulseAudioWorker::new()
        }
    }
}



pub struct PulseAudioWorker
{
    in_sender: Sender<PulseAudioWorkerMessage>
}


enum PulseAudioWorkerMessage
{
    Quit,
    SetVolume { device_name: String, value: AnalogValue },
    GetOutputDevices { sender: SharedSender<Vec<PulseAudioDevice>> }
}


impl PulseAudioWorker
{
    fn new() -> Self
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

        log::debug!("PulseAudio worker started");

        'worker: loop
        {
            // Connect to PulseAudio server if required
            if connection.is_none() && backoff.allowed()
            {
                log::debug!("Connecting to PulseAudio server...");

                connection = match Self::try_connect()
                {
                    Some(mut new_connection) =>
                    {
                        log::debug!("Waiting for PulseAudio state changes");

                        let mut last_state = new_connection.context.get_state();
                        log::debug!("Current PulseAudio state: {:?}", last_state);

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
                                log::debug!("PulseAudio state changed to: {:?}", last_state);
                                last_state = new_state;
                            }
                        }

                        match new_connection.context.get_state()
                        {
                            libpulse_binding::context::State::Ready =>
                            {
                                log::info!("Connected to PulseAudio server");
                                backoff.clear();
                                Some(new_connection)
                            },
                            libpulse_binding::context::State::Failed =>
                            {
                                log::error!("Failed to connect to PulseAudio server: {}", new_connection.context.errno());
                                backoff.fail();
                                None
                            },

                            state =>
                            {
                                log::error!("PulseAudio state not expected: {:?}", state);
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


    fn set_volume(&self, device_name: &str, value: AnalogValue)
    {
        // TODO verbose logging
        _ = self.in_sender.send(PulseAudioWorkerMessage::SetVolume { device_name: String::from(device_name), value });
    }


    fn available_output_devices(&self, sender: SharedSender<Vec<PulseAudioDevice>>)
    {
        // TODO verbose logging
        _ = self.in_sender.send(PulseAudioWorkerMessage::GetOutputDevices { sender });
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
                                log::warn!("Failed to connect PulseAudio Context");
                                None
                            },
                        }
                    }
                    None =>
                    {
                        log::warn!("Failed to construct PulseAudio Context");
                        None
                    },
                }
            },

            None =>
            {
                log::warn!("Failed to construct PulseAudio Mainloop");
                None
            },
        }
    }


    fn handle_message(msg: PulseAudioWorkerMessage, connection: &PulseAudioConnection) -> bool
    {
        match msg
        {
            PulseAudioWorkerMessage::Quit => false,
            PulseAudioWorkerMessage::SetVolume { device_name, value } =>
            {
                let volume_value = helpers::into_volume(value);

                let mut volume = ChannelVolumes::default();
                volume.set(ChannelVolumes::CHANNELS_MAX, volume_value);

                // TODO callback for logging / awaiting?
                log::debug!("Setting volume for {} to {} ({})", device_name, value, volume_value.0);
                connection.context.introspect().set_sink_volume_by_name(&device_name, &volume, None);

                true
            },

            PulseAudioWorkerMessage::GetOutputDevices { sender } =>
            {
                let mut result: Option<Vec<PulseAudioDevice>> = Some(vec![]);

                connection.context.introspect().get_sink_info_list(move |list_result|
                {
                    match list_result
                    {
                        ListResult::Item(item) =>
                        {
                            if let Some(ref mut result) = result
                            {
                                result.push(PulseAudioDevice
                                {
                                    name: item.name.as_ref().map_or_else(|| String::from("@UNKNOWN@"), |v| v.to_string()),
                                    device_name: item.proplist.get_str("alsa.card_name").unwrap_or_else(|| String::from("<No ALSA device name>")),
                                    display_name: item.description.as_ref().map_or_else(|| String::from("<No device description>"), |v| v.to_string())
                                });
                            }
                        },

                        ListResult::End =>
                        {
                            if let Some(result) = result.take()
                            {
                                let _ = sender.try_send(result);
                            }
                        },

                        ListResult::Error => todo!(),
                    };
                });

                true
            },
        }
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
        // TODO verbose logging
        let _ = self.in_sender.send(PulseAudioWorkerMessage::Quit);
    }
}


struct PulseAudioConnection
{
    pub mainloop: Mainloop,
    pub context: Context
}