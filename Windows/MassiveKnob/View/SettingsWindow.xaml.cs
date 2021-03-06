using MassiveKnob.ViewModel;

namespace MassiveKnob.View
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        // ReSharper disable once SuggestBaseTypeForParameter - for clarity
        public SettingsWindow(SettingsViewModel settingsViewModel)
        {
            DataContext = settingsViewModel;
            InitializeComponent();

            Closed += (sender, args) =>
            {
                settingsViewModel.Dispose();
            };
        }
    }
}
