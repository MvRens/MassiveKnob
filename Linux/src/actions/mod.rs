pub mod pipewire;

use crate::registry::MkRegistry;
use crate::registry::RegistryItem;
use crate::util::unique_id::UniqueId;


pub struct MkAction
{
    pub unique_id: UniqueId
}


impl RegistryItem for MkAction
{
    fn unique_id(&self) -> UniqueId
    {
        self.unique_id.clone()
    }

    fn name(&self) -> String
    {
        t!(format!("actions.{}.name", self.unique_id.as_str()).as_str()).to_string()
    }
}



pub fn register(registry: &mut MkRegistry<MkAction>)
{
    pipewire::register(registry);
}