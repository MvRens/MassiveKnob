using MassiveKnob.ViewModel.Settings;

namespace MassiveKnob.View.Settings
{
    /// <summary>
    /// Interaction logic for SettingsDeviceView.xaml
    /// </summary>
    public partial class SettingsDeviceView
    {
        public SettingsDeviceView(SettingsDeviceViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
