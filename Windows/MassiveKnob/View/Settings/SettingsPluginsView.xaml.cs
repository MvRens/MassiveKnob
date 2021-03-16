using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using MassiveKnob.ViewModel;
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

        private void UrlMouseDown(object sender, MouseButtonEventArgs e)
        {
            var dataContext = ((FrameworkElement) e.Source).DataContext;

            Process.Start(new ProcessStartInfo
            {
                FileName = ((PluginViewModel) dataContext).Url,
                Verb = "open"
            });
        }
    }
}
