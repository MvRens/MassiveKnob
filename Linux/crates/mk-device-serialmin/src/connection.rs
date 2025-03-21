use std::sync::Arc;
use std::sync::Mutex;
use std::thread;
use std::time::Duration;

use crossbeam_channel::Sender;
use min_rs::Msg;
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
    min: min_rs::Context<Uart>
}


pub enum SerialMinConnectionState
{
    Disconnected,
    Connected { specs: MassiveKnobDeviceSpecs },
    AnalogInput { input: u8, value: u8 },
    DigitalInput { input: u8, value: bool }
}


impl SerialMinConnection
{
    pub fn new(port: String, baud_rate: u32, state_sender: Sender<SerialMinConnectionState>) -> Self
    {
        let uart = Arc::new(Mutex::new(Uart::new(port, baud_rate, 128)));
        let min = min_rs::Context::new(
            String::from("min"),
            uart.clone(),
            0,
            true);

        Self
        {
            state_sender,

            is_connected: false,
            connect_backoff: ExponentialBackoff::default(),
            uart,
            min
        }
    }


    pub fn try_connect(&mut self) -> bool
    {
        if self.is_connected { return true; }
        if !self.connect_backoff.allowed() { return false; }

        if self.internal_try_connect()
        {
            self.connect_backoff.clear();
            true
        }
        else
        {
            self.connect_backoff.fail();
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


        let mut response_specs = None;

        // Send handshake
        let handshake: [u8; 2] = [b'M', b'K'];
        {
            self.min.queue_frame(MassiveKnobHostToDeviceFrameID::Handshake as u8, &handshake[..], handshake.len() as u8).unwrap_or(());
        }

        // TODO disconnect and reconnect after handshake timeout

        for msg in self.poll_messages(true)
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

                    break;
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
        }


        if let Some(specs) = response_specs
        {
            log::debug!("Connection established");

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


    pub fn poll(&mut self)
    {
        let sender = self.state_sender.clone();

        self.try_connect();

        for msg in self.poll_messages(false)
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
                    _ = sender.send(SerialMinConnectionState::AnalogInput { input: msg.buf[0], value: msg.buf[1] });
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

                Ok(MassiveKnobDeviceToHostFrameID::Error) =>
                {
                    Self::log_error_frame(&msg);
                },

                _ =>
                {
                    Self::log_unrecognized_frame(&msg);
                }
            }
        }

        // TODO poll_messages but stop when no messages are available
    }


    fn disconnect(&mut self)
    {
        // TODO close serial
        // TODO send disconnect state
        self.is_connected = false;
    }


    fn poll_messages(&mut self, wait_for_messages: bool) -> MessageStream<'_>
    {
        MessageStream::new(&mut self.min, wait_for_messages)
    }


    fn log_error_frame(msg: &Msg)
    {
        let error = String::from_utf8(msg.buf.clone()).unwrap_or_else(|_| String::from("<UTF-8 decoding failed>"));
        log::error!("Error reported by device: {}", error);
    }

    fn log_unrecognized_frame(msg: &Msg)
    {
        log::error!("Unrecognized frame ID: {}", msg.min_id);
    }
}


struct MessageStream<'a>
{
    min: &'a mut min_rs::Context<Uart>,
    buffer: Vec<u8>,
    wait_for_messages: bool
}


impl<'a> MessageStream<'a>
{
    fn new(min: &'a mut min_rs::Context<Uart>, wait_for_messages: bool) -> Self
    {
        Self
        {
            min,
            buffer: (0..255).collect(),
            wait_for_messages
        }
    }
}


impl Iterator for MessageStream<'_>
{
    type Item = min_rs::Msg;


    fn next(&mut self) -> Option<Self::Item>
    {
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
                Ok(msg) => return Some(msg),

                Err(min_rs::Error::NoMsg) =>
                {
                    if !self.wait_for_messages
                    {
                        return None;
                    }
                },

                Err(min_rs::Error::NoEnoughTxSpace(overflow)) =>
                {
                    log::debug!("MIN transmit buffer is full, {} bytes overflowed", overflow);

                    // TODO set disconnected??
                    return None;
                }
            }

            thread::sleep(Duration::from_millis(10));
        }
    }
}


/*
                Ok(MassiveKnobDeviceToHostFrameID::AnalogInput) =>
                {
                    if msg.len < 2
                    {
                        println!("Invalid analog input payload length, expected 2, got {}", msg.len);
                    }

                    println!("[Analog input #{}] {}", msg.buf[0], msg.buf[1]);
                },

                Ok(MassiveKnobDeviceToHostFrameID::Error) =>
                {
                    println!("[Device error] {}", match str::from_utf8(msg.buf.as_slice())
                    {
                        Ok(v) => v,
                        Err(_) => "(unable to parse device error message, invalid UTF-8 sequence)"
                    })
                },

                Err(_) =>
                {
                    println!("Unknown message ID: {}", msg.min_id);
                }
            }
        }

        thread::sleep(Duration::from_millis(10));
    }
}

*/