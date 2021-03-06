namespace MassiveKnob.Plugin.EmulatorDevice.Devices
{
    /// <summary>
    /// Interaction logic for EmulatorDeviceWindow.xaml
    /// </summary>
    public partial class EmulatorDeviceWindow
    {
        public EmulatorDeviceWindow(EmulatorDeviceWindowViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
