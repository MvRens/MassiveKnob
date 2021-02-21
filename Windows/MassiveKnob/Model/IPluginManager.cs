using System.Collections.Generic;
using MassiveKnob.Plugin;

namespace MassiveKnob.Model
{
    public interface IPluginManager
    {
        IEnumerable<IMassiveKnobPlugin> Plugins { get; }

        IEnumerable<IMassiveKnobDevicePlugin> GetDevicePlugins();
    }
}
