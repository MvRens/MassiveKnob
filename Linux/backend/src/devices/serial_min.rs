use uuid::Uuid;

use super::DeviceContext;


pub struct SerialMinDevice
{
    port: String
}


pub struct SerialMinDeviceSettings
{
    pub port: String
}


impl SerialMinDevice
{
    pub fn new(_context: DeviceContext, _instance_id: Uuid) -> Self
    {
        // TODO read settings
        let port = String::from("/dev/ttyACM0");


        Self
        {
            port
        }
    }


    pub fn get_settings(&self) -> SerialMinDeviceSettings
    {
        SerialMinDeviceSettings
        {
            port: self.port.clone()
        }
    }


    pub fn update_settings(&mut self, settings: SerialMinDeviceSettings)
    {
        self.port = settings.port;
    }
}