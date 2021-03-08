using System.Collections.Generic;

namespace MassiveKnob.Plugin
{
    /// <summary>
    /// Implemented by plugins supporting actions which can be assigned to knobs or buttons.
    /// </summary>
    public interface IMassiveKnobActionPlugin : IMassiveKnobPlugin
    {
        /// <summary>
        /// A list of actions supported by the plugin which can be assigned to knobs or buttons.
        /// </summary>
        IEnumerable<IMassiveKnobAction> Actions { get; }
    }
}
