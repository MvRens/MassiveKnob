
use std::thread;
use std::time::Duration;

use crossbeam_channel::bounded;
use crossbeam_channel::select;
use crossbeam_channel::unbounded;
use crossbeam_channel::Receiver;
use crossbeam_channel::Sender;
use mk_core::device::DeviceEventMessage;
use mk_core::device::DeviceFactory;
use mk_core::device::Device;
use mk_core::device_output_buffer::DeviceOutputBuffer;

use crate::connection::SerialMinConnection;
use crate::connection::SerialMinConnectionState;
use crate::protocol::MassiveKnobDeviceSpecs;


pub struct SerialMinDevice
{
    events_sender: Sender<DeviceEventMessage>,
    in_sender: Sender<WorkerInMessage>
}


pub struct SerialMinDeviceSettings
{
    pub port: String,
    pub baud_rate: u32
}


enum WorkerInMessage
{
    Stop,
    SetAnalogOutput { output: u8, value: u8 },
    SetDigitalOutput { output: u8, value: bool }
}


impl SerialMinDevice
{
    pub fn available_ports() -> Vec<String>
    {
        match serialport::available_ports()
        {
            Ok(ports) => ports.iter().map(|p| p.port_name.clone()).collect(),
            Err(_) => Vec::new(),
        }
    }


    fn start(&self, in_receiver: Receiver<WorkerInMessage>, settings: SerialMinDeviceSettings)
    {
        let events_sender_local = self.events_sender.clone();


        thread::spawn(move ||
        {
            let mut output_buffer = DeviceOutputBuffer::new();
            let (state_sender, state_receiver) = unbounded();

            let mut connection = SerialMinConnection::new(settings.port, settings.baud_rate, state_sender);

            // TODO check result
            connection.try_connect();

            loop
            {
                select! {
                    recv(in_receiver) -> msg =>
                    {
                        // Handle incoming requests
                        match msg
                        {
                            Ok(WorkerInMessage::Stop) => break,
                            // TODO also send to connection
                            Ok(WorkerInMessage::SetAnalogOutput { output, value }) => { output_buffer.set_analog_output(output,value); },
                            Ok(WorkerInMessage::SetDigitalOutput { output, value }) => { output_buffer.set_digital_output(output,value); },
                            Err(_) => todo!()
                        }
                    },

                    recv(state_receiver) -> msg =>
                    {
                        // Connection state changed
                        match msg
                        {
                            Ok(SerialMinConnectionState::Connected { specs }) =>
                            {
                                Self::send_buffered_outputs(&mut output_buffer, specs);
                            },

                            Ok(SerialMinConnectionState::Disconnected) =>
                            {
                                // TODO
                            },

                            Ok(SerialMinConnectionState::AnalogInput { input, value }) =>
                            {
                                _ = events_sender_local.send(DeviceEventMessage::AnalogInput { input, value });
                            },

                            Ok(SerialMinConnectionState::DigitalInput { input, value }) =>
                            {
                                _ = events_sender_local.send(DeviceEventMessage::DigitalInput { input, value });
                            },

                            Err(_) => todo!()
                        }

                    }

                    default(Duration::from_millis(10)) =>
                    {
                        connection.poll();
                    }
                }
            }
        });
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


impl Drop for SerialMinDevice
{
    fn drop(&mut self)
    {
        _ = self.in_sender.send(WorkerInMessage::Stop);
    }
}


impl Device for SerialMinDevice
{
    fn set_analog_output(&mut self, output: u8, value: u8)
    {
        _ = self.in_sender.send(WorkerInMessage::SetAnalogOutput { output, value });
    }


    fn set_digital_output(&mut self, output: u8, value: bool)
    {
        _ = self.in_sender.send(WorkerInMessage::SetDigitalOutput { output, value });
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
            in_sender
        };

        device.start(in_receiver, settings);
        device
    }
}