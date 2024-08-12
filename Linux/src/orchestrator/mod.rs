use std::borrow::{Borrow, BorrowMut};
use std::sync::Arc;

use crate::actions;
//use crate::actions::ActionRegistryItem;
use crate::config::json::JsonConfigManager;
use crate::config::{ConfigManager, ConfigName};
use crate::devices::{self, Device};
use crate::devices::DeviceRegistryItem;
use crate::registry::Registry;
use crate::util::unique_id::UniqueId;


mod settings;


pub struct Orchestrator
{
    config_manager: ConfigManager,
    settings_name: ConfigName,

    device_registry: Registry<DeviceRegistryItem>,
    //action_registry: Registry<ActionRegistryItem>,

//    current_device_instance: Option<Box<dyn Device>>
    active_device: Option<ActiveDevice>
}


impl Orchestrator
{
    pub fn new() -> Self
    {
        let config_manager = ConfigManager::new();
        let settings_name = ConfigName::new("settings");
        let settings = match config_manager.read_json(&settings_name).expect("Error reading settings")
        {
            None => settings::Settings::default(),
            Some(v) => v
        };


        let mut device_registry = Registry::new();
        let mut action_registry = Registry::new();

        devices::register(&mut device_registry);
        actions::register(&mut action_registry);

        let mut instance = Self
        {
            config_manager,
            settings_name,

            device_registry,
            //action_registry,

            //settings,


            active_device: None
        };


        instance.initialize(settings);
        instance
    }


    fn initialize(&mut self, settings: settings::Settings)
    {
        if let Some(device_id) = settings.device_id
        {
            self.set_active_device(Some(UniqueId::from(device_id)));
        }
    }


    pub fn devices(&self) -> impl Iterator<Item = &DeviceRegistryItem>
    {
        self.device_registry.iter()
    }


    pub fn active_device_id(&self) -> Option<UniqueId>
    {
        let Some(active_device) = &self.active_device else { return None };
        Some(active_device.id.clone())
    }

    /*


    pub fn current_device(&self) -> Option<&DeviceRegistryItem>
    {
        let Some(device_id) = self.current_device_id() else { return None };
        self.device_registry.by_id(device_id)
    }
    */


    pub fn with_active_device<F>(&self, callback: F) where F: FnOnce(&dyn Device)
    {
        let Some(active_device) = self.active_device else { return };
        let instance = active_device.instance.clone();

        callback(instance.as_ref().as_ref());

        self.active_device.as_ref().map(|device| callback(&*device.instance.clone()));
    }



    pub fn set_active_device_id<F>(&mut self, id: &UniqueId, on_changed: F) where F: FnOnce(&dyn Device)
    {
        let id = Some(id.clone());
        if id == self.active_device_id() { return }

        self.set_active_device(id);
        self.store_settings();

        self.with_active_device(on_changed);
    }



    fn store_settings(&self)
    {
        let settings = settings::Settings
        {
            device_id: match &self.active_device
            {
                None => None,
                Some(v) => Some(v.id.clone().into())
            }
        };

        if let Err(e) = self.config_manager.write_json(&self.settings_name, &settings)
        {
            log::error!("Error writing settings: {e}");
        }
    }



    fn set_active_device(&mut self, id: Option<UniqueId>)
    {
        self.active_device = match id
        {
            None => None,
            Some(v) =>
                match self.device_registry.by_id(&v)
                {
                    None => None,
                    Some(d) =>
                    {
                        Some(ActiveDevice
                        {
                            id: v.clone(),
                            instance: Arc::new((d.factory)())
                        })
                    }
                }
        };
    }
}


impl Drop for Orchestrator
{
    fn drop(&mut self)
    {
        self.set_active_device(None);
    }
}



struct ActiveDevice
{
    id: UniqueId,
    instance: Arc<Box<dyn Device>>
}