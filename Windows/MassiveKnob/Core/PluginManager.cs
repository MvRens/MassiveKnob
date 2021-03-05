using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MassiveKnob.Plugin;
using Newtonsoft.Json;
using Serilog;
using Serilog.Extensions.Logging;

namespace MassiveKnob.Core
{
    public class MassiveKnobPluginIdConflictException : Exception
    {
        public Guid ConflictingId { get; }
        public string FirstAssemblyFilename { get; }
        public string ConflictingAssemblyFilename { get; }

        
        public MassiveKnobPluginIdConflictException(
            Guid conflictingId, 
            string firstAssemblyFilename, 
            string conflictingAssemblyFilename)
            : base($"Conflicting ID {conflictingId} was already registered by {firstAssemblyFilename}.")
        {
            ConflictingId = conflictingId;
            FirstAssemblyFilename = firstAssemblyFilename;
            ConflictingAssemblyFilename = conflictingAssemblyFilename;
        }
    }
    
    
    public class PluginManager : IPluginManager
    {
        private readonly ILogger logger;
        private readonly List<IMassiveKnobPlugin> plugins = new List<IMassiveKnobPlugin>();


        public PluginManager(ILogger logger)
        {
            this.logger = logger;
        }
        

        public IEnumerable<IMassiveKnobDevicePlugin> GetDevicePlugins()
        {
            return plugins.Where(p => p is IMassiveKnobDevicePlugin).Cast<IMassiveKnobDevicePlugin>();
        }

        public IEnumerable<IMassiveKnobActionPlugin> GetActionPlugins()
        {
            return plugins.Where(p => p is IMassiveKnobActionPlugin).Cast<IMassiveKnobActionPlugin>();
        }


        public void Load(Action<Exception, string> onException)
        {
            var registeredIds = new RegisteredIds();
            
            var codeBase = Assembly.GetEntryAssembly()?.CodeBase;
            if (!string.IsNullOrEmpty(codeBase))
            {
                var localPath = Path.GetDirectoryName(new Uri(codeBase).LocalPath);
                if (!string.IsNullOrEmpty(localPath))
                {
                    var applicationPluginPath = Path.Combine(localPath, @"Plugins");
                    LoadPlugins(applicationPluginPath, registeredIds, onException);
                }
            }


            var localPluginPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"MassiveKnob", @"Plugins");
            LoadPlugins(localPluginPath, registeredIds, onException);
        }


        private void LoadPlugins(string path, RegisteredIds registeredIds, Action<Exception, string> onException)
        {
            logger.Information("Checking {path} for plugins...", path);
            if (!Directory.Exists(path))
                return;
            
            
            var metadataFilenames = Directory.GetFiles(path, "MassiveKnobPlugin.json", SearchOption.AllDirectories);
            
            foreach (var metadataFilename in metadataFilenames)
            {
                var pluginPath = Path.GetDirectoryName(metadataFilename);
                if (string.IsNullOrEmpty(pluginPath))
                    continue;
                
                PluginMetadata pluginMetadata;
                try
                {
                    pluginMetadata = LoadMetadata(metadataFilename);
                }
                catch (Exception e)
                {
                    logger.Warning("Could not load plugin metadata from {metadataFilename}: {message}", metadataFilename, e.Message);
                    continue;
                }

                var entryAssemblyFilename = Path.Combine(pluginPath, pluginMetadata.EntryAssembly);
                if (!File.Exists(entryAssemblyFilename))
                {
                    logger.Warning("Entry assembly specified in {metadataFilename} does not exist: {entryAssemblyFilename}", entryAssemblyFilename);
                    continue;
                }
                    
                try
                {
                    logger.Information("Plugin found in {pluginPath}", pluginPath);

                    var pluginAssembly = Assembly.LoadFrom(entryAssemblyFilename);
                    RegisterPlugins(entryAssemblyFilename, pluginAssembly, registeredIds);
                }
                catch (Exception e)
                {
                    logger.Warning("Error while loading plugin {entryAssemblyFilename}: {message}", entryAssemblyFilename, e.Message);
                    onException(e, entryAssemblyFilename);
                }
            }
        }


