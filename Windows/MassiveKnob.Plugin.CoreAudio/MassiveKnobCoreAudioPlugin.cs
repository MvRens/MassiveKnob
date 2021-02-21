using System;
using System.Collections.Generic;
using MassiveKnob.Plugin.CoreAudio.Actions;

namespace MassiveKnob.Plugin.CoreAudio
{
    [MassiveKnobPlugin]
    public class MassiveKnobCoreAudioPlugin : IMassiveKnobActionPlugin
    {
        public Guid PluginId { get; } = new Guid("eaa5d3f8-8f9b-4a4b-8e29-827228d23e95");
        public string Name { get; } = "Windows Core Audio";
        public string Description { get; } = "Included with Massive Knob by default. Provides volume control per device and default device switching.";
        public string Author { get; } = "Mark van Renswoude <mark@x2software.net>";
        public string Url { get; } = "https://www.github.com/MvRens/MassiveKnob/";

        public IEnumerable<IMassiveKnobAction> Actions { get; } = new IMassiveKnobAction[]
        {
            new DeviceVolumeAction()
        };
    }
}
