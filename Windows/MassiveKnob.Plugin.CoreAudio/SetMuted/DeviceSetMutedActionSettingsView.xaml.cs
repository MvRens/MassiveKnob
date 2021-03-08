namespace MassiveKnob.Plugin.CoreAudio.SetMuted
{
    /// <summary>
    /// Interaction logic for DeviceSetMutedActionSettingsView.xaml
    /// </summary>
    public partial class DeviceSetMutedActionSettingsView
    {
        public DeviceSetMutedActionSettingsView(DeviceSetMutedActionSettingsViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
