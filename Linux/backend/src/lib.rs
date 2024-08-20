#[macro_use]
extern crate rust_i18n;

i18n!("locales");


pub mod util;
pub mod devices;
pub mod actions;

pub mod config;
pub mod orchestrator;