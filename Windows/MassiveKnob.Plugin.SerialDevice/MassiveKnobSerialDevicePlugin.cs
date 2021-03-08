using System;
using System.Collections.Generic;

namespace MassiveKnob.Plugin.SerialDevice
{
    [MassiveKnobPlugin]
    public class MassiveKnobSerialDevicePlugin : IMassiveKnobDevicePlugin
    {
        public Guid PluginId { get; } = new Guid("276475e6-5ff0-420f-82dc-8aff5e8631d5");
        public string Name { get; } = "Serial Device";
        public string Description { get; } = "A Serial (USB) device which implements the Massive Knob Protocol.";
        public string Author { get; } = "Mark van Renswoude <mark@x2software.net>";
        public string Url { get; } = "https://www.github.com/MvRens/MassiveKnob/";

        public IEnumerable<IMassiveKnobDevice> Devices { get; } = new IMassiveKnobDevice[]
        {
            new Devices.SerialDevice()
        };
    }
}
