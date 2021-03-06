using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassiveKnob.Plugin.CoreAudio.GetDefault;
using MassiveKnob.Plugin.CoreAudio.GetMuted;
using MassiveKnob.Plugin.CoreAudio.GetVolume;
using MassiveKnob.Plugin.CoreAudio.SetDefault;
using MassiveKnob.Plugin.CoreAudio.SetMuted;
using MassiveKnob.Plugin.CoreAudio.SetVolume;

namespace MassiveKnob.Plugin.CoreAudio
{
    [MassiveKnobPlugin]
    public class MassiveKnobCoreAudioPlugin : IMassiveKnobActionPlugin
    {
        public Guid PluginId { get; } = new Guid("eaa5d3f8-8f9b-4a4b-8e29-827228d23e95");
        public string Name { get; } = Strings.PluginName;
        public string Description { get; } = Strings.PluginDescription;
        public string Author { get; } = "Mark van Renswoude <mark@x2software.net>";
        public string Url { get; } = "https://www.github.com/MvRens/MassiveKnob/";

        public IEnumerable<IMassiveKnobAction> Actions { get; } = new IMassiveKnobAction[]
        {
            new DeviceSetVolumeAction(),
            new DeviceGetVolumeAction(),
            
            new DeviceSetMutedAction(),
            new DeviceGetMutedAction(),
            
            new DeviceSetDefaultAction(),
            new DeviceGetDefaultAction()
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
