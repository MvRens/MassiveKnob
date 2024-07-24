use emulatorwindow::EmulatorWindow;
use relm4::{component::Connector, ComponentController};
use relm4::Component;
use relm4::gtk::prelude::*;

use crate::registry::Registry;
use crate::util::unique_id::UniqueId;
use super::{Device, DeviceRegistryItem};


pub mod emulatorwindow;


pub struct EmulatorWindowDevice
{
    window: Option<Connector<EmulatorWindow>>
}


impl EmulatorWindowDevice
{
    fn new() -> Self
    {
        EmulatorWindowDevice
        {
            window: None
        }
    }
}


impl Device for EmulatorWindowDevice
{
    fn activate(&mut self)
    {
        if self.window.is_some() { return }

        let builder = EmulatorWindow::builder();
        self.window = Some(builder.launch({}));
    }


    fn deactivate(&mut self)
    {
        let Some(window) = self.window.take() else { return };

        window.widget().close();
    }
}


pub fn register(registry: &mut Registry<DeviceRegistryItem>)
{
    registry.register(DeviceRegistryItem {
        unique_id: UniqueId::new("emulator"),
        factory: || Box::new(EmulatorWindowDevice::new())
    });
}