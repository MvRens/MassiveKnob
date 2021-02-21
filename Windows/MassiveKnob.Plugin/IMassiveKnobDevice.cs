using System;

namespace MassiveKnob.Plugin
{
    /// <summary>
    /// Contains information about the device supported by this plugin.
    /// </summary>
    public interface IMassiveKnobDevice
    {
        /// <summary>
        /// Unique identifier for the device.
        /// </summary>
        Guid DeviceId { get; }

        /// <summary>
        /// The name of the action as shown in the action list when assigning to a knob or button.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// A short description of the functionality provided by the action.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Called when the device is selected.
        /// </summary>
        /// <param name="context">Provides an interface to the Massive Knob settings and device. Can be stored until the device instance is disposed.</param>
        IMassiveKnobDeviceInstance Create(IMassiveKnobContext context);
    }
}
