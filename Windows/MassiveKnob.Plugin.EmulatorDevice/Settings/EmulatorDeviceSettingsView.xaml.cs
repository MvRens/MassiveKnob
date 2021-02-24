namespace MassiveKnob.Plugin.EmulatorDevice.Settings
{
    /// <summary>
    /// Interaction logic for EmulatorDeviceSettingsView.xaml
    /// </summary>
    public partial class EmulatorDeviceSettingsView
    {
        public EmulatorDeviceSettingsView(EmulatorDeviceSettingsViewModel settingsViewModel)
        {
            DataContext = settingsViewModel;
            
            InitializeComponent();
        }
    }
}
