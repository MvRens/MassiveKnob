using MassiveKnob.Plugin.CoreAudio.Base;

namespace MassiveKnob.Plugin.CoreAudio.SetMuted
{
    public class DeviceSetMutedActionSettings : BaseDeviceSettings
    {
        public bool Toggle { get; set; }
        public bool SetInverted { get; set;}
    }
}
