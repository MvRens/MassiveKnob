using MassiveKnob.ViewModel.Settings;

namespace MassiveKnob.View.Settings
{
    /// <summary>
    /// Interaction logic for SettingsLoggingView.xaml
    /// </summary>
    public partial class SettingsLoggingView
    {
        public SettingsLoggingView(SettingsLoggingViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
