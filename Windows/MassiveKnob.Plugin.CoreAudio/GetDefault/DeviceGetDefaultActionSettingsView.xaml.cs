namespace MassiveKnob.Plugin.CoreAudio.GetDefault
{
    /// <summary>
    /// Interaction logic for DeviceGetDefaultActionSettingsView.xaml
    /// </summary>
    public partial class DeviceGetDefaultActionSettingsView
    {
        public DeviceGetDefaultActionSettingsView(DeviceGetDefaultActionSettingsViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}