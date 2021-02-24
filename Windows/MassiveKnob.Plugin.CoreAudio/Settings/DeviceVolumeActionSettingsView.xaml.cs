namespace MassiveKnob.Plugin.CoreAudio.Settings
{
    /// <summary>
    /// Interaction logic for DeviceVolumeActionSettingsView.xaml
    /// </summary>
    public partial class DeviceVolumeActionSettingsView
    {
        public DeviceVolumeActionSettingsView(DeviceVolumeActionSettingsViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
