using MassiveKnob.ViewModel;

namespace MassiveKnob.View
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        public SettingsWindow(SettingsViewModel settingsViewModel)
        {
            DataContext = settingsViewModel;
            InitializeComponent();
        }
    }
}
