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
        /// Called right after this instance is created.
        /// </summary>
        /// <param name="context">Provides an interface to the Massive Knob settings and device. Can be stored until the device instance is disposed.</param>
        void Initialize(IMassiveKnobDeviceContext context);
        
        /// <summary>
        /// Called when a device should display it's settings. Assume the width is variable, height is
        /// determined by the UserControl. Return null to indicate there are no settings for this device.
        /// </summary>
        UserControl CreateSettingsControl();
    }
}