        private static PluginMetadata LoadMetadata(string filename)
        {
            string json;

            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, true))
            using (var streamReader = new StreamReader(stream, Encoding.UTF8))
            {
                json = streamReader.ReadToEnd();
            }

            if (string.IsNullOrEmpty(json))
                throw new IOException("Metadata file is empty");
                
            return JsonConvert.DeserializeObject<PluginMetadata>(json);
        }


        private void RegisterPlugins(string filename, Assembly assembly, RegisteredIds registeredIds)
        {
            var pluginTypes = assembly.GetTypes().Where(t => t.GetCustomAttribute<MassiveKnobPluginAttribute>() != null);
            foreach (var pluginType in pluginTypes)
            {
                var pluginInstance = Activator.CreateInstance(pluginType);
                if (!(pluginInstance is IMassiveKnobPlugin))
                    throw new InvalidCastException($"Type {pluginType.FullName} claims to be a MassiveKnobPlugin but does not implement IMassiveKnobPlugin");

                var plugin = (IMassiveKnobPlugin) pluginInstance;
                logger.Information("Found plugin with Id {pluginId}: {name}", plugin.PluginId, plugin.Name);

                ValidateRegistration(filename, plugin, registeredIds);
                plugins.Add((IMassiveKnobPlugin)pluginInstance);
            }
        }


        private void ValidateRegistration(string filename, IMassiveKnobPlugin plugin, RegisteredIds registeredIds)
        {
            // Make sure all GUIDs are actually unique and someone has not copy/pasted a plugin without
            // modifying the values. This way we can safely make that assumption in other code.
            if (registeredIds.PluginById.TryGetValue(plugin.PluginId, out var conflictingPluginFilename))
                throw new MassiveKnobPluginIdConflictException(plugin.PluginId, conflictingPluginFilename, filename);

            registeredIds.PluginById.Add(plugin.PluginId, filename);

            
            // ReSharper disable once ConvertIfStatementToSwitchStatement - no, a plugin can implement both interfaces
            if (plugin is IMassiveKnobDevicePlugin devicePlugin)
            {
                foreach (var device in devicePlugin.Devices)
                {
                    logger.Information("- Device {deviceId}: {name}", device.DeviceId, device.Name);
                    
                    if (registeredIds.DeviceById.TryGetValue(device.DeviceId, out var conflictingDeviceFilename))
                        throw new MassiveKnobPluginIdConflictException(device.DeviceId, conflictingDeviceFilename, filename);
                    
                    registeredIds.DeviceById.Add(device.DeviceId, filename);
                }
            }


            // ReSharper disable once InvertIf
            if (plugin is IMassiveKnobActionPlugin actionPlugin)
            {
                foreach (var action in actionPlugin.Actions)
                {
                    logger.Information("- Action {actionId}: {name}", action.ActionId, action.Name);

                    if (registeredIds.ActionById.TryGetValue(action.ActionId, out var conflictingActionFilename))
                        throw new MassiveKnobPluginIdConflictException(action.ActionId, conflictingActionFilename, filename);

                    registeredIds.ActionById.Add(action.ActionId, filename);
                    ValidateActionType(action);
                }
            }
        }
        
        
        private void ValidateActionType(IMassiveKnobAction action)
        {
            var instance = action.Create(new SerilogLoggerProvider(logger).CreateLogger(null));
            if (instance == null)
                throw new NullReferenceException("Create method must not return null");
            
            switch (action.ActionType)
            {
                case MassiveKnobActionType.InputAnalog:
                    if (!(instance is IMassiveKnobAnalogAction))
                        throw new InvalidCastException("InputAnalog action must implement IMassiveKnobAnalogAction");
                            
                    break;
                
                case MassiveKnobActionType.InputDigital:
                    if (!(instance is IMassiveKnobDigitalAction))
                        throw new InvalidCastException("InputDigital action must implement IMassiveKnobDigitalAction");

                    break;
                
                case MassiveKnobActionType.OutputAnalog:
                case MassiveKnobActionType.OutputDigital:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(action.ActionType), action.ActionType, @"Unsupported action type: " + (int)action.ActionType);
            }
        }


        private class RegisteredIds
        {
            public readonly Dictionary<Guid, string> PluginById = new Dictionary<Guid, string>();
            public readonly Dictionary<Guid, string> DeviceById = new Dictionary<Guid, string>();
            public readonly Dictionary<Guid, string> ActionById = new Dictionary<Guid, string>();
        }


        private class PluginMetadata
        {
            public string EntryAssembly { get; set; }
        }
    }
}
