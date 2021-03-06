using MassiveKnob.Plugin.CoreAudio.Base;

namespace MassiveKnob.Plugin.CoreAudio.GetVolume
{
    public class DeviceGetVolumeActionSettingsViewModel : BaseDeviceSettingsViewModel<DeviceGetVolumeActionSettings>
    {
        // ReSharper disable once SuggestBaseTypeForParameter - by design
        public DeviceGetVolumeActionSettingsViewModel(DeviceGetVolumeActionSettings settings) : base(settings)
        {
        }
    }
}
