use std::path::{Path, PathBuf};
use std::io::{Error, Read, Write};
use platform_dirs::AppDirs;

use crate::util::option_result::OptionResult;
use crate::util::validated_string::{ValidatedString, ValidatedStringPattern};


pub mod json;



#[derive(Debug)]
pub struct ConfigManager
{
    root: PathBuf,
}


impl ConfigManager
{
    pub fn new() -> Self
    {
        let appdirs = AppDirs::new(Some("massiveknob"), false).unwrap();

        Self
        {
            root: appdirs.data_dir
        }
    }


    pub fn get_reader(&self, name: &ConfigName) -> OptionResult<impl Read, anyhow::Error>
    {
        let path = Path::join(&self.root, name.as_str());
        if !path.exists()
        {
            return OptionResult::None;
        }
        
        match std::fs::File::open(path)
        {
            Ok(v) => OptionResult::Some(v),
            Err(e) => OptionResult::Err(e.into())
        }
    }


    pub fn get_writer(&self, name: &ConfigName) -> Result<impl Write, Error>
    {
        let path = Path::join(&self.root, name.as_str());
        if !path.exists()
        {
            match std::fs::create_dir_all(path.clone())
            {
                Ok(_v) => (),
                Err(e) => return Err(e)
            }
        }

        std::fs::File::create(path)
    }
}



pub type ConfigName = ValidatedString<ConfigNamePattern>;


pub struct ConfigNamePattern;

impl ValidatedStringPattern for ConfigNamePattern
{
    fn pattern() -> &'static str { r"^[a-zA-Z0-9\.\-_]+$" }
}