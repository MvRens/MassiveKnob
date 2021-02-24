using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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


        public MassiveKnobCoreAudioPlugin()
        {
            // My system suffers from this issue: https://github.com/xenolightning/AudioSwitcher/issues/40
            // ...which causes the first call to the CoreAudioController to take up to 10 seconds,
            // so initialise it as soon as possible. Bit of a workaround, but eh.
            Task.Run(() =>
            {
                CoreAudioControllerInstance.Acquire();
            });
        }
    }
}
