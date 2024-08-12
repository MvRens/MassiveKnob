use relm4::Component;
use settingswidget::SerialMinSettingsInit;
use settingswidget::SerialMinSettingsWidget;

use crate::registry::Registry;
use crate::util::unique_id::UniqueId;
use super::Device;
use super::DeviceRegistryItem;


pub mod settingswidget;


pub struct SerialMinDevice
{

}


impl Device for SerialMinDevice
{
}


pub fn register(registry: &mut Registry<DeviceRegistryItem>)
{
    registry.register(DeviceRegistryItem {
        unique_id: UniqueId::from("serial_min"),
        factory: || Box::new(SerialMinDevice {}),
        settings_widget_factory: ||
        {
            let builder = SerialMinSettingsWidget::builder();
            Some(Box::new(builder.launch(SerialMinSettingsInit
            {
            })))
        }
    });
}