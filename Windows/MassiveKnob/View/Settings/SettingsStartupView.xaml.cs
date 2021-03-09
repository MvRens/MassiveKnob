using MassiveKnob.ViewModel.Settings;

namespace MassiveKnob.View.Settings
{
    /// <summary>
    /// Interaction logic for SettingsStartupView.xaml
    /// </summary>
    public partial class SettingsStartupView
    {
        public SettingsStartupView(SettingsStartupViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
