using System;

namespace MassiveKnob.Plugin.VoiceMeeter.GetParameter
{
    /// <summary>
    /// Interaction logic for VoiceMeeterGetParameterActionSettingsView.xaml
    /// </summary>
    public partial class VoiceMeeterGetParameterActionSettingsView : IDisposable
    {
        public VoiceMeeterGetParameterActionSettingsView(VoiceMeeterGetParameterActionSettingsViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        
        public void Dispose()
        {
            (DataContext as VoiceMeeterGetParameterActionSettingsViewModel)?.Dispose();
        }
    }
}