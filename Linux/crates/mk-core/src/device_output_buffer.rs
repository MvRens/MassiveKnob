use std::collections::HashMap;

use crate::device::Device;


pub struct DeviceOutputBuffer
{
    analog_outputs: HashMap<u8, u8>,
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


    pub fn flush_analog_outputs(&mut self) -> HashMap<u8, u8>
    {
        std::mem::take(&mut self.analog_outputs)
    }


    pub fn flush_digital_outputs(&mut self) -> HashMap<u8, bool>
    {
        std::mem::take(&mut self.digital_outputs)
    }
}


impl Default for DeviceOutputBuffer
{
    fn default() -> Self
    {
        Self::new()
    }
}


impl Device for DeviceOutputBuffer
{
    fn set_analog_output(&mut self, output: u8, value: u8)
    {
        self.analog_outputs.insert(output, value);
    }


    fn set_digital_output(&mut self, output: u8, value: bool)
    {
        self.digital_outputs.insert(output, value);
    }
}