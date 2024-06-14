pub mod registry;
pub mod emulator;
pub mod serial_min;


pub fn register()
{
    emulator::register();
    serial_min::register();
}