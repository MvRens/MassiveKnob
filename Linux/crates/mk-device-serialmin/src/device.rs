use std::sync::Arc;
use std::sync::Mutex;
use std::thread;
use std::time::Duration;

use crossbeam_channel::select;
use crossbeam_channel::unbounded;
use crossbeam_channel::Receiver;
use crossbeam_channel::RecvError;
use crossbeam_channel::Sender;
use mk_core::device::DeviceEventMessage;
use mk_core::device::DeviceFactory;
use mk_core::device::Device;
use mk_core::device::DeviceSpecs;
use mk_core::device::OutputDevice;
use mk_core::device_output_buffer::DeviceOutputBuffer;
use mk_core::types::AnalogValue;

use crate::connection::SerialMinConnection;
use crate::connection::SerialMinConnectionState;
use crate::protocol::MassiveKnobDeviceSpecs;
use crate::protocol::MassiveKnobHostToDeviceFrameID;


pub struct SerialMinDevice
{
    events_sender: Sender<DeviceEventMessage>,
    in_sender: Sender<WorkerInMessage>,
    specs: Arc<Mutex<Option<DeviceSpecs>>>
}


pub struct SerialMinDeviceSettings
{
    pub port: String,
    pub baud_rate: u32
}


pub struct SerialMinDevicePort
{
    pub port: String,
    pub display_name: String,
    pub candidate_mk_device: bool
}


enum WorkerInMessage
{
    Stop,
    SetAnalogOutput { output: u8, value: AnalogValue },
    SetDigitalOutput { output: u8, value: bool }
}


impl SerialMinDevice
{
    pub fn available_ports() -> Vec<SerialMinDevicePort>
    {
        match serialport::available_ports()
        {
            Ok(ports) => ports.iter().map(|p|
            {
                match &p.port_type
                {
                    serialport::SerialPortType::UsbPort(usb_port_info) =>
                    {
                        SerialMinDevicePort
                        {
                            port: p.port_name.clone(),
                            display_name: match &usb_port_info.product
                            {
                                Some(product) => format!("{}: {}", p.port_name, product),
                                None => format!("{}: vendor = {}, productid = {}", p.port_name, usb_port_info.vid, usb_port_info.pid)
                            },
                            candidate_mk_device: true
                        }
                    },

                    serialport::SerialPortType::PciPort |
                    serialport::SerialPortType::BluetoothPort |
                    serialport::SerialPortType::Unknown =>
                    {
                        SerialMinDevicePort
                        {
                            port: p.port_name.clone(),
                            display_name: p.port_name.clone(),
                            candidate_mk_device: false
                        }
                    }
                }
            }).collect(),
            Err(_) => Vec::new(),
        }
    }


    fn start(&self, in_receiver: Receiver<WorkerInMessage>, settings: SerialMinDeviceSettings)
    {
        let events_sender = self.events_sender.clone();
        let specs = self.specs.clone();

        thread::spawn(move ||
        {
            SerialMinDeviceWorker::new(specs.clone(), settings, events_sender.clone())
                .run(in_receiver);
        });
    }
}


impl Drop for SerialMinDevice
{
    fn drop(&mut self)
    {
        _ = self.in_sender.send(WorkerInMessage::Stop);
    }
}


impl OutputDevice for SerialMinDevice
{
    async fn set_analog_output(&mut self, output: u8, value: AnalogValue)
    {
        _ = self.in_sender.send(WorkerInMessage::SetAnalogOutput { output, value });
    }


    async fn set_digital_output(&mut self, output: u8, value: bool)
    {
        _ = self.in_sender.send(WorkerInMessage::SetDigitalOutput { output, value });
    }
}


impl Device for SerialMinDevice
{
    fn is_connected(&self) -> bool
    {
        match self.specs.lock()
        {
            Ok(specs) => specs.is_some(),
            Err(_) => false
        }
    }


    fn get_specs(&self) -> Option<DeviceSpecs>
    {
        match self.specs.lock()
        {
            Ok(specs) => specs.clone(),
            Err(_) => None
        }
    }
}


impl DeviceFactory<SerialMinDeviceSettings> for SerialMinDevice
{
    fn create(settings: SerialMinDeviceSettings, events_sender: Sender<DeviceEventMessage>) -> SerialMinDevice
    {
        let (in_sender, in_receiver) = unbounded();

        let device = SerialMinDevice
        {
            events_sender,
            in_sender,
            specs: Arc::new(Mutex::new(None))
        };

        device.start(in_receiver, settings);
        device
    }
}



struct SerialMinDeviceWorker
{
    events_sender: Sender<DeviceEventMessage>,
    specs: Arc<Mutex<Option<DeviceSpecs>>>,

    output_buffer: DeviceOutputBuffer,
    connection: SerialMinConnection,
    state_receiver: Receiver<SerialMinConnectionState>
}


impl SerialMinDeviceWorker
{
    pub fn new(specs: Arc<Mutex<Option<DeviceSpecs>>>, settings: SerialMinDeviceSettings, events_sender: Sender<DeviceEventMessage>) -> Self
    {
        let (state_sender, state_receiver) = unbounded();

        Self
        {
            events_sender,
            specs,

            output_buffer: DeviceOutputBuffer::new(),
            connection: SerialMinConnection::new(settings.port, settings.baud_rate, state_sender),
            state_receiver
        }
    }


    pub fn run(&mut self, in_receiver: Receiver<WorkerInMessage>)
    {
        // TODO check result
        self.connection.try_connect();

        loop
        {
            select! {
                recv(in_receiver) -> msg =>
                    if !self.handle_worker_request(msg)
                    {
                        break;
                    },

                recv(self.state_receiver) -> msg =>
                    self.handle_state_change(msg),

                default(Duration::from_millis(10)) =>
                {
                    self.connection.poll();
                }
            }
        }
    }


    fn handle_worker_request(&mut self, msg: Result<WorkerInMessage, RecvError>) -> bool
    {
        match msg
        {
            Ok(WorkerInMessage::Stop) =>
            {
                let payload: [u8; 0] = [];
                self.connection.try_send(MassiveKnobHostToDeviceFrameID::Quit, &payload, 0);
                false
            },

            // TODO also send to connection
            Ok(WorkerInMessage::SetAnalogOutput { output, value }) =>
            {
                self.output_buffer.set_analog_output(output,value);

                let payload: [u8; 2] = [output, value.into()];
                self.connection.try_send(MassiveKnobHostToDeviceFrameID::AnalogOutput, &payload[..], payload.len() as u8);
                true
            },

            Ok(WorkerInMessage::SetDigitalOutput { output, value }) =>
            {
                self.output_buffer.set_digital_output(output,value);

                let payload: [u8; 2] = [output, if value { 1 } else { 0 }];
                self.connection.try_send(MassiveKnobHostToDeviceFrameID::DigitalOutput, &payload[..], payload.len() as u8);
                true
            },

            Err(e) =>
            {
                log::warn!(target: "serialmin", "Internal error while processing worker request message: {}", e);
                false
            }
        }
    }


    fn handle_state_change(&mut self, msg: Result<SerialMinConnectionState, RecvError>)
    {
        match msg
        {
            Ok(SerialMinConnectionState::Connected { specs }) =>
            {
                if let Ok(mut stored_specs) = self.specs.lock()
                {
                    stored_specs.replace(DeviceSpecs
                    {
                        analog_inputs: specs.analog_inputs,
                        digital_inputs: specs.digital_inputs,
                        analog_outputs: specs.analog_outputs,
                        digital_outputs: specs.digital_outputs
                    });
                }
                Self::send_buffered_outputs(&mut self.output_buffer, specs);
            },

            Ok(SerialMinConnectionState::Disconnected) =>
            {
                if let Ok(mut stored_specs) = self.specs.lock()
                {
                    stored_specs.take();
                }
            },

            Ok(SerialMinConnectionState::AnalogInput { input, value }) =>
            {
                _ = self.events_sender.send(DeviceEventMessage::AnalogInput { input, value });
            },

            Ok(SerialMinConnectionState::DigitalInput { input, value }) =>
            {
                _ = self.events_sender.send(DeviceEventMessage::DigitalInput { input, value });
            },

            Err(_) => todo!()
        }
    }


    fn send_buffered_outputs(output_buffer: &mut DeviceOutputBuffer, specs: MassiveKnobDeviceSpecs)
    {
        for (output, _value) in output_buffer.flush_analog_outputs()
        {
            if output <= specs.analog_outputs
            {
                // TODO
            }
        }

        for (output, _value) in output_buffer.flush_digital_outputs()
        {
            if output <= specs.digital_outputs
            {
                // TODO
            }
        }
    }
}