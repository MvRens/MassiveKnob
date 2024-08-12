use crate::registry::Registry;
use crate::util::unique_id::UniqueId;
use super::ActionRegistryItem;

pub mod set_volume;


pub fn register(registry: &mut Registry<ActionRegistryItem>)
{
    registry.register(ActionRegistryItem
    {
        unique_id: UniqueId::from("pipewire.set_volume")
    });
}