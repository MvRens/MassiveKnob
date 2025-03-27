use mk_core::action::Action;
use mk_core::action::AnalogInputAction;
use mk_core::types::AnalogValue;

use crate::pulseaudio_client::PulseAudioClient;


pub struct SetVolumeAction
{
}


impl Action for SetVolumeAction
{
    fn display_name() -> String { String::from("Volume") }
    fn description() -> String { String::from("Sets the volume for the selected device to the value of the analog input, regardless of the current default device.") }
}


impl AnalogInputAction for SetVolumeAction
{
    async fn update_analog(&self, value: AnalogValue)
    {
        PulseAudioClient::call(|p|
        {
            p.set_volume(value);
        })
    }
}