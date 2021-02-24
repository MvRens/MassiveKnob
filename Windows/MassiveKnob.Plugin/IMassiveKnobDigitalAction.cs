using System.Threading.Tasks;

namespace MassiveKnob.Plugin
{
    /// <summary>
    /// Required to be implemented for Action type InputDigital. Receives an update when a knob's position changes.
    /// </summary>
    public interface IMassiveKnobDigitalAction : IMassiveKnobActionInstance
    {
        /// <summary>
        /// Called when a digital input's value changes.
        /// </summary>
        /// <param name="on">The new value.</param>
        ValueTask DigitalChanged(bool on);
    }
}
