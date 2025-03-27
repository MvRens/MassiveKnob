use std::future::Future;

use crossbeam_channel::Sender;

use crate::types::AnalogValue;

pub trait Device: OutputDevice
{
    fn is_connected(&self) -> bool;
    fn get_specs(&self) -> Option<DeviceSpecs>;
}

pub trait OutputDevice
{
    fn set_analog_output(&mut self, output: u8, value: AnalogValue) -> impl Future<Output = ()> + Send;
    fn set_digital_output(&mut self, output: u8, value: bool) -> impl Future<Output = ()> + Send;
}



pub trait DeviceFactory<TSettings>
{
    fn create(settings: TSettings, events_sender: Sender<DeviceEventMessage>) -> Self;
}



#[derive(Clone, Debug)]
pub struct DeviceSpecs
{
    pub analog_inputs: u8,
    pub digital_inputs: u8,
    pub analog_outputs: u8,
    pub digital_outputs: u8
}



pub enum DeviceEventMessage
{
    Connected { specs: DeviceSpecs },
    Disconnected,

    AnalogInput { input: u8, value: AnalogValue },
    DigitalInput { input: u8, value: bool }
}