use std::sync::Arc;
use std::sync::Mutex;
use std::thread;
use std::time::Duration;
use std::time::Instant;

use crossbeam_channel::Sender;
use min_rs::Msg;
use mk_core::types::AnalogValue;
use mk_core::util::exponential_backoff::ExponentialBackoff;

use crate::protocol::MassiveKnobDeviceToHostFrameID;
use crate::protocol::MassiveKnobHostToDeviceFrameID;
use crate::uart::Uart;
use crate::protocol::MassiveKnobDeviceSpecs;



pub struct SerialMinConnection
{
    state_sender: Sender<SerialMinConnectionState>,

    is_connected: bool,
    connect_backoff: ExponentialBackoff,

    uart: Arc<Mutex<Uart>>,
    min: min_rs::Context<Uart>,
    buffer: Vec<u8>,
    last_message: Instant
}


pub enum SerialMinConnectionState
{
    Disconnected,
    Connected { specs: MassiveKnobDeviceSpecs },
    AnalogInput { input: u8, value: AnalogValue },
    DigitalInput { input: u8, value: bool }
}


static HANDSHAKE_TIMEOUT: Duration = Duration::from_secs(1);
static KEEPALIVE_TIMEOUT: Duration = Duration::from_secs(5);


impl SerialMinConnection
{
    pub fn new(port: String, baud_rate: u32, state_sender: Sender<SerialMinConnectionState>) -> Self
    {
        let uart = Arc::new(Mutex::new(Uart::new(port, baud_rate, 128)));
        let min = min_rs::Context::new(
            String::from("min_rs"),
            uart.clone(),
            0,
            true);

        Self
        {
            state_sender,

            is_connected: false,
            connect_backoff: ExponentialBackoff::default(),
            uart,
            min,

            buffer: (0..255).collect(),
            last_message: Instant::now()
        }
    }


    pub fn try_connect(&mut self) -> bool
    {
        if self.is_connected
        {
            if Instant::now().duration_since(self.last_message) > KEEPALIVE_TIMEOUT
            {
                log::warn!("Keep alive timeout expired, disconnecting from serial device");
                self.disconnect();
            }
            else
            {
                return true;
            }
        }

        if !self.connect_backoff.allowed() { return false; }

        if self.internal_try_connect()
        {
            self.connect_backoff.clear();
            true
        }
        else
        {
            let retry_timeout = self.connect_backoff.fail();
            log::warn!("Connecting to serial device failed, retry in {:?}", retry_timeout);

            false
        }
    }

    fn internal_try_connect(&mut self) -> bool
    {
        {
            let locked_uart = self.uart.lock();
            if let Ok(locked_uart) = locked_uart
            {
                if let Err(e) = locked_uart.try_open()
                {
                    log::error!("Error while opening serial device: {}", e);
                    return false;
                }
            }
            else
            {
                log::debug!("Failed to acquire uart lock");
                return false;
            }

            if let Err(e) = self.min.reset_transport(true)
            {
                log::error!("Error while resetting MIN transport: {}", e);
                return false;
            }
        }


        let response_specs;

        // Send handshake
        log::debug!("Sending handshake...");
        let handshake: [u8; 2] = [b'M', b'K'];
        {
            if let Err(e) = self.min.queue_frame(MassiveKnobHostToDeviceFrameID::Handshake as u8, &handshake[..], handshake.len() as u8)
            {
                log::error!("Failed to send handshake: {}", e);
            }
        }

        let handshake_start = Instant::now();

        'handshake: loop
        {
            if Instant::now() - handshake_start > HANDSHAKE_TIMEOUT
            {
                log::warn!("Handshake timeout, disconnecting...");
                self.disconnect();
                return false
            }

            match self.poll_message(Some(HANDSHAKE_TIMEOUT))
            {
                PollMessageResult::Message(msg) =>
                {
                    match MassiveKnobDeviceToHostFrameID::try_from(msg.min_id)
                    {
                        Ok(MassiveKnobDeviceToHostFrameID::HandshakeResponse) =>
                        {
                            if msg.len < 4
                            {
                                log::warn!("Invalid handshake response length during handshake. Expected 4, got {}", msg.len);
                                return false;
                            }

                            response_specs = Some(MassiveKnobDeviceSpecs
                            {
                                analog_inputs: msg.buf[0],
                                digital_inputs: msg.buf[1],
                                analog_outputs: msg.buf[2],
                                digital_outputs: msg.buf[3]
                            });

                            break 'handshake;
                        },

                        Ok(MassiveKnobDeviceToHostFrameID::AnalogInput) |
                        Ok(MassiveKnobDeviceToHostFrameID::DigitalInput) =>
                        {
                            log::debug!("Received input frame during handshake, ignoring");
                        },

                        Ok(MassiveKnobDeviceToHostFrameID::Error) =>
                        {
                            Self::log_error_frame(&msg);
                        },

                        _ =>
                        {
                            Self::log_unrecognized_frame(&msg);
                            return false;
                        }
                    }
                },

                PollMessageResult::Timeout =>
                {
                    log::warn!("Timeout while polling for messages");
                    return false;
                }

                PollMessageResult::Disconnected =>
                {
                    log::warn!("Device disconnected while polling for messages");
                    return false;
                }
            }
        }


        if let Some(specs) = response_specs
        {
            log::info!("Connected to serial device");

            // TODO verbose logging on failure
            _ = self.state_sender.send(SerialMinConnectionState::Connected { specs });

            self.is_connected = true;

            true
        }
        else
        {
            log::debug!("No device specs received after handshake, connecting failed");
            false
        }
    }


    pub fn try_send(&mut self, id: MassiveKnobHostToDeviceFrameID, payload: &[u8], len: u8) -> bool
    {
        self.min.queue_frame(id as u8, payload, len).is_ok()
    }


    pub fn poll(&mut self)
    {
        let sender = self.state_sender.clone();

        if !self.try_connect() { return; }

        loop
        {
            match self.poll_message(None)
            {
                PollMessageResult::Message(msg) =>
                {
                    match MassiveKnobDeviceToHostFrameID::try_from(msg.min_id)
                    {
                        Ok(MassiveKnobDeviceToHostFrameID::HandshakeResponse) =>
                        {
                            log::debug!("Handshake response received after initial handshake");
                            // TODO reconnect?
                            break;
                        },

                        Ok(MassiveKnobDeviceToHostFrameID::AnalogInput) =>
                        {
                            if msg.len < 2
                            {
                                log::warn!("Invalid analog input payload length, expected 2, got {}", msg.len);
                            }

                            // TODO verbose logging on failure
                            _ = sender.send(SerialMinConnectionState::AnalogInput { input: msg.buf[0], value: msg.buf[1].into() });
                        },

                        Ok(MassiveKnobDeviceToHostFrameID::DigitalInput) =>
                        {
                            if msg.len < 2
                            {
                                log::warn!("Invalid digital input payload length, expected 2, got {}", msg.len);
                            }

                            // TODO verbose logging on failure
                            _ = sender.send(SerialMinConnectionState::DigitalInput { input: msg.buf[0], value: msg.buf[1] != 0 });
                        },

                        Ok(MassiveKnobDeviceToHostFrameID::KeepAlive) =>
                        {
                            // TODO record moment
                            log::debug!("Keep-alive received");
                        },

                        Ok(MassiveKnobDeviceToHostFrameID::Error) =>
                        {
                            Self::log_error_frame(&msg);
                        },

                        _ =>
                        {
                            Self::log_unrecognized_frame(&msg);
                        }
                    }
                },

                PollMessageResult::Timeout | PollMessageResult::Disconnected =>
                {
                    return;
                },
            }
        }
    }


    fn disconnect(&mut self)
    {
        if !self.is_connected { return; }

        if let Ok(uart) = self.uart.lock()
        {
            uart.close();
        }

        _ = self.state_sender.send(SerialMinConnectionState::Disconnected);
        self.is_connected = false;
    }


    fn poll_message(&mut self, timeout: Option<Duration>) -> PollMessageResult
    {
        let poll_start = Instant::now();

        loop
        {
            self.min.poll(&[0][0..0], 0);

            let data = match self.min.hw_if.lock()
            {
                Ok(n) => n.read(&mut self.buffer[..]),
                Err(_) => Err(()),
            };

            match data
            {
                Ok(n) => self.min.poll(&self.buffer[0..n], n as u32),
                Err(_e) =>
                {
                    /*
                    log::error!("Error while reading from serial port: {:?}", e);
                    // TODO set disconnected
                    return None
                    */
                }
            }

            match self.min.get_msg()
            {
                Ok(msg) =>
                {
                    self.last_message = Instant::now();
                    return PollMessageResult::Message(msg);
                },

                Err(min_rs::Error::NoMsg) =>
                {
                    match timeout
                    {
                        Some(t) => if Instant::now() - poll_start > t { return PollMessageResult::Timeout; },
                        None => return PollMessageResult::Timeout,
                    }
                },

                Err(min_rs::Error::NoEnoughTxSpace(overflow)) =>
                {
                    log::debug!("MIN transmit buffer is full, {} bytes overflowed", overflow);

                    self.disconnect();
                    return PollMessageResult::Disconnected;
                }
            }

            thread::sleep(Duration::from_millis(10));
        }
    }


    fn log_error_frame(msg: &Msg)
    {
        let error = String::from_utf8(msg.buf.clone()).unwrap_or_else(|_| String::from("<UTF-8 decoding failed>"));
        log::error!("Error reported by device: {}", error);
    }

    fn log_unrecognized_frame(msg: &Msg)
    {
        log::warn!("Unrecognized frame ID {} of length {}", msg.min_id, msg.len);
    }
}


enum PollMessageResult
{
    Timeout,
    Message(Msg),
    Disconnected
}