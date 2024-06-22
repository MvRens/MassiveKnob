use std::path::{Path, PathBuf};
use std::io::{Error, Read, Write};
use platform_dirs::AppDirs;

#[derive(Debug)]
pub struct Config
{
    root: PathBuf,
    pub device_id: Option<String>
}


impl Config
{
    pub fn new() -> Self
    {
        let appdirs = AppDirs::new(Some("massiveknob"), false).unwrap();

        Self
        {
            root: appdirs.data_dir,
            device_id: None
        }
    }


    pub fn get_reader(&self, name: &str) -> Option<impl Read>
    {
        let path = Path::join(&self.root, name);
        if !path.exists()
        {
            return None;
        }
        
        match std::fs::File::open(path)
        {
            Ok(v) => Some(v),
            Err(_) => None
        }
    }


    pub fn get_writer(&self, name: &str) -> Result<impl Write, Error>
    {
        let path = Path::join(&self.root, name);
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
