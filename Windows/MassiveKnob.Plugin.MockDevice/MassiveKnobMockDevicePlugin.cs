using System;
using System.Collections.Generic;

namespace MassiveKnob.Plugin.MockDevice
{
    [MassiveKnobPlugin]
    public class MassiveKnobMockDevicePlugin : IMassiveKnobDevicePlugin
    {
        public Guid PluginId { get; } = new Guid("85f04232-d70f-494c-94a2-41452591ffb3");
        public string Name { get; } = "Mock Device";
        public string Description { get; } = "Emulates the actual device but does not communicate with anything.";
        public string Author { get; } = "Mark van Renswoude <mark@x2software.net>";
        public string Url { get; } = "https://www.github.com/MvRens/MassiveKnob/";

        public IEnumerable<IMassiveKnobDevice> Devices { get; } = new IMassiveKnobDevice[]
        {
            new Devices.MockDevice()
        };
    }
}
