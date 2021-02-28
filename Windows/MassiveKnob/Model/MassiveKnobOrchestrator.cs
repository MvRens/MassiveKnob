using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using MassiveKnob.Helpers;
using MassiveKnob.Plugin;
using MassiveKnob.Settings;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Serilog.Extensions.Logging;
using ILogger = Serilog.ILogger;

namespace MassiveKnob.Model
{
    public class MassiveKnobOrchestrator : IMassiveKnobOrchestrator
    {
        private readonly IPluginManager pluginManager;
        private readonly ILogger logger;

        private readonly object settingsLock = new object();
        private Settings.Settings settings;
        private readonly SerialQueue flushSettingsQueue = new SerialQueue();

        private MassiveKnobDeviceInfo activeDevice;
        private readonly Subject<MassiveKnobDeviceInfo> activeDeviceInfoSubject = new Subject<MassiveKnobDeviceInfo>();
        private IMassiveKnobDeviceContext activeDeviceContext;

        private readonly List<ActionMapping> analogInputs = new List<ActionMapping>();
        private readonly List<ActionMapping> digitalInputs = new List<ActionMapping>();
        private readonly List<ActionMapping> analogOutputs = new List<ActionMapping>();
        private readonly List<ActionMapping> digitalOutputs = new List<ActionMapping>();


        public MassiveKnobDeviceInfo ActiveDevice
        {
            get => activeDevice;
            private set
            {
                if (value == activeDevice)
                    return;

                activeDevice = value;
                activeDeviceInfoSubject.OnNext(activeDevice);
            }
        }

        public IObservable<MassiveKnobDeviceInfo> ActiveDeviceSubject => activeDeviceInfoSubject;


        public MassiveKnobOrchestrator(IPluginManager pluginManager, ILogger logger)
        {
            this.pluginManager = pluginManager;
            this.logger = logger;
        }


        public void Dispose()
        {
            activeDevice?.Instance?.Dispose();

            void DisposeMappings(IEnumerable<ActionMapping> mappings)
            {
                foreach (var mapping in mappings)
                    mapping?.ActionInfo.Instance?.Dispose();
            }
            
            
            DisposeMappings(analogInputs);
            DisposeMappings(digitalInputs);
            DisposeMappings(analogOutputs);
            DisposeMappings(digitalOutputs);

            activeDeviceInfoSubject?.Dispose();
        }


        public void Load()
        {
            lock (settingsLock)
            {
                settings = SettingsJsonSerializer.Deserialize();

                if (settings.Device == null)
                    return;

                var allDevices = pluginManager.GetDevicePlugins().SelectMany(dp => dp.Devices);
                var device = allDevices.FirstOrDefault(d => d.DeviceId == settings.Device.DeviceId);
                
                InternalSetActiveDevice(device, false);
            }
        }


        MassiveKnobDeviceInfo IMassiveKnobOrchestrator.ActiveDevice => activeDevice;

        public MassiveKnobDeviceInfo SetActiveDevice(IMassiveKnobDevice device)
        {
            return InternalSetActiveDevice(device, true);
        }

        
        public MassiveKnobActionInfo GetAction(MassiveKnobActionType actionType, int index)
        {
            var list = GetActionMappingList(actionType);
            return index >= list.Count ? null : list[index]?.ActionInfo;
        }

        
        public MassiveKnobActionInfo SetAction(MassiveKnobActionType actionType, int index, IMassiveKnobAction action)
        {
            var list = GetActionMappingList(actionType);
            if (index >= list.Count)
                return null;

            if (list[index]?.ActionInfo.Info == action)
                return list[index].ActionInfo;
            
            list[index]?.ActionInfo.Instance?.Dispose();

            lock (settingsLock)
            {
                var settingsList = GetActionSettingsList(actionType);
                while (index >= settingsList.Count)
                    settingsList.Add(null);

                settingsList[index] = action == null ? null : new Settings.Settings.ActionSettings
                {
                    ActionId = action.ActionId,
                    Settings = null
                };
            }
            
            FlushSettings();


            Action initializeAfterRegistration = null;
            var mapping = CreateActionMapping(action, index, (actionInstance, actionContext) =>
            {
                initializeAfterRegistration = () => actionInstance.Initialize(actionContext);
            });

            list[index] = mapping;
            initializeAfterRegistration?.Invoke();

            return mapping?.ActionInfo;
        }


