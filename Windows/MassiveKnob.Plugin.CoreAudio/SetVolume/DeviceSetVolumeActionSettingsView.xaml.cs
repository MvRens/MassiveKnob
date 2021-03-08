namespace MassiveKnob.Plugin.CoreAudio.SetVolume
{
    /// <summary>
    /// Interaction logic for DeviceSetVolumeActionSettingsView.xaml
    /// </summary>
    public partial class DeviceSetVolumeActionSettingsView
    {
        public DeviceSetVolumeActionSettingsView(DeviceSetVolumeActionSettingsViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
