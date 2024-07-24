use crate::actions;
use crate::actions::ActionRegistryItem;
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

    device_registry: Registry<DeviceRegistryItem>,
    action_registry: Registry<ActionRegistryItem>,

    settings_name: ConfigName,
    settings: settings::Settings,


    current_device_instance: Option<Box<dyn Device>>
}


impl Orchestrator
{
    pub fn new() -> Self
    {
        let config_manager = ConfigManager::new();

        let settings_name = ConfigName::new("settings");
        let settings = match config_manager.read_json(&settings_name).expect("Error reading settings")
        {
            None => settings::Settings::new(),
            Some(v) => v
        };

        let mut device_registry = Registry::new();
        let mut action_registry = Registry::new();

        devices::register(&mut device_registry);
        actions::register(&mut action_registry);

        Self
        {
            config_manager,

            device_registry,
            action_registry,

            settings_name,
            settings,


            current_device_instance: None
        }
    }


    pub fn initialize(&mut self)
    {
        self.set_current_device(self.current_device_id());
    }


    pub fn finalize(&mut self)
    {
        self.set_current_device(None);
    }


    pub fn devices(&self) -> impl Iterator<Item = &DeviceRegistryItem>
    {
        self.device_registry.iter()
    }


    pub fn current_device_id(&self) -> Option<UniqueId>
    {
        let Some(device_id) = &self.settings.device_id else { return None };
        Some(UniqueId::new(device_id.as_str()))
    }


    pub fn current_device(&self) -> Option<&DeviceRegistryItem>
    {
        let Some(device_id) = self.current_device_id() else { return None };
        self.device_registry.by_id(device_id)
    }


    pub fn set_current_device_id(&mut self, id: UniqueId)
    {
        let new_id = Some(String::from(id.as_str()));
        if new_id == self.settings.device_id { return; }

        self.settings.device_id = new_id;
        self.store_settings();

        self.set_current_device(Some(id));
    }


    fn store_settings(&self)
    {
        if let Err(e) = self.config_manager.write_json(&self.settings_name, &self.settings)
        {
            log::error!("Error writing settings: {e}");
        }
    }


    fn set_current_device(&mut self, id: Option<UniqueId>)
    {
        let prev_device_instance;
        let new_device = match id
        {
            None => None,
            Some(v) => self.device_registry.by_id(v)
        };


        // Replace the device instance
        if let Some(new_device) = new_device
        {
            let mut new_device_instance = (new_device.factory)();
            new_device_instance.activate();

            prev_device_instance = self.current_device_instance.replace(new_device_instance);
        }
        else
        {
            prev_device_instance = self.current_device_instance.take();
        }


        // Deactivate the previous instance
        if let Some(mut prev_device_instance) = prev_device_instance
        {
            prev_device_instance.deactivate();
        }
    }
}