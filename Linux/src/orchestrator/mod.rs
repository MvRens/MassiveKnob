use crate::actions;
use crate::actions::MkAction;
use crate::config::json::JsonConfigManager;
use crate::config::{ConfigManager, ConfigName};
use crate::devices;
use crate::devices::MkDevice;
use crate::registry::MkRegistry;
use crate::util::unique_id::UniqueId;


mod settings;


pub struct Orchestrator
{
    config_manager: ConfigManager,

    device_registry: MkRegistry<MkDevice>,
    action_registry: MkRegistry<MkAction>,

    settings_name: ConfigName,
    settings: settings::Settings
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

        let mut device_registry = MkRegistry::new();
        let mut action_registry = MkRegistry::new();

        devices::register(&mut device_registry);
        actions::register(&mut action_registry);

        Self
        {
            config_manager,

            device_registry,
            action_registry,

            settings_name,
            settings
        }
    }


    pub fn current_device(&self) -> Option<&MkDevice>
    {
        let Some(device_id) = &self.settings.device_id else { return None };
        self.device_registry.by_id(UniqueId::new(device_id.as_str()))
    }


    pub fn set_current_device_id(&mut self, id: &str)
    {
        let new_id = Some(String::from(id));
        if new_id == self.settings.device_id { return; }

        self.settings.device_id = new_id;
        self.store_settings();

        // TODO unload old device, activate new
    }


    fn store_settings(&self)
    {
        if let Err(e) = self.config_manager.write_json(&self.settings_name, &self.settings)
        {
            log::error!("Error writing settings: {e}");
        }
    }
}