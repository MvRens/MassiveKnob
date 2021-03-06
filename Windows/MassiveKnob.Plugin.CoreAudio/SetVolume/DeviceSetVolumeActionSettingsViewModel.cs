using MassiveKnob.Plugin.CoreAudio.Base;

namespace MassiveKnob.Plugin.CoreAudio.SetVolume
{
    public class DeviceSetVolumeActionSettingsViewModel : BaseDeviceSettingsViewModel<DeviceSetVolumeActionSettings>
    {
        // ReSharper disable once SuggestBaseTypeForParameter - by design
        public DeviceSetVolumeActionSettingsViewModel(DeviceSetVolumeActionSettings settings) : base(settings)
        {
        }
    }
}
