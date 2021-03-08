namespace MassiveKnob.Plugin.CoreAudio.GetMuted
{
    /// <summary>
    /// Interaction logic for DeviceGetMutedActionSettingsView.xaml
    /// </summary>
    public partial class DeviceGetMutedActionSettingsView
    {
        public DeviceGetMutedActionSettingsView(DeviceGetMutedActionSettingsViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}