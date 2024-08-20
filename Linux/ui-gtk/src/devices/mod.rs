use emulator_settings_ui::EmulatorSettingsUi;
use emulator_settings_ui::EmulatorSettingsUiInit;
use massiveknob_backend::devices::Device;
use massiveknob_backend::orchestrator::DeviceReference;
use serial_min_settings_ui::SerialMinSettingsUi;
use serial_min_settings_ui::SerialMinSettingsUiInit;

use crate::ui::uicomponent::UiComponent;
use crate::ui::uicomponent::UiComponentConnectorWidget;

pub mod emulator_settings_ui;
pub mod serial_min_settings_ui;


pub struct DeviceSettingsUiBuilder
{
}


impl DeviceSettingsUiBuilder
{
    pub fn build(device: DeviceReference) -> Box<dyn UiComponentConnectorWidget>
    {
        match device.as_ref()
        {
            Device::Emulator(_) => Box::new(EmulatorSettingsUi::builder().build(EmulatorSettingsUiInit { device })),
            Device::SerialMin(_) => Box::new(SerialMinSettingsUi::builder().build(SerialMinSettingsUiInit { device }))
        }
    }
}