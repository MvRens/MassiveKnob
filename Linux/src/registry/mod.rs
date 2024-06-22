use std::collections::HashMap;
use log::info;

use crate::util::unique_id::UniqueId;


pub trait RegistryItem
{
    /// The unique ID of the item. This should remain stable across releases for
    /// the purpose of storing it in the user's configuration.
    fn unique_id(&self) -> UniqueId;

    /// The name of the item for display purposes.
    fn name(&self) -> String;
}



pub struct MkRegistry<T> where T: RegistryItem
{
    items: HashMap<String, T>
}



impl<'a, T> MkRegistry<T> where T: RegistryItem
{
    pub fn new() -> Self
    {
        Self
        {
            items: HashMap::new()
        }
    }


    pub fn register(&mut self, device: T)
    {    
        let device_id = device.unique_id();        

        info!("Registered device: [{}] {}", device_id.as_str(), device.name());
        self.items.insert(String::from(device_id.as_str()), device);
    }


    pub fn by_id(&self, id: UniqueId) -> Option<&T>
    {
        self.items.get(id.as_str())
    }
}