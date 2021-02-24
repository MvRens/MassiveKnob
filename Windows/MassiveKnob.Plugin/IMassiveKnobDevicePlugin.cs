using System.Collections.Generic;

namespace MassiveKnob.Plugin
{
    /// <summary>
    /// Implemented by plugins which interface with Massive Knob device.
    /// </summary>
    public interface IMassiveKnobDevicePlugin : IMassiveKnobPlugin
    {
        /// <summary>
        /// A list of devices supported by the plugin.
        /// </summary>
        IEnumerable<IMassiveKnobDevice> Devices { get; }
    }
}
