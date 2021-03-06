using System;
using JetBrains.Annotations;

// ReSharper disable once UnusedMember.Global - public API

namespace MassiveKnob.Plugin
{
    /// <summary>
    /// Attach this attribute to a class to register it as a Massive Knob plugin.
    /// The class must implement IMassiveKnobPlugin.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [MeansImplicitUse]
    public class MassiveKnobPluginAttribute : Attribute
    {
    }
}
