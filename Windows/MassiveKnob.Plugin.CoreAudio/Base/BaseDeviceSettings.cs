using System;

namespace MassiveKnob.Plugin.CoreAudio.Base
{
    public class BaseDeviceSettings
    {
        public Guid? DeviceId { get; set; }
        
        // TODO more options, like positioning and style
        public bool OSD { get; set; } = true;
    }
}
