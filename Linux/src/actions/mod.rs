pub mod registry;
pub mod pipewire;


pub fn register()
{
    pipewire::register();
}