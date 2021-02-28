using MassiveKnob.Plugin.CoreAudio.Base;

namespace MassiveKnob.Plugin.CoreAudio.SetMuted
{
    public class DeviceSetMutedActionSettingsViewModel : BaseDeviceSettingsViewModel<DeviceSetMutedActionSettings>
    {
        // ReSharper disable UnusedMember.Global - used by WPF Binding
        public bool ToggleTrue
        {
            get => Settings.Toggle;
            set
            {
                if (!value)
                    return;
                
                if (Settings.Toggle)
                    return;

                Settings.Toggle = true;
                OnPropertyChanged();
            }
        }


        public bool ToggleFalse
        {
            get => !Settings.Toggle;
            set
            {
                if (!value)
                    return;
                
                if (!Settings.Toggle)
                    return;

                Settings.Toggle = false;
                OnPropertyChanged();
            }
        }

        
        public bool SetInverted
        {
            get => Settings.SetInverted;
            set
            {
                if (value == Settings.SetInverted)
                    return;

                Settings.SetInverted = value;
                OnPropertyChanged();
            }
        }
        // ReSharper restore UnusedMember.Global

        
        // ReSharper disable once SuggestBaseTypeForParameter - by design
        public DeviceSetMutedActionSettingsViewModel(DeviceSetMutedActionSettings settings) : base(settings)
        {
        }
    }
}
