using MassiveKnob.Plugin.CoreAudio.Base;

namespace MassiveKnob.Plugin.CoreAudio.GetMuted
{
    public class DeviceGetMutedActionSettingsViewModel : BaseDeviceSettingsViewModel<DeviceGetMutedActionSettings>
    {
        // ReSharper disable UnusedMember.Global - used by WPF Binding
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
        public DeviceGetMutedActionSettingsViewModel(DeviceGetMutedActionSettings settings) : base(settings)
        {
        }        
    }
}
