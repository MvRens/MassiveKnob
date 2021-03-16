using System;
using System.Collections.Generic;
using MassiveKnob.Plugin.RunProgram.RunProgram;

namespace MassiveKnob.Plugin.RunProgram
{
    [MassiveKnobPlugin]
    public class MassiveKnobRunProgramPlugin : IMassiveKnobActionPlugin
    {
        public Guid PluginId { get; } = new Guid("10537f2a-6876-48b8-8ef9-8d05f185fa62");
        public string Name { get; } = Strings.PluginName;
        public string Description { get; } = Strings.PluginDescription;
        public string Author { get; } = "Mark van Renswoude <mark@x2software.net>";
        public string Url { get; } = "https://www.github.com/MvRens/MassiveKnob/";

        public IEnumerable<IMassiveKnobAction> Actions { get; } = new IMassiveKnobAction[]
        {
            new RunProgramAction()
        };
    }
}
