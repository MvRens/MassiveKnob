namespace MassiveKnob.Plugin.VoiceMeeter.RunMacro
{
    /// <summary>
    /// Interaction logic for VoiceMeeterRunMacroActionSettingsView.xaml
    /// </summary>
    public partial class VoiceMeeterRunMacroActionSettingsView
    {
        public VoiceMeeterRunMacroActionSettingsView(VoiceMeeterRunMacroActionSettingsViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
