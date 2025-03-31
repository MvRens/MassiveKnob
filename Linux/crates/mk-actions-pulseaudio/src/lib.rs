pub mod set_volume;

mod pulseaudio;
mod shared_oneshot;



pub struct PulseAudioDevice
{
    pub name: String,
    pub device_name: String,
    pub display_name: String
}


pub async fn available_output_devices() -> Vec<PulseAudioDevice>
{
    pulseaudio::client::PulseAudioClient::available_output_devices().await
}