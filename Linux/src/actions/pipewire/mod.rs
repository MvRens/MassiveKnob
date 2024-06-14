use super::registry::{MkAction, register_action};


pub mod set_volume;


pub fn register()
{
    register_action(MkAction::new("Set volume"));
}