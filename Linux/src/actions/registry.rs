use log::info;


pub struct MkAction
{
    // TODO prepare for i18n by making this a translation key?
    name: &'static str
}


impl MkAction 
{
    pub fn new(name: &'static str) -> Self
    {
        Self
        {
            name
        }
    }
}


pub fn register_action(action: MkAction)
{
    info!("Registered action: {}", action.name);
}