using MassiveKnob.Plugin.CoreAudio.Base;

namespace MassiveKnob.Plugin.CoreAudio.SetDefault
{
    public class DeviceSetDefaultActionSettingsViewModel : BaseDeviceSettingsViewModel<DeviceSetDefaultActionSettings>
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
        // ReSharper restore UnusedMember.Global


        // ReSharper disable once SuggestBaseTypeForParameter - by design
        public DeviceSetDefaultActionSettingsViewModel(DeviceSetDefaultActionSettings settings) : base(settings)
        {
        }
    }
}
