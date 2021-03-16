using MassiveKnob.ViewModel.Settings;

namespace MassiveKnob.View.Settings
{
    /// <summary>
    /// Interaction logic for BaseSettingsInputOutputView.xaml
    /// </summary>
    public partial class BaseSettingsInputOutputView
    {
        public BaseSettingsInputOutputView(BaseSettingsInputOutputViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
