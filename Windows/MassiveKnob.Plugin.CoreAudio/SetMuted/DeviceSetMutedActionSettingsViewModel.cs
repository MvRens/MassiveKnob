using MassiveKnob.Plugin.CoreAudio.Base;

namespace MassiveKnob.Plugin.CoreAudio.SetMuted
{
    public class DeviceSetMutedActionSettingsViewModel : BaseDeviceSettingsViewModel<DeviceSetMutedActionSettings>
    {
        // ReSharper disable UnusedMember.Global - used by WPF Binding
        public bool Toggle
        {
            get => Settings.Toggle;
            set
            {
                if (value == Settings.Toggle)
                    return;

                Settings.Toggle = value;
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
