using System;

namespace MassiveKnob.Plugin
{
    /// <summary>
    /// Minimum required interface to implement for writing a Massive Knob plugin.
    /// Implement one of the various descendant interfaces to provide more functionality.
    /// </summary>
    public interface IMassiveKnobPlugin
    {
        /// <summary>
        /// Unique identifier for the plugin.
        /// </summary>
        Guid PluginId { get; }
        
        /// <summary>
        /// The name of the plugin as shown in the plugin list.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// A short description of the functionality provided by the plugin.
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// The name of the author(s) of the plugin.
        /// </summary>
        string Author { get; }
        
        /// <summary>
        /// URL to the plugin's website.
        /// </summary>
        string Url { get; }
    }
}
