using MassiveKnob.Core;
using MassiveKnob.Plugin;

namespace MassiveKnob.ViewModel.Settings
{
    public class SettingsAnalogInputsViewModel : BaseSettingsInputOutputViewModel
    {
        public SettingsAnalogInputsViewModel(IPluginManager pluginManager, IMassiveKnobOrchestrator orchestrator) 
            : base(pluginManager, orchestrator, MassiveKnobActionType.InputAnalog)
        {
        }
    }
}
