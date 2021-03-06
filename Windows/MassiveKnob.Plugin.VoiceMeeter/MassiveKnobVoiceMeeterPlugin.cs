using System;
using System.Collections.Generic;
using MassiveKnob.Plugin.VoiceMeeter.GetParameter;
using MassiveKnob.Plugin.VoiceMeeter.RunMacro;

namespace MassiveKnob.Plugin.VoiceMeeter
{
    [MassiveKnobPlugin]
    public class MassiveKnobVoiceMeeterPlugin : IMassiveKnobActionPlugin
    {
        public Guid PluginId { get; } = new Guid("cf6634f1-97e3-4a18-a4aa-289b558c0e82");
        public string Name { get; } = Strings.PluginName;
        public string Description { get; } = Strings.PluginDescription;
        public string Author { get; } = "Mark van Renswoude <mark@x2software.net>";
        public string Url { get; } = "https://www.github.com/MvRens/MassiveKnob/";

        public IEnumerable<IMassiveKnobAction> Actions { get; } = new IMassiveKnobAction[]
        {
            new VoiceMeeterRunMacroAction(),
            new VoiceMeeterGetParameterAction()
        };
    }
}
