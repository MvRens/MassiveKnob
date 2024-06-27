use anyhow::Error;

use crate::util::option_result::OptionResult;
use super::{ConfigManager, ConfigName};

pub trait JsonConfigManager
{
    fn read_json<T>(&self, name: &ConfigName) -> OptionResult<T, Error> where T : serde::de::DeserializeOwned;
    fn write_json<T>(&self, name: &ConfigName, value: &T) -> Result<(), Error> where T : serde::ser::Serialize;
}


impl JsonConfigManager for ConfigManager
{
    fn read_json<T>(&self, name: &ConfigName) -> OptionResult<T, Error> where T : serde::de::DeserializeOwned
    {
        match self.get_reader(&json_config_name(name))
        {
            OptionResult::None => OptionResult::None,

            OptionResult::Some(reader) =>
            {
                match serde_json::from_reader(reader)
                {
                    Ok(v) => OptionResult::Some(v),
                    Err(e) => OptionResult::Err(e.into())
                }
            },

            OptionResult::Err(e) => OptionResult::Err(e)
        }
    }


    fn write_json<T>(&self, name: &ConfigName, value: &T) -> Result<(), Error> where T : serde::ser::Serialize
    {
        match self.get_writer(&json_config_name(name)) 
        {
            Ok(writer) => match serde_json::to_writer(writer, value)
            {
                Ok(_) => Ok(()),
                Err(e) => Err(e.into())
            },

            Err(e) => Err(e.into())
        }
    }
}


#[inline]
fn json_config_name(name: &ConfigName) -> ConfigName
{
    ConfigName::new(format!("{}.json", name.as_str()).as_str())
}