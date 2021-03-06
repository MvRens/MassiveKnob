using MassiveKnob.Plugin.VoiceMeeter.Base;

namespace MassiveKnob.Plugin.VoiceMeeter.RunMacro
{
    public class VoiceMeeterRunMacroActionSettingsViewModel : BaseVoiceMeeterSettingsViewModel<VoiceMeeterRunMacroActionSettings>
    {
        // ReSharper disable UnusedMember.Global - used by WPF Bindingpriv
        public string Script
        {
            get => Settings.Script;
            set
            {
                // TODO timer for change notification
                if (value == Settings.Script)
                    return;

                Settings.Script = value;
                OnPropertyChanged();
            }
        }
        // ReSharper restore UnusedMember.Global


        // ReSharper disable once SuggestBaseTypeForParameter - by design
        public VoiceMeeterRunMacroActionSettingsViewModel(VoiceMeeterRunMacroActionSettings settings) : base(settings)
        {
        }
    }
}
