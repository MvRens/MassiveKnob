using System;

namespace MassiveKnob.Plugin.SerialDevice.Settings
{
    /// <summary>
    /// Interaction logic for SerialDeviceSettingsView.xaml
    /// </summary>
    public partial class SerialDeviceSettingsView : IDisposable
    {
        public SerialDeviceSettingsView(SerialDeviceSettingsViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        
        public void Dispose()
        {
            (DataContext as SerialDeviceSettingsViewModel)?.Dispose();
        }
    }
}
