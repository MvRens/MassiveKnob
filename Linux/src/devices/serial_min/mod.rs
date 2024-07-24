use crate::registry::Registry;
use crate::util::unique_id::UniqueId;
use super::{Device, DeviceRegistryItem};


pub struct SerialMinDevice
{

}


impl Device for SerialMinDevice
{
    fn activate(&mut self)
    {
        //todo!()
    }


    fn deactivate(&mut self)
    {
        //todo!()
    }
}


pub fn register(registry: &mut Registry<DeviceRegistryItem>)
{
    registry.register(DeviceRegistryItem {
        unique_id: UniqueId::new("serial_min"),
        factory: || Box::new(SerialMinDevice {})
    });
}