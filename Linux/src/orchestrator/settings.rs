use serde::{Serialize, Deserialize};

#[derive(Serialize, Deserialize)]
pub struct Settings
{
    pub device_id: Option<String>
}


impl Default for Settings
{
    fn default() -> Self
    {
        Self
        {
            device_id: None
        }
    }
}