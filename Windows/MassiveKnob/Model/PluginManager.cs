using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MassiveKnob.Plugin;

namespace MassiveKnob.Model
{
    public class PluginManager : IPluginManager
    {
        private readonly List<IMassiveKnobPlugin> plugins = new List<IMassiveKnobPlugin>();


        public IEnumerable<IMassiveKnobPlugin> Plugins => plugins;

        public IEnumerable<IMassiveKnobDevicePlugin> GetDevicePlugins()
        {
            return plugins.Where(p => p is IMassiveKnobDevicePlugin).Cast<IMassiveKnobDevicePlugin>();
        }


        public void Load()
        {
            var codeBase = Assembly.GetEntryAssembly()?.CodeBase;
            if (!string.IsNullOrEmpty(codeBase))
            {
                var localPath = Path.GetDirectoryName(new Uri(codeBase).LocalPath);
                if (!string.IsNullOrEmpty(localPath))
                {
                    var applicationPluginPath = Path.Combine(localPath, @"Plugins");
                    LoadPlugins(applicationPluginPath);
                }
            }


            var localPluginPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"MassiveKnob", @"Plugins");
            LoadPlugins(localPluginPath);
        }


        private void LoadPlugins(string path)
        {
            if (!Directory.Exists(path))
                return;
            
            var filenames = Directory.GetFiles(path, "*.dll");

            foreach (var filename in filenames)
            {
                try
                {
                    var pluginAssembly = Assembly.LoadFrom(filename);
                    RegisterPlugins(pluginAssembly);
                }
                catch (Exception e)
                {
                    // TODO report error
//                    Console.WriteLine(e);
                    throw;
                }
            }
        }


        private void RegisterPlugins(Assembly assembly)
        {
            var pluginTypes = assembly.GetTypes().Where(t => t.GetCustomAttribute<MassiveKnobPluginAttribute>() != null);
            foreach (var pluginType in pluginTypes)
            {
                var pluginInstance = Activator.CreateInstance(pluginType);
                if (!(pluginInstance is IMassiveKnobPlugin))
                    throw new InvalidCastException($"Type {pluginType.FullName} claims to be a MassiveKnobPlugin but does not implement IMassiveKnobPlugin");
                
                plugins.Add((IMassiveKnobPlugin)pluginInstance);
            }
        }
    }
}
