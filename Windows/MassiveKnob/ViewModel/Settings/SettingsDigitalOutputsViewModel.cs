using MassiveKnob.Core;
using MassiveKnob.Plugin;

namespace MassiveKnob.ViewModel.Settings
{
    public class SettingsDigitalOutputsViewModel : BaseSettingsInputOutputViewModel
    {
        public SettingsDigitalOutputsViewModel(IPluginManager pluginManager, IMassiveKnobOrchestrator orchestrator) 
            : base(pluginManager, orchestrator, MassiveKnobActionType.OutputDigital)
        {
        }
    }
}
