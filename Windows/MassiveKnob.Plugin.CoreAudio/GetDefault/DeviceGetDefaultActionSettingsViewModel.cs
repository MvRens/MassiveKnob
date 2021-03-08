using MassiveKnob.Plugin.CoreAudio.Base;

namespace MassiveKnob.Plugin.CoreAudio.GetDefault
{
    public class DeviceGetDefaultActionSettingsViewModel : BaseDeviceSettingsViewModel<DeviceGetDefaultActionSettings>
    {
        // ReSharper disable UnusedMember.Global - used by WPF Binding
        public bool Playback
        {
            get => Settings.Playback;
            set
            {
                if (value == Settings.Playback)
                    return;

                Settings.Playback = value;
                OnPropertyChanged();
            }
        }

        public bool Communications
        {
            get => Settings.Communications;
            set
            {
                if (value == Settings.Communications)
                    return;

                Settings.Communications = value;
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
        public DeviceGetDefaultActionSettingsViewModel(DeviceGetDefaultActionSettings settings) : base(settings)
        {
        }        
    }
}
