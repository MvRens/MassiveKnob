using MassiveKnob.Plugin.VoiceMeeter.Base;

namespace MassiveKnob.Plugin.VoiceMeeter.GetParameter
{
    public class VoiceMeeterGetParameterActionSettingsViewModel : BaseVoiceMeeterSettingsViewModel<VoiceMeeterGetParameterActionSettings>
    {
        // ReSharper disable UnusedMember.Global - used by WPF Binding
        public string Parameter
        {
            get => Settings.Parameter;
            set
            {
                if (value == Settings.Parameter)
                    return;

                Settings.Parameter = value;
                OnPropertyChanged();
            }
        }
        
        public string Value
        {
            get => Settings.Value;
            set
            {
                if (value == Settings.Value)
                    return;

                Settings.Value = value;
                OnPropertyChanged();
            }
        }
        
        public bool Inverted
        {
            get => Settings.Inverted;
            set
            {
                if (value == Settings.Inverted)
                    return;
                
                Settings.Inverted = value;
                OnPropertyChanged();
            }
        }
        // ReSharper restore UnusedMember.Global


        // ReSharper disable once SuggestBaseTypeForParameter - by design
        public VoiceMeeterGetParameterActionSettingsViewModel(VoiceMeeterGetParameterActionSettings settings) : base(settings)
        {
        }        
    }
}
