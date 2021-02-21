namespace MassiveKnob.Plugin
{
    /// <inheritdoc />
    public interface IMassiveKnobActionContext : IMassiveKnobContext
    {
        /// <summary>
        /// Sets the state of the signal. Only valid for OutputSignal action types, will raise an exception otherwise.
        /// </summary>
        /// <param name="on">Whether the signal is on or off.</param>
        void SetSignal(bool on);
    }
}
