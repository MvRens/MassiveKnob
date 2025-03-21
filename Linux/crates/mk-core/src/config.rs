use std::io::Read;
use std::io::Write;

use crate::util::option_result::OptionResult;
use crate::util::validated_string::ValidatedString;
use crate::util::validated_string::ValidatedStringPattern;


pub trait ConfigManager
{
    fn get_reader(&self, name: &ConfigName) -> OptionResult<Box<dyn Read>, anyhow::Error>;
    fn get_writer(&self, name: &ConfigName) -> Result<Box<dyn Write>, anyhow::Error>;
}



pub type ConfigName = ValidatedString<ConfigNamePattern>;


pub struct ConfigNamePattern;

impl ValidatedStringPattern for ConfigNamePattern
{
    fn pattern() -> &'static str { r"^[a-zA-Z0-9\.\-_]+$" }
}