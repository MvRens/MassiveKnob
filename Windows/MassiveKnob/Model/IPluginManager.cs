using System.Collections.Generic;
using MassiveKnob.Plugin;

namespace MassiveKnob.Model
{
    public interface IPluginManager
    {
        IEnumerable<IMassiveKnobDevicePlugin> GetDevicePlugins();
        IEnumerable<IMassiveKnobActionPlugin> GetActionPlugins();
    }
}
