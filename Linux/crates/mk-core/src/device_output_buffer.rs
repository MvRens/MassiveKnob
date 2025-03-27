use std::collections::HashMap;

use crate::types::AnalogValue;


pub struct DeviceOutputBuffer
{
    analog_outputs: HashMap<u8, AnalogValue>,
    digital_outputs: HashMap<u8, bool>
}



impl DeviceOutputBuffer
{
    pub fn new() -> Self
    {
        Self
        {
            analog_outputs: HashMap::new(),
            digital_outputs: HashMap::new()
        }
    }


    pub fn flush_analog_outputs(&mut self) -> HashMap<u8, AnalogValue>
    {
        std::mem::take(&mut self.analog_outputs)
    }


    pub fn flush_digital_outputs(&mut self) -> HashMap<u8, bool>
    {
        std::mem::take(&mut self.digital_outputs)
    }


    pub fn set_analog_output(&mut self, output: u8, value: AnalogValue)
    {
        self.analog_outputs.insert(output, value);
    }


    pub fn set_digital_output(&mut self, output: u8, value: bool)
    {
        self.digital_outputs.insert(output, value);
    }
}


impl Default for DeviceOutputBuffer
{
    fn default() -> Self
    {
        Self::new()
    }
}