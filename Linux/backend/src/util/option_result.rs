use std::fmt::Display;


pub enum OptionResult<T, E: Display> 
{
    None,
    Some(T),
    Err(E),
}


impl<T, E: Display> OptionResult<T, E>
{
    pub fn unwrap(self) -> Option<T>
    {
        match self
        {
            OptionResult::None => None,
            OptionResult::Some(v) => Some(v),
            OptionResult::Err(e) => panic!("called `OptionResult::unwrap()` on an `Err` value: {e}"),
        }
    }

    pub fn expect(self, msg: &str) -> Option<T>
    {
        match self 
        {
            OptionResult::None => None,
            OptionResult::Some(v) => Some(v),
            OptionResult::Err(e) => panic!("{msg}: {e}"),
        }
    }
}