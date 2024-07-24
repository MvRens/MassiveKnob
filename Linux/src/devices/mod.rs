pub mod emulator;
pub mod serial_min;


use crate::registry::Registry;
use crate::registry::RegistryItem;
use crate::util::unique_id::UniqueId;


pub struct DeviceRegistryItem
{
    pub unique_id: UniqueId,
    pub factory: fn() -> Box<dyn Device>
}


pub trait Device
{
    fn activate(&mut self);
    fn deactivate(&mut self);
}


impl RegistryItem for DeviceRegistryItem
{
    fn unique_id(&self) -> UniqueId
    {
        self.unique_id.clone()
    }

    fn name(&self) -> String
    {
        t!(format!("devices.{}.name", self.unique_id.as_str()).as_str()).to_string()
    }
}



pub fn register(registry: &mut Registry<DeviceRegistryItem>)
{
    emulator::register(registry);
    serial_min::register(registry);
}