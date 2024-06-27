use serde::{Serialize, Deserialize};

#[derive(Serialize, Deserialize)]
pub struct Settings
{
    pub device_id: Option<String>
}


impl Settings
{
    pub fn new() -> Self
    {
        Self 
        {
            device_id: None
        }
    }
}