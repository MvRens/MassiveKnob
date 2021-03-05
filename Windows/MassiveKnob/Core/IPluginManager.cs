using System.Collections.Generic;
using MassiveKnob.Plugin;

namespace MassiveKnob.Core
{
    public interface IPluginManager
    {
        IEnumerable<IMassiveKnobDevicePlugin> GetDevicePlugins();
        IEnumerable<IMassiveKnobActionPlugin> GetActionPlugins();
    }
}
