using System.Windows;

namespace MassiveKnob.Plugin.RunProgram.RunProgram
{
    /// <summary>
    /// Interaction logic for RunProgramSettingsView.xaml
    /// </summary>
    public partial class RunProgramSettingsView
    {
        public RunProgramSettingsView(RunProgramSettingsViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void ButtonBrowseClick(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = Strings.FilenameDialogFilter
            };

            if (dialog.ShowDialog().GetValueOrDefault())
                ((RunProgramSettingsViewModel) DataContext).Filename = dialog.FileName;
        }
    }
}
