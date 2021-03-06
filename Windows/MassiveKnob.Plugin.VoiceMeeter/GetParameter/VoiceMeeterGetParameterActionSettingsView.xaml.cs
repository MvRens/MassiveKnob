namespace MassiveKnob.Plugin.VoiceMeeter.GetParameter
{
    /// <summary>
    /// Interaction logic for VoiceMeeterGetParameterActionSettingsView.xaml
    /// </summary>
    public partial class VoiceMeeterGetParameterActionSettingsView
    {
        public VoiceMeeterGetParameterActionSettingsView(VoiceMeeterGetParameterActionSettingsViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}