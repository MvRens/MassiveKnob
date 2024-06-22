use crate::actions;
use crate::actions::MkAction;
use crate::config::Config;
use crate::devices;
use crate::devices::MkDevice;
use crate::registry::MkRegistry;
use crate::util::unique_id::UniqueId;


pub struct Orchestrator
{
    config: Config,

    device_registry: MkRegistry<MkDevice>,
    action_registry: MkRegistry<MkAction>
}


impl Orchestrator
{
    pub fn new() -> Self
    {
        let config = Config::new();
        //config.get_reader(name)

        let mut device_registry = MkRegistry::new();
        let mut action_registry = MkRegistry::new();

        devices::register(&mut device_registry);
        actions::register(&mut action_registry);

        Self
        {
            config,
            device_registry,
            action_registry
        }
    }


    pub fn current_device(&self) -> Option<&MkDevice>
    {
        let Some(device_id) = &self.config.device_id else { return None };
        self.device_registry.by_id(UniqueId::new(device_id.as_str()))
    }


    pub fn set_current_device_id(&self, id: &str)
    {
        // TODO if changed, unload old device, activate new
        todo!("Store in config");
    }
}