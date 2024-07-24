pub mod pipewire;

use crate::registry::Registry;
use crate::registry::RegistryItem;
use crate::util::unique_id::UniqueId;


pub struct ActionRegistryItem
{
    pub unique_id: UniqueId
}


pub trait Action
{
}


impl RegistryItem for ActionRegistryItem
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



pub fn register(registry: &mut Registry<ActionRegistryItem>)
{
    pipewire::register(registry);
}