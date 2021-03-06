using MassiveKnob.Plugin.CoreAudio.Base;

namespace MassiveKnob.Plugin.CoreAudio.GetDefault
{
    public class DeviceGetDefaultActionSettings : BaseDeviceSettings
    {
        public bool Playback { get; set; } = true;
        public bool Communications { get; set; } = true;

        public bool Inverted { get; set; }
    }
}
