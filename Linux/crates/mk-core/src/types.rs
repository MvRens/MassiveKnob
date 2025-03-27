use std::fmt::Display;

/// Represents a value from 0 to 100
#[derive(Copy, Clone)]
pub struct AnalogValue
{
    value: u8
}


impl From<u8> for AnalogValue
{
    fn from(value: u8) -> Self
    {
        AnalogValue { value: value.min(100) }
    }
}


impl From<AnalogValue> for u8
{
    fn from(val: AnalogValue) -> Self
    {
        val.value
    }
}


impl Display for AnalogValue
{
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result
    {
        self.value.fmt(f)
    }
}