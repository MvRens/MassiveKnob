using MassiveKnob.Core;
using MassiveKnob.Plugin;

namespace MassiveKnob.ViewModel.Settings
{
    public class SettingsDigitalInputsViewModel : BaseSettingsInputOutputViewModel
    {
        public SettingsDigitalInputsViewModel(IPluginManager pluginManager, IMassiveKnobOrchestrator orchestrator) 
            : base(pluginManager, orchestrator, MassiveKnobActionType.InputDigital)
        {
        }
    }
}
