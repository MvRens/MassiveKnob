namespace MassiveKnob.Plugin.CoreAudio.SetDefault
{
    /// <summary>
    /// Interaction logic for DeviceSetDefaultActionSettingsView.xaml
    /// </summary>
    public partial class DeviceSetDefaultActionSettingsView
    {
        public DeviceSetDefaultActionSettingsView(DeviceSetDefaultActionSettingsViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
