using System;

namespace MassiveKnob.Plugin.VoiceMeeter.RunMacro
{
    /// <summary>
    /// Interaction logic for VoiceMeeterRunMacroActionSettingsView.xaml
    /// </summary>
    public partial class VoiceMeeterRunMacroActionSettingsView : IDisposable
    {
        // ReSharper disable once SuggestBaseTypeForParameter
        public VoiceMeeterRunMacroActionSettingsView(VoiceMeeterRunMacroActionSettingsViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        public void Dispose()
        {
            (DataContext as VoiceMeeterRunMacroActionSettingsViewModel)?.Dispose();
        }
    }
}
