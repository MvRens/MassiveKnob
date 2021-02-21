using System;
using System.Windows.Controls;

namespace MassiveKnob.Plugin
{
    /// <summary>
    /// Represents a connection to a Massive Knob device.
    /// </summary>
    public interface IMassiveKnobDeviceInstance : IDisposable
    {
        /// <summary>
        /// Called when a device should display it's settings. Assume the width is variable, height is
        /// determined by the UserControl. Return null to indicate there are no settings for this device.
        /// </summary>
        UserControl CreateSettingsControl();
    }
}
