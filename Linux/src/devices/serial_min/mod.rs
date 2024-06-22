use crate::registry::MkRegistry;
use crate::util::unique_id::UniqueId;
use super::MkDevice;


pub fn register(registry: &mut MkRegistry<MkDevice>)
{
    registry.register(MkDevice {
        unique_id: UniqueId::new("serial_min")
    });
}