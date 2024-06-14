use log::info;


pub struct MkDevice
{
    // TODO prepare for i18n by making this a translation key?
    name: &'static str
}


impl MkDevice 
{
    pub fn new(name: &'static str) -> Self
    {
        Self
        {
            name
        }
    }
}


pub fn register_device(action: MkDevice)
{
    info!("Registered device: {}", action.name);
}