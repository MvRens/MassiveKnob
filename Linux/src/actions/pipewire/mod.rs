use crate::registry::MkRegistry;
use crate::util::unique_id::UniqueId;
use super::MkAction;

pub mod set_volume;


pub fn register(registry: &mut MkRegistry<MkAction>)
{
    registry.register(MkAction 
    {
        unique_id: UniqueId::new("pipewire.set_volume")
    });
}