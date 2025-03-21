use super::validated_string::{ValidatedString, ValidatedStringPattern};


pub type ReadableKey = ValidatedString<ReadableKeyPattern>;


#[derive(PartialEq, Eq)]
pub struct ReadableKeyPattern;


impl ValidatedStringPattern for ReadableKeyPattern
{
    fn pattern() -> &'static str { r"^[a-zA-Z0-9\.\-_]+$" }
}