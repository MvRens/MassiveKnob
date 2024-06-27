use super::validated_string::{ValidatedString, ValidatedStringPattern};


pub type UniqueId = ValidatedString<UniqueIdPattern>;


pub struct UniqueIdPattern;

impl ValidatedStringPattern for UniqueIdPattern
{
    fn pattern() -> &'static str { r"^[a-zA-Z0-9\.\-_]+$" }
}