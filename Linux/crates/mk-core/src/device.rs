use crossbeam_channel::Sender;

pub trait Device
{
    fn set_analog_output(&mut self, output: u8, value: u8);
    fn set_digital_output(&mut self, output: u8, value: bool);
}


pub trait DeviceFactory<TSettings>
{
    fn create(settings: TSettings, events_sender: Sender<DeviceEventMessage>) -> Self;
}


pub enum DeviceEventMessage
{
    AnalogInput { input: u8, value: u8 },
    DigitalInput { input: u8, value: bool }
}