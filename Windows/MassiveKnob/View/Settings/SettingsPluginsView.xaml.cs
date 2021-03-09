using MassiveKnob.ViewModel.Settings;

namespace MassiveKnob.View.Settings
{
    /// <summary>
    /// Interaction logic for SettingsPluginsView.xaml
    /// </summary>
    public partial class SettingsPluginsView
    {
        public SettingsPluginsView(SettingsPluginsViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
