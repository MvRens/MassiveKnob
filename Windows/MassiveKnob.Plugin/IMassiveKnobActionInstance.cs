using System;
using System.Windows.Controls;

namespace MassiveKnob.Plugin
{
    /// <summary>
    /// Base interface for actions assigned to a knob or button. Implement one of the descendant
    /// interfaces to provide input or output.
    /// </summary>
    public interface IMassiveKnobActionInstance : IDisposable
    {
        /// <summary>
        /// Called when an action should display it's settings. Assume the width is variable, height is
        /// determined by the UserControl. Return null to indicate there are no settings for this action.
        /// </summary>
        UserControl CreateSettingsControl();
    }
}
