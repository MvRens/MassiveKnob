using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using MassiveKnob.Helpers;
using MassiveKnob.Plugin;
using MassiveKnob.Settings;
using Newtonsoft.Json.Linq;
using Serilog.Extensions.Logging;
using ILogger = Serilog.ILogger;

namespace MassiveKnob.Core
{
    public class MassiveKnobOrchestrator : IMassiveKnobOrchestrator
    {
        private readonly IPluginManager pluginManager;
        private readonly ILogger logger;

        private readonly object settingsLock = new object();
        private readonly MassiveKnobSettings settings;
        private readonly SerialQueue flushSettingsQueue = new SerialQueue();

        private MassiveKnobDeviceInfo activeDevice;
        private readonly Subject<MassiveKnobDeviceInfo> activeDeviceInfoSubject = new Subject<MassiveKnobDeviceInfo>();
        private readonly Subject<MassiveKnobDeviceStatus> deviceStatusSubject = new Subject<MassiveKnobDeviceStatus>();
        private IMassiveKnobDeviceContext activeDeviceContext;

        private readonly List<ActionMapping> analogInputs = new List<ActionMapping>();
        private readonly List<ActionMapping> digitalInputs = new List<ActionMapping>();
        private readonly List<ActionMapping> analogOutputs = new List<ActionMapping>();
        private readonly List<ActionMapping> digitalOutputs = new List<ActionMapping>();

        private readonly Dictionary<int, byte> analogOutputValues = new Dictionary<int, byte>();
        private readonly Dictionary<int, bool> digitalOutputValues = new Dictionary<int, bool>();
        private readonly Dictionary<int, bool> digitalToAnalogOutputValues = new Dictionary<int, bool>();


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

        public MassiveKnobDeviceStatus DeviceStatus { get; private set; } = MassiveKnobDeviceStatus.Disconnected;
        public IObservable<MassiveKnobDeviceStatus> DeviceStatusSubject => deviceStatusSubject;


        public MassiveKnobOrchestrator(IPluginManager pluginManager, ILogger logger, MassiveKnobSettings settings)
        {
            this.pluginManager = pluginManager;
            this.logger = logger;
            this.settings = settings;
        }


        public void Dispose()
        {
            activeDeviceContext = null;
            activeDevice?.Instance?.Dispose();

            void DisposeMappings(ICollection<ActionMapping> mappings)
            {
                foreach (var mapping in mappings)
                    mapping?.ActionInfo.Instance?.Dispose();

                mappings.Clear();
            }


            lock (settingsLock)
            {
                DisposeMappings(analogInputs);
                DisposeMappings(digitalInputs);
                DisposeMappings(analogOutputs);
                DisposeMappings(digitalOutputs);
            }

            activeDeviceInfoSubject?.Dispose();
        }


        public void Load()
        {
            lock (settingsLock)
            {
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
            lock (settingsLock)
            {
                var list = GetActionMappingList(actionType);
                return index >= list.Count ? null : list[index]?.ActionInfo;
            }
        }

        
        public MassiveKnobActionInfo SetAction(MassiveKnobActionType actionType, int index, IMassiveKnobAction action)
        {
            List<ActionMapping> list;
            
            lock (settingsLock)
            {
                list = GetActionMappingList(actionType);
                if (index >= list.Count)
                    return null;

                if (list[index]?.ActionInfo.Info == action)
                    return list[index].ActionInfo;
                
                list[index]?.ActionInfo.Instance?.Dispose();

                var settingsList = GetActionSettingsList(actionType);
                while (index >= settingsList.Count)
                    settingsList.Add(null);

                settingsList[index] = action == null ? null : new MassiveKnobSettings.ActionSettings
                {
                    ActionId = action.ActionId,
                    Settings = null
                };
            }
            
            FlushSettings();


            Action initializeAfterRegistration = null;
            var mapping = CreateActionMapping(actionType, action, index, (actionInstance, actionContext) =>
            {
                initializeAfterRegistration = () => actionInstance.Initialize(actionContext);
            });

            lock (settingsLock)
            {
                list[index] = mapping;
            }
            
            initializeAfterRegistration?.Invoke();

            return mapping?.ActionInfo;
        }


        public MassiveKnobSettings.DigitalToAnalogSettings GetDigitalToAnalogSettings(int analogOutputIndex)
        {
            lock (settingsLock)
            {
                var settingsList = GetActionSettingsList(MassiveKnobActionType.OutputAnalog);
                if (analogOutputIndex  >= settingsList.Count)
                    return new MassiveKnobSettings.DigitalToAnalogSettings();

                return settingsList[analogOutputIndex].DigitalToAnalog?.Clone() ?? new MassiveKnobSettings.DigitalToAnalogSettings();
            }
        }

        
        public void UpdateDigitalToAnalogSettings(int analogOutputIndex, Action<MassiveKnobSettings.DigitalToAnalogSettings> applyChanges)
        {
            lock (settingsLock)
            {
                var settingsList = GetActionSettingsList(MassiveKnobActionType.OutputAnalog);
                while (analogOutputIndex >= settingsList.Count)
                    settingsList.Add(null);

                if (settingsList[analogOutputIndex] == null)
                    settingsList[analogOutputIndex] = new MassiveKnobSettings.ActionSettings();

                if (settingsList[analogOutputIndex].DigitalToAnalog == null)
                    settingsList[analogOutputIndex].DigitalToAnalog = new MassiveKnobSettings.DigitalToAnalogSettings();

                applyChanges(settingsList[analogOutputIndex].DigitalToAnalog);
            }
            
            FlushSettings();
            
            if (digitalToAnalogOutputValues.TryGetValue(analogOutputIndex, out var on))
                SetDigitalToAnalogOutput(null, analogOutputIndex, on, true);
        }


        public MassiveKnobSettings GetSettings()
        {
            lock (settingsLock)
            {
                return settings.Clone();
            }
        }


        public void UpdateSettings(Action<MassiveKnobSettings> applyChanges)
        {
            lock (settingsLock)
            {
                applyChanges(settings);
            }
            
            FlushSettings();
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
                        settings.Device = new MassiveKnobSettings.DeviceSettings
                        {
                            DeviceId = device.DeviceId,
                            Settings = null
                        };
                    }
                }
                
                FlushSettings();
            }

            ActiveDevice?.Instance.Dispose();
            SetDeviceStatus(null, MassiveKnobDeviceStatus.Disconnected);

            // TODO (must have) exception handling!
            if (device != null)
            {
                var instance = device.Create(new SerilogLoggerProvider(logger.ForContext("Context", new { Device = device.DeviceId })).CreateLogger(null));
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
                    settings.Device = new MassiveKnobSettings.DeviceSettings
                    {
                        DeviceId = device.DeviceId
                    };

                settings.Device.Settings = JObject.FromObject(deviceSettings);
            }

            FlushSettings();
        }


        protected void SetDeviceStatus(IMassiveKnobDeviceContext context, MassiveKnobDeviceStatus status)
        {
            if (context != null && context != activeDeviceContext)
                return;

            lock (settingsLock)
            {
                if (status == DeviceStatus)
                    return;

                DeviceStatus = status;
            }

            deviceStatusSubject.OnNext(status);
        }


        protected T GetActionSettings<T>(IMassiveKnobActionContext context, IMassiveKnobAction action, int index) where T : class, new()
        {
            lock (settingsLock)
            {
                var list = GetActionMappingList(action.ActionType);
                if (index >= list.Count)
                    return new T();
                
                if (list[index]?.Context != context)
                    throw new InvalidOperationException("Caller must be the active action to retrieve the settings");

                var settingsList = GetActionSettingsList(action.ActionType);
                if (index >= settingsList.Count)
                    return new T();

                return settingsList[index].Settings?.ToObject<T>() ?? new T();
            }    
        }


        protected void SetActionSettings<T>(IMassiveKnobActionContext context, IMassiveKnobAction action, int index, T actionSettings) where T : class, new()
        {
            lock (settingsLock)
            {
                var list = GetActionMappingList(action.ActionType);
                if (index >= list.Count)
                    return;

                if (list[index]?.Context != context)
                    throw new InvalidOperationException("Caller must be the active action to retrieve the settings");

                var settingsList = GetActionSettingsList(action.ActionType);
                
                while (index >= settingsList.Count)
                    settingsList.Add(null);

                if (settingsList[index] == null)
                    settingsList[index] = new MassiveKnobSettings.ActionSettings
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

            IMassiveKnobAnalogAction analogAction;
            
            lock (settingsLock)
            {
                var mapping = GetActionMappingList(MassiveKnobActionType.InputAnalog);
                if (mapping == null || analogInputIndex >= mapping.Count || mapping[analogInputIndex] == null)
                    return;


                analogAction = mapping[analogInputIndex].ActionInfo.Instance as IMassiveKnobAnalogAction;
            }

            analogAction?.AnalogChanged(value);
        }


        protected void DigitalChanged(IMassiveKnobDeviceContext context, int digitalInputIndex, bool on)
        {
            if (context != activeDeviceContext)
                return;

            IMassiveKnobDigitalAction digitalAction;

            lock (settingsLock)
            {
                var mapping = GetActionMappingList(MassiveKnobActionType.InputDigital);
                if (mapping == null || digitalInputIndex >= mapping.Count || mapping[digitalInputIndex] == null)
                    return;

                digitalAction = mapping[digitalInputIndex].ActionInfo.Instance as IMassiveKnobDigitalAction;
            }

            digitalAction?.DigitalChanged(on);
        }


        public void SetAnalogOutput(IMassiveKnobActionContext context, int index, byte value, bool force)
        {
            IMassiveKnobDeviceInstance deviceInstance;

            lock (settingsLock)
            {
                if (!force && analogOutputValues.TryGetValue(index, out var currentValue) && currentValue == value)
                    return;

                analogOutputValues[index] = value;


                if (activeDevice == null)
                    return;

                var list = GetActionMappingList(MassiveKnobActionType.OutputAnalog);
                if (index >= list.Count)
                    return;

                if (context != null && list[index]?.Context != context)
                    return;

                deviceInstance = activeDevice.Instance;
            }
            
            deviceInstance.SetAnalogOutput(index, value);
        }


        public void SetDigitalToAnalogOutput(IMassiveKnobActionContext context, int index, bool on, bool force)
        {
            IMassiveKnobDeviceInstance deviceInstance;
            MassiveKnobSettings.DigitalToAnalogSettings digitalToAnalogSettings = null;

            lock (settingsLock)
            {
                if (!force && digitalToAnalogOutputValues.TryGetValue(index, out var currentValue) && currentValue == on)
                    return;

                digitalToAnalogOutputValues[index] = on;


                if (activeDevice == null)
                    return;

                var list = GetActionMappingList(MassiveKnobActionType.OutputAnalog);
                if (index >= list.Count)
                    return;

                if (context != null && list[index]?.Context != context)
                    return;

                var settingsList = GetActionSettingsList(MassiveKnobActionType.OutputAnalog);
                if (index < settingsList.Count)
                    digitalToAnalogSettings = settingsList[index].DigitalToAnalog;

                deviceInstance = activeDevice.Instance;
            }

            deviceInstance.SetAnalogOutput(index, on 
                ? digitalToAnalogSettings?.OnValue ?? 100
                : digitalToAnalogSettings?.OffValue ?? 0);
        }
        

        public void SetDigitalOutput(IMassiveKnobActionContext context, int index, bool on, bool force)
        {
            IMassiveKnobDeviceInstance deviceInstance;

            lock (settingsLock)
            {
                if (!force && digitalOutputValues.TryGetValue(index, out var currentValue) && currentValue == on)
                    return;

                digitalOutputValues[index] = on;


                if (activeDevice == null)
                    return;

                var list = GetActionMappingList(MassiveKnobActionType.OutputDigital);
                if (index >= list.Count)
                    return;

                if (context != null && list[index]?.Context != context)
                    return;

                deviceInstance = activeDevice.Instance;
            }

            deviceInstance.SetDigitalOutput(index, on);
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

        
        private List<MassiveKnobSettings.ActionSettings> GetActionSettingsList(MassiveKnobActionType actionType)
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
            MassiveKnobSettings massiveKnobSettingsSnapshot;
            
            lock (settingsLock)
            { 
                massiveKnobSettingsSnapshot = settings.Clone();
            }
            
            flushSettingsQueue.Enqueue(async () =>
            {
                await MassiveKnobSettingsJsonSerializer.Serialize(massiveKnobSettingsSnapshot);
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
                UpdateMapping(analogInputs, specs.AnalogInputCount, MassiveKnobActionType.InputAnalog, settings.AnalogInput, DelayedInitialize);
                UpdateMapping(digitalInputs, specs.DigitalInputCount, MassiveKnobActionType.InputDigital, settings.DigitalInput, DelayedInitialize);
                UpdateMapping(analogOutputs, specs.AnalogOutputCount, MassiveKnobActionType.OutputAnalog, settings.AnalogOutput, DelayedInitialize);
                UpdateMapping(digitalOutputs, specs.DigitalOutputCount, MassiveKnobActionType.OutputDigital, settings.DigitalOutput, DelayedInitialize);
            }

            foreach (var delayedInitializeAction in delayedInitializeActions)
                delayedInitializeAction();


            ActiveDevice = new MassiveKnobDeviceInfo(
                ActiveDevice.Info,
                ActiveDevice.Instance,
                specs);


            // Send out all cached values to initialize the device's outputs
            foreach (var pair in analogOutputValues.Where(pair => pair.Key < specs.AnalogOutputCount))
                SetAnalogOutput(null, pair.Key, pair.Value, true);

            foreach (var pair in digitalOutputValues.Where(pair => pair.Key < specs.DigitalOutputCount))
                SetDigitalOutput(null, pair.Key, pair.Value, true);

            foreach (var pair in digitalToAnalogOutputValues.Where(pair => pair.Key < specs.AnalogOutputCount))
                SetDigitalToAnalogOutput(null, pair.Key, pair.Value, true);
        }


        private void UpdateMapping(List<ActionMapping> mapping, int newCount, MassiveKnobActionType assignedActionType, List<MassiveKnobSettings.ActionSettings> actionSettings, Action<IMassiveKnobActionInstance, IMassiveKnobActionContext> initializeOutsideLock)
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
                        mapping.Add(CreateActionMapping(assignedActionType, action, actionIndex, initializeOutsideLock));
                    }
                    else
                        mapping.Add(null);
                }
            }
        }


        private ActionMapping CreateActionMapping(MassiveKnobActionType assignedActionType, IMassiveKnobAction action, int index, Action<IMassiveKnobActionInstance, IMassiveKnobActionContext> initialize)
        {
            if (action == null)
                return null;

            var actionLogger = logger.ForContext("Context", 
                new
                {
                    Action = action.ActionId,
                    ActionType = assignedActionType,
                    Index = index
                });
            
            var instance = action.Create(new SerilogLoggerProvider(actionLogger).CreateLogger(null));
            var context = new ActionContext(this, action, index, assignedActionType);

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
                owner.SetDeviceStatus(this, MassiveKnobDeviceStatus.Connecting);
            }


            public void Connected(DeviceSpecs specs)
            {
                owner.SetDeviceStatus(this, MassiveKnobDeviceStatus.Connected);
                owner.UpdateActiveDeviceSpecs(this, specs);
            }


            public void Disconnected()
            {
                owner.SetDeviceStatus(this, MassiveKnobDeviceStatus.Disconnected);
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
            private readonly MassiveKnobActionType assignedActionType;

            public ActionContext(MassiveKnobOrchestrator owner, IMassiveKnobAction action, int index, MassiveKnobActionType assignedActionType)
            {
                this.owner = owner;
                this.action = action;
                this.index = index;
                this.assignedActionType = assignedActionType;
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
                owner.SetAnalogOutput(this, index, value, false);
            }
            
            
            public void SetDigitalOutput(bool on)
            {
                if (assignedActionType == MassiveKnobActionType.OutputAnalog)
                    owner.SetDigitalToAnalogOutput(this, index, on, false);
                else
                    owner.SetDigitalOutput(this, index, on, false);
            }
        }
    }
}
