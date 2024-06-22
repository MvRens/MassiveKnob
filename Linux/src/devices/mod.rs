pub mod emulator;
pub mod serial_min;


use crate::registry::MkRegistry;
use crate::registry::RegistryItem;
use crate::util::unique_id::UniqueId;


pub struct MkDevice
{
    pub unique_id: UniqueId
}


impl RegistryItem for MkDevice
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



pub fn register(registry: &mut MkRegistry<MkDevice>)
{
    emulator::register(registry);
    serial_min::register(registry);
}