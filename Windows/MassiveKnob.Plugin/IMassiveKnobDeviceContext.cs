namespace MassiveKnob.Plugin
{
    /// <inheritdoc />
    public interface IMassiveKnobDeviceContext : IMassiveKnobContext
    {
        /// <summary>
        /// Call when an attempt is being made to connect to the device. If the connection is always ready
        /// this call can be skipped.
        /// </summary>
        void Connecting();

        /// <summary>
        /// Call when the device is connected. This method must be called before AnalogChanged or DigitalChanged.
        /// </summary>
        /// <param name="specs">The specs as reported by the device.</param>
        void Connected(DeviceSpecs specs);

        /// <summary>
        /// Call when the connection to the device has been lost.
        /// </summary>
        void Disconnected();

        /// <summary>
        /// Call when an analog input's value has changed.
        /// </summary>
        /// <param name="analogInputIndex">The index of the analog input. Must be within bounds of the value reported in Connected.</param>
        /// <param name="value">The new value in the range from 0 to 100.</param>
        void AnalogChanged(int analogInputIndex, byte value);

        /// <summary>
        /// Call when a digital input's state has changed.
        /// </summary>
        /// <param name="digitalInputIndex">The index of the digital input. Must be within bounds of the value reported in Connected.</param>
        /// <param name="on">Whether the input is considered on or off.</param>
        void DigitalChanged(int digitalInputIndex, bool on);
    }


    /// <summary>
    /// Defines the specs as reported by the device.
    /// </summary>
    public readonly struct DeviceSpecs
    {
        /// <summary>
        /// The number of analog input controls supported by the device.
        /// </summary>
        public readonly int AnalogInputCount;
        
        /// <summary>
        /// The number of digital input controls supported by the device.
        /// </summary>
        public readonly int DigitalInputCount;

        /// <summary>
        /// The number of analog output controls supported by the device.
        /// </summary>
        public readonly int AnalogOutputCount;

        /// <summary>
        /// The number of digital output controls supported by the device.
        /// </summary>
        public readonly int DigitalOutputCount;


        /// <summary>
        /// Defines the specs as reported by the device.
        /// </summary>
        /// <param name="analogInputCount">The number of analog input controls supported by the device.</param>
        /// <param name="digitalInputCount">The number of digital input controls supported by the device.</param>
        /// <param name="analogOutputCount">The number of analog output controls supported by the device.</param>
        /// <param name="digitalOutputCount">The number of digital output controls supported by the device.</param>
        public DeviceSpecs(int analogInputCount, int digitalInputCount, int analogOutputCount, int digitalOutputCount)
        {
            AnalogInputCount = analogInputCount;
            DigitalInputCount = digitalInputCount;
            AnalogOutputCount = analogOutputCount;
            DigitalOutputCount = digitalOutputCount;
        }
    }
}
