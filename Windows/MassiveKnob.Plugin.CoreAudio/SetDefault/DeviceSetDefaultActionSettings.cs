using MassiveKnob.Plugin.CoreAudio.Base;

namespace MassiveKnob.Plugin.CoreAudio.SetDefault
{
    public class DeviceSetDefaultActionSettings : BaseDeviceSettings
    {
        public bool Playback { get; set; } = true;
        public bool Communications { get; set; } = true;
    }
}
