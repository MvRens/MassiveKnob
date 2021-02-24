namespace MassiveKnob.Plugin.MockDevice.Settings
{
    /// <summary>
    /// Interaction logic for MockDeviceSettingsView.xaml
    /// </summary>
    public partial class MockDeviceSettingsView
    {
        public MockDeviceSettingsView(MockDeviceSettingsViewModel settingsViewModel)
        {
            DataContext = settingsViewModel;
            
            InitializeComponent();
        }
    }
}
