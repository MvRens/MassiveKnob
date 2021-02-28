namespace MassiveKnob.Plugin.CoreAudio.GetVolume
{
    /// <summary>
    /// Interaction logic for DeviceGetVolumeActionSettingsView.xaml
    /// </summary>
    public partial class DeviceGetVolumeActionSettingsView
    {
        public DeviceGetVolumeActionSettingsView(DeviceGetVolumeActionSettingsViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}