use serde::{Serialize, Deserialize};

#[derive(Default, Serialize, Deserialize)]
pub struct Settings
{
    pub device_id: Option<String>
}
