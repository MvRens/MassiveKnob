use std::marker::PhantomData;
use regex::Regex;


/// A string which must conform to the specified regex pattern,
/// otherwise it will panic by design. Intended for code validation,
/// not for runtime input validation.
#[derive(PartialEq, Eq)]
pub struct ValidatedString<T: ValidatedStringPattern>
{
    inner: String,

    // Satisfy the compiler's demand to use T
    _phantom: std::marker::PhantomData<T>
}


impl<T: ValidatedStringPattern> ValidatedString<T>
{
    pub fn new(value: &str) -> Self
    {
        let pattern = Regex::new(T::pattern()).unwrap();
        assert!(pattern.is_match(value), "Value '{value}' has invalid characters");

        Self
        {
            inner: value.to_string(),
            _phantom: PhantomData
        }
    }


    pub fn as_str(&self) -> &str
    {
        self.inner.as_str()
    }
}


impl<T: ValidatedStringPattern> Clone for ValidatedString<T>
{
    fn clone(&self) -> Self
    {
        Self
        {
            inner: self.inner.clone(),
            _phantom: PhantomData
        }
    }
}


impl<T: ValidatedStringPattern> From<&str> for ValidatedString<T>
{
    fn from(value: &str) -> Self
    {
        Self::new(value)
    }
}


impl<T: ValidatedStringPattern> From<String> for ValidatedString<T>
{
    fn from(value: String) -> Self
    {
        Self::new(value.as_str())
    }
}


impl<T: ValidatedStringPattern> Into<String> for ValidatedString<T>
{
    fn into(self) -> String
    {
        self.inner.clone()
    }
}


pub trait ValidatedStringPattern
{
    fn pattern() -> &'static str;
}