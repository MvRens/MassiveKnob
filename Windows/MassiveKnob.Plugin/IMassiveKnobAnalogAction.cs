using System.Threading.Tasks;

namespace MassiveKnob.Plugin
{
    /// <summary>
    /// Required to be implemented for Action type InputAnalog. Receives an update when a knob's position changes.
    /// </summary>
    public interface IMassiveKnobAnalogAction : IMassiveKnobActionInstance
    {
        /// <summary>
        /// Called when a knob's position changes.
        /// </summary>
        /// <param name="value">The new value. Range is 0 to 100.</param>
        ValueTask AnalogChanged(byte value);
    }
}
