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

        /// <summary>
        /// Called when the state of an analog output should be changed.
        /// </summary>
        /// <param name="analogOutputIndex">The index of the analog output to set.</param>
        /// <param name="value">The analog value in the range of 0 to 100.</param>
        void SetAnalogOutput(int analogOutputIndex, byte value);

        /// <summary>
        /// Called when the state of a digital output should be changed.
        /// </summary>
        /// <param name="digitalOutputIndex">The index of the digital output to set.</param>
        /// <param name="on">Whether the signal is on or off.</param>
        void SetDigitalOutput(int digitalOutputIndex, bool on);
    }
}
