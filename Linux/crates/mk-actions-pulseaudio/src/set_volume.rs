use mk_core::action::Action;
use mk_core::action::ActionFactory;
use mk_core::action::AnalogInputAction;
use mk_core::types::AnalogValue;

use crate::pulseaudio::client::PulseAudioClient;


pub struct SetVolumeAction
{
    settings: SetVolumeActionSettings
}


pub struct SetVolumeActionSettings
{
    pub device_name: String
}


impl Action for SetVolumeAction
{
    fn display_name() -> String { String::from("Volume") }
    fn description() -> String { String::from("Sets the volume for the selected device to the value of the analog input, regardless of the current default device.") }
}


impl ActionFactory<SetVolumeActionSettings> for SetVolumeAction
{
    fn create(settings: SetVolumeActionSettings) -> Self
    {
        Self
        {
            settings
        }
    }
}


impl AnalogInputAction for SetVolumeAction
{
    async fn update_analog(&self, value: AnalogValue)
    {
        PulseAudioClient::set_volume(&self.settings.device_name, value);
    }
}