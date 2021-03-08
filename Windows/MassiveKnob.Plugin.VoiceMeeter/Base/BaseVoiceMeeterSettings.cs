using Voicemeeter;

namespace MassiveKnob.Plugin.VoiceMeeter.Base
{
    public class BaseVoiceMeeterSettings
    {
        public RunVoicemeeterParam Version
        {
            get => InstanceRegister.Version;
            set => InstanceRegister.Version = value;
        }
    }
}
