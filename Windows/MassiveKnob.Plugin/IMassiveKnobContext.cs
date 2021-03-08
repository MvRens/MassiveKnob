namespace MassiveKnob.Plugin
{
    /// <summary>
    /// Provides an interface to the Massive Knob settings and device.
    /// </summary>
    public interface IMassiveKnobContext
    {
        /// <summary>
        /// Reads the settings for the current action instance.
        /// </summary>
        /// <typeparam name="T">The class type to be deserialized using Newtonsoft.Json.</typeparam>
        /// <returns>The deserialized class if settings were previously stored, or a new instance of the class otherwise.</returns>
        T GetSettings<T>() where T : class, new();

        /// <summary>
        /// Stores the settings for the current action instance.
        /// </summary>
        /// <typeparam name="T">The class type to be serialized using Newtonsoft.Json.</typeparam>
        /// <param name="settings">The object to be serialized using Newtonsoft.Json.</param>
        void SetSettings<T>(T settings) where T : class, new();
    }
}
