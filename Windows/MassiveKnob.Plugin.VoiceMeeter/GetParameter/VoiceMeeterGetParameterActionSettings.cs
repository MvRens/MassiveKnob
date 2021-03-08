using MassiveKnob.Plugin.VoiceMeeter.Base;

namespace MassiveKnob.Plugin.VoiceMeeter.GetParameter
{
    public class VoiceMeeterGetParameterActionSettings : BaseVoiceMeeterSettings
    {
        public string Parameter { get; set; }
        public string Value { get; set; }
        public bool Inverted { get; set; }
    }
}
