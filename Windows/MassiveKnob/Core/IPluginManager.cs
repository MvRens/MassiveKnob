using System.Collections.Generic;
using MassiveKnob.Plugin;

namespace MassiveKnob.Core
{
    public interface IPluginManager
    {
        IEnumerable<IMassiveKnobPluginInfo> GetPlugins();
            
        IEnumerable<IMassiveKnobDevicePlugin> GetDevicePlugins();
        IEnumerable<IMassiveKnobActionPlugin> GetActionPlugins();
    }


    public interface IMassiveKnobPluginInfo
    {
        string Filename { get; }
        IMassiveKnobPlugin Plugin { get; }
    }
}