        private MassiveKnobDeviceInfo InternalSetActiveDevice(IMassiveKnobDevice device, bool resetSettings)
        {
            if (device == ActiveDevice?.Info)
                return ActiveDevice;


            if (resetSettings)
            {
                lock (settingsLock)
                {
                    if (device == null)
                        settings.Device = null;
                    else
                    {
                        settings.Device = new Settings.Settings.DeviceSettings
                        {
                            DeviceId = device.DeviceId,
                            Settings = null
                        };
                    }
                }
                
                FlushSettings();
            }

            ActiveDevice?.Instance.Dispose();

            if (device != null)
            {
                var instance = device.Create(new SerilogLoggerProvider(logger.ForContext("Device", device.DeviceId)).CreateLogger(null));
                ActiveDevice = new MassiveKnobDeviceInfo(device, instance, null);

                activeDeviceContext = new DeviceContext(this, device);
                instance.Initialize(activeDeviceContext);
            }
            else
            {
                ActiveDevice = null;
                activeDeviceContext = null;
            }

            return ActiveDevice;
        }


        protected T GetDeviceSettings<T>(IMassiveKnobDeviceContext context) where T : class, new()
        {
            if (context != activeDeviceContext)
                throw new InvalidOperationException("Caller must be the active device to retrieve the settings");

            lock (settingsLock)
            {
                return settings.Device.Settings?.ToObject<T>() ?? new T();
            }
        }


        protected void SetDeviceSettings<T>(IMassiveKnobDeviceContext context, IMassiveKnobDevice device, T deviceSettings) where T : class, new()
        {
            if (context != activeDeviceContext)
                throw new InvalidOperationException("Caller must be the active device to update the settings");

            lock (settingsLock)
            {
                if (settings.Device == null)
                    settings.Device = new Settings.Settings.DeviceSettings
                    {
                        DeviceId = device.DeviceId
                    };

                settings.Device.Settings = JObject.FromObject(deviceSettings);
            }

            FlushSettings();
        }


        protected T GetActionSettings<T>(IMassiveKnobActionContext context, IMassiveKnobAction action, int index) where T : class, new()
        {
            var list = GetActionMappingList(action.ActionType);
            if (index >= list.Count)
                return new T();
            
            if (list[index]?.Context != context)
                throw new InvalidOperationException("Caller must be the active action to retrieve the settings");

            lock (settingsLock)
            {
                var settingsList = GetActionSettingsList(action.ActionType);
                if (index >= settingsList.Count)
                    return new T();

                return settingsList[index].Settings?.ToObject<T>() ?? new T();
            }    
        }


        protected void SetActionSettings<T>(IMassiveKnobActionContext context, IMassiveKnobAction action, int index, T actionSettings) where T : class, new()
        {
            var list = GetActionMappingList(action.ActionType);
            if (index >= list.Count)
                return;

            if (list[index]?.Context != context)
                throw new InvalidOperationException("Caller must be the active action to retrieve the settings");

            lock (settingsLock)
            {
                var settingsList = GetActionSettingsList(action.ActionType);
                
                while (index >= settingsList.Count)
                    settingsList.Add(null);

                if (settingsList[index] == null)
                    settingsList[index] = new Settings.Settings.ActionSettings
                    {
                        ActionId = action.ActionId
                    };

                settingsList[index].Settings = JObject.FromObject(actionSettings);
            }
            
            FlushSettings();
        }


        protected void AnalogChanged(IMassiveKnobDeviceContext context, int analogInputIndex, byte value)
        {
            if (context != activeDeviceContext)
                return;

            var mapping = GetActionMappingList(MassiveKnobActionType.InputAnalog);
            if (mapping == null || analogInputIndex >= mapping.Count || mapping[analogInputIndex] == null)
                return;

            if (mapping[analogInputIndex].ActionInfo.Instance is IMassiveKnobAnalogAction analogAction)
                analogAction.AnalogChanged(value);
        }


        protected void DigitalChanged(IMassiveKnobDeviceContext context, int digitalInputIndex, bool on)
        {
            if (context != activeDeviceContext)
                return;

            var mapping = GetActionMappingList(MassiveKnobActionType.InputDigital);
            if (mapping == null || digitalInputIndex >= mapping.Count || mapping[digitalInputIndex] == null)
                return;

            if (mapping[digitalInputIndex].ActionInfo.Instance is IMassiveKnobDigitalAction digitalAction)
                digitalAction.DigitalChanged(on);
        }


        public void SetAnalogOutput(IMassiveKnobActionContext context, IMassiveKnobAction action, int index, byte value)
        {
            if (activeDevice == null)
                return;
            
            var list = GetActionMappingList(action.ActionType);
            if (index >= list.Count)
                return;

            if (list[index]?.Context != context)
                return;
            
            activeDevice.Instance.SetAnalogOutput(index, value);
        }


        public void SetDigitalOutput(IMassiveKnobActionContext context, IMassiveKnobAction action, int index, bool on)
        {
            if (activeDevice == null)
                return;

            var list = GetActionMappingList(action.ActionType);
            if (index >= list.Count)
                return;

            if (list[index]?.Context != context)
                return;

            activeDevice.Instance.SetDigitalOutput(index, on);
        }


        private List<ActionMapping> GetActionMappingList(MassiveKnobActionType actionType)
        {
            switch (actionType)
            {
                case MassiveKnobActionType.InputAnalog:
                    return analogInputs;
                
                case MassiveKnobActionType.InputDigital:
                    return digitalInputs;
                
                case MassiveKnobActionType.OutputAnalog:
                    return analogOutputs;
                
                case MassiveKnobActionType.OutputDigital:
                    return digitalOutputs;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
            }
        }

        
        private List<Settings.Settings.ActionSettings> GetActionSettingsList(MassiveKnobActionType actionType)
        {
            switch (actionType)
            {
                case MassiveKnobActionType.InputAnalog:
                    return settings.AnalogInput;

                case MassiveKnobActionType.InputDigital:
                    return settings.DigitalInput;

                case MassiveKnobActionType.OutputAnalog:
                    return settings.AnalogOutput;

                case MassiveKnobActionType.OutputDigital:
                    return settings.DigitalOutput;

                default:
                    throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
            }
        }

        private void FlushSettings()
        {
            Settings.Settings settingsSnapshot;
            
            lock (settingsLock)
            { 
                settingsSnapshot = settings.Clone();
            }
            
            flushSettingsQueue.Enqueue(async () =>
            {
                await SettingsJsonSerializer.Serialize(settingsSnapshot);
            });
        }


        protected void UpdateActiveDeviceSpecs(IMassiveKnobDeviceContext context, DeviceSpecs specs)
        {
            if (context != activeDeviceContext)
                return;

            
            var delayedInitializeActions = new List<Action>();
            void DelayedInitialize(IMassiveKnobActionInstance instance, IMassiveKnobActionContext instanceContext)
            {
                delayedInitializeActions.Add(() =>
                {
                    instance.Initialize(instanceContext);
                });
            }
            
            lock (settingsLock)
            {
                UpdateMapping(analogInputs, specs.AnalogInputCount, settings.AnalogInput, DelayedInitialize);
                UpdateMapping(digitalInputs, specs.DigitalInputCount, settings.DigitalInput, DelayedInitialize);
                UpdateMapping(analogOutputs, specs.AnalogOutputCount, settings.AnalogOutput, DelayedInitialize);
                UpdateMapping(digitalOutputs, specs.DigitalOutputCount, settings.DigitalOutput, DelayedInitialize);
            }

            foreach (var delayedInitializeAction in delayedInitializeActions)
                delayedInitializeAction();


            ActiveDevice = new MassiveKnobDeviceInfo(
                ActiveDevice.Info,
                ActiveDevice.Instance,
                specs);
        }


        private void UpdateMapping(List<ActionMapping> mapping, int newCount, List<Settings.Settings.ActionSettings> actionSettings, Action<IMassiveKnobActionInstance, IMassiveKnobActionContext> initializeOutsideLock)
        {
            if (mapping.Count > newCount)
            {
                for (var actionIndex = newCount; actionIndex < mapping.Count; actionIndex++)
                    mapping[actionIndex]?.ActionInfo.Instance?.Dispose();

                mapping.RemoveRange(newCount, mapping.Count - newCount);
            }

            if (actionSettings.Count > newCount)
                actionSettings.RemoveRange(newCount, actionSettings.Count - newCount);


            if (mapping.Count >= newCount) return;
            {
                var allActions = pluginManager.GetActionPlugins().SelectMany(ap => ap.Actions).ToArray();
                
                for (var actionIndex = mapping.Count; actionIndex < newCount; actionIndex++)
                {
                    if (actionIndex < actionSettings.Count && actionSettings[actionIndex] != null)
                    {
                        var action = allActions.FirstOrDefault(d => d.ActionId == actionSettings[actionIndex].ActionId);
                        mapping.Add(CreateActionMapping(action, actionIndex, initializeOutsideLock));
                    }
                    else
                        mapping.Add(null);
                }
            }
        }


        private ActionMapping CreateActionMapping(IMassiveKnobAction action, int index, Action<IMassiveKnobActionInstance, IMassiveKnobActionContext> initialize)
        {
            if (action == null)
                return null;

            var actionLogger = logger
                .ForContext("Action", action.ActionId)
                .ForContext("ActionType", action.ActionType)
                .ForContext("Index", index);
            
            var instance = action.Create(new SerilogLoggerProvider(actionLogger).CreateLogger(null));
            var context = new ActionContext(this, action, index);

            var mapping = new ActionMapping(new MassiveKnobActionInfo(action, instance), context);
            initialize(instance, context);

            return mapping;
        }


        private class ActionMapping
        {
            public MassiveKnobActionInfo ActionInfo { get; }
            public IMassiveKnobActionContext Context { get; }

            
            public ActionMapping(MassiveKnobActionInfo actionInfo, IMassiveKnobActionContext context)
            {
                ActionInfo = actionInfo;
                Context = context;
            }
        }


        private class DeviceContext : IMassiveKnobDeviceContext
        {
            private readonly MassiveKnobOrchestrator owner;
            private readonly IMassiveKnobDevice device;


            public DeviceContext(MassiveKnobOrchestrator owner, IMassiveKnobDevice device)
            {
                this.owner = owner;
                this.device = device;
            }
                    
            
            public T GetSettings<T>() where T : class, new()
            {
                return owner.GetDeviceSettings<T>(this);
            }
            
            
            public void SetSettings<T>(T settings) where T : class, new()
            {
                owner.SetDeviceSettings(this, device, settings);
            }

            
            public void Connecting()
            {
                // TODO update status ?
            }


            public void Connected(DeviceSpecs specs)
            {
                // TODO update status ?

                owner.UpdateActiveDeviceSpecs(this, specs);
            }


            public void Disconnected()
            {
                // TODO update status ?
            }


            public void AnalogChanged(int analogInputIndex, byte value)
            {
                owner.AnalogChanged(this, analogInputIndex, value);
            }

            
            public void DigitalChanged(int digitalInputIndex, bool on)
            {
                owner.DigitalChanged(this, digitalInputIndex, on);
            }
        }


        private class ActionContext : IMassiveKnobActionContext
        {
            private readonly MassiveKnobOrchestrator owner;
            private readonly IMassiveKnobAction action;
            private readonly int index;


            public ActionContext(MassiveKnobOrchestrator owner, IMassiveKnobAction action, int index)
            {
                this.owner = owner;
                this.action = action;
                this.index = index;
            }

            
            public T GetSettings<T>() where T : class, new()
            {
                return owner.GetActionSettings<T>(this, action, index);
            }

            
            public void SetSettings<T>(T settings) where T : class, new()
            {
                owner.SetActionSettings(this, action, index, settings);
            }

            
            public void SetAnalogOutput(byte value)
            {
                owner.SetAnalogOutput(this, action, index, value);
            }
            
            
            public void SetDigitalOutput(bool on)
            {
                owner.SetDigitalOutput(this, action, index, on);
            }
        }
    }
}
