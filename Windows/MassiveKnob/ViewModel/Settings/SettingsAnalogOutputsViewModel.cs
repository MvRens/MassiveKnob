using MassiveKnob.Core;
using MassiveKnob.Plugin;

namespace MassiveKnob.ViewModel.Settings
{
    public class SettingsAnalogOutputsViewModel : BaseSettingsInputOutputViewModel
    {
        public SettingsAnalogOutputsViewModel(IPluginManager pluginManager, IMassiveKnobOrchestrator orchestrator) 
            : base(pluginManager, orchestrator, MassiveKnobActionType.OutputAnalog)
        {
        }
    }
}
