use std::sync::Arc;

use uuid::Uuid;

use crate::config::json::JsonConfigManager;
use crate::config::ConfigName;
use crate::config::ConfigManager;
use crate::devices::DeviceContext;
use crate::devices::DeviceInfo;
use crate::devices::DeviceReference;
use crate::devices::DeviceRegistry;
use crate::util::unique_id::UniqueId;


mod settings;


pub struct Orchestrator
{
    config_manager: Arc<ConfigManager>,
    settings_name: ConfigName,

    device_registry: DeviceRegistry,

    active_device: Option<ActiveDevice>
}


impl Orchestrator
{
    #![allow(clippy::new_without_default)]
    pub fn new() -> Self
    {
        let config_manager = Arc::new(ConfigManager::new());
        let settings_name = ConfigName::new("settings");
        let settings = config_manager.read_json(&settings_name).expect("Error reading settings").unwrap_or_default();


        let mut instance = Self
        {
            config_manager,
            settings_name,

            device_registry: DeviceRegistry::new(),
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


    pub fn devices(&self) -> impl Iterator<Item = &DeviceInfo>
    {
        self.device_registry.iter()
    }


    pub fn active_device_id(&self) -> Option<UniqueId>
    {
        let Some(active_device) = &self.active_device else { return None };
        Some(active_device.id.clone())
    }


    pub fn active_device(&self) -> Option<DeviceReference>
    {
        let Some(active_device) = &self.active_device else { return None };
        let instance = active_device.instance.clone();

        Some(instance)
    }


    pub fn set_active_device_id(&mut self, id: &UniqueId) -> DeviceReference
    {
        if let Some(active_device) = &self.active_device
        {
            if *id == active_device.id
            {
                return active_device.instance.clone();
            }
        }


        let active_device = self.set_active_device(Some(id.clone())).unwrap_or_else(|| panic!("Invalid device ID: {}", id.as_str()));
        self.store_settings();

        active_device
    }



    fn store_settings(&self)
    {
        let settings = settings::Settings
        {
            device_id: self.active_device.as_ref().map(|v| v.id.clone().into())
        };

        if let Err(e) = self.config_manager.write_json(&self.settings_name, &settings)
        {
            log::error!("Error writing settings: {e}");
        }
    }



    fn set_active_device(&mut self, id: Option<UniqueId>) -> Option<DeviceReference>
    {
        let new_device = match id
        {
            None => None,
            Some(v) =>
                match self.device_registry.by_id(&v)
                {
                    None => None,
                    Some(d) =>
                    {
                        let context = DeviceContext
                        {
                            config_manager: self.config_manager.clone()
                        };

                        let instance_id = Uuid::new_v4();

                        Some(ActiveDevice
                        {
                            id: v.clone(),
                            instance: Arc::new((d.factory)(context, instance_id))
                        })
                    }
                }
        };

        let result = new_device.as_ref().map(|v| v.instance.clone());

        self.active_device = new_device;
        result
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
    instance: DeviceReference
}