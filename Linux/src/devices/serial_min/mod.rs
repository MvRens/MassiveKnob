use super::registry::{MkDevice, register_device};


pub fn register()
{
    register_device(MkDevice::new("Serial device using MIN protocol"));
}