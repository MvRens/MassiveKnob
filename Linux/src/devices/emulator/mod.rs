use emulatorwindow::EmulatorWindow;
use relm4::component::Connector;
use relm4::Component;
use relm4::gtk::prelude::*;
use relm4::ComponentController;

use crate::registry::Registry;
use crate::util::unique_id::UniqueId;
use super::Device;
use super::DeviceRegistryItem;


pub mod emulatorwindow;


pub struct EmulatorWindowDevice
{
    window: Connector<EmulatorWindow>
}


impl EmulatorWindowDevice
{
    fn new() -> Self
    {
        let builder = EmulatorWindow::builder();
        let window = builder.launch({});

        EmulatorWindowDevice
        {
            window
        }
    }
}


impl Device for EmulatorWindowDevice
{
}


impl Drop for EmulatorWindowDevice
{
    fn drop(&mut self)
    {
        self.window.widget().close();
    }
}


pub fn register(registry: &mut Registry<DeviceRegistryItem>)
{
    registry.register(DeviceRegistryItem {
        unique_id: UniqueId::from("emulator"),
        factory: || Box::new(EmulatorWindowDevice::new()),
        settings_widget_factory: || None
    });
}