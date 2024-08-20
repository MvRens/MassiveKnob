use uuid::Uuid;

use super::DeviceContext;


pub struct SerialMinDevice
{
}


impl SerialMinDevice
{
    pub fn new(_context: DeviceContext, _instance_id: Uuid) -> Self
    {
        Self
        {
        }
    }
}