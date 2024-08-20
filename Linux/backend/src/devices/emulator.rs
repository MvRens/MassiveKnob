use uuid::Uuid;

use super::DeviceContext;

pub struct EmulatorDevice
{
}


impl EmulatorDevice
{
    pub fn new(_context: DeviceContext, _instance_id: Uuid) -> Self
    {
        Self
        {
        }
    }
}