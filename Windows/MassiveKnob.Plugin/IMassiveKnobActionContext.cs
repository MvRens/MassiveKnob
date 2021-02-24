namespace MassiveKnob.Plugin
{
    /// <inheritdoc />
    public interface IMassiveKnobActionContext : IMassiveKnobContext
    {
        /// <summary>
        /// Sets the state of the analog output. Only valid for OutputAnalog action types, will raise an exception otherwise.
        /// </summary>
        /// <param name="value">The analog value in the range of 0 to 100.</param>
        void SetAnalogOutput(byte value);

        /// <summary>
        /// Sets the state of the digital output. Only valid for OutputDigital action types, will raise an exception otherwise.
        /// </summary>
        /// <param name="on">Whether the signal is on or off.</param>
        void SetDigitalOutput(bool on);
    }
}
