namespace MassiveKnob.Plugin.SerialDevice.Settings
{
    /// <summary>
    /// Interaction logic for SerialDeviceSettingsView.xaml
    /// </summary>
    public partial class SerialDeviceSettingsView
    {
        public SerialDeviceSettingsView(SerialDeviceSettingsViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
