use regex::Regex;


pub struct UniqueId
{
    inner: String,
}


impl UniqueId 
{
    pub fn new(id: &str) -> Self
    {
        assert!(is_valid_unique_id(id), "Id '{id}' has invalid characters");
        UniqueId { inner: id.to_string() }
    }

    pub fn as_str(&self) -> &str
    {
        self.inner.as_str()
    }
}


fn is_valid_unique_id(id: &str) -> bool 
{
    let re = Regex::new(r"^[a-zA-Z0-9\.\-_]+$").unwrap();
    re.is_match(id)
}


impl Clone for UniqueId
{
    fn clone(&self) -> Self 
    {
        Self { inner: self.inner.clone() }
    }    
}