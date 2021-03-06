using System;
using MassiveKnob.Plugin;
using MassiveKnob.Settings;

namespace MassiveKnob.Core
{
    public interface IMassiveKnobOrchestrator : IDisposable
    {
        MassiveKnobDeviceInfo ActiveDevice { get; }
        IObservable<MassiveKnobDeviceInfo> ActiveDeviceSubject { get; }

        MassiveKnobDeviceInfo SetActiveDevice(IMassiveKnobDevice device);

        MassiveKnobActionInfo GetAction(MassiveKnobActionType actionType, int index);
        MassiveKnobActionInfo SetAction(MassiveKnobActionType actionType, int index, IMassiveKnobAction action);

        MassiveKnobSettings GetSettings();
        void UpdateSettings(Action<MassiveKnobSettings> applyChanges);
    }
    
    
    public class MassiveKnobDeviceInfo 
    {
        public IMassiveKnobDevice Info { get; }
        public IMassiveKnobDeviceInstance Instance { get; }
        public DeviceSpecs? Specs { get; }

        public MassiveKnobDeviceInfo(IMassiveKnobDevice info, IMassiveKnobDeviceInstance instance, DeviceSpecs? specs)
        {
            Info = info;
            Instance = instance;
            Specs = specs;
        }
    }


    public class MassiveKnobActionInfo
    {
        public IMassiveKnobAction Info { get; }
        public IMassiveKnobActionInstance Instance { get; }

        public MassiveKnobActionInfo(IMassiveKnobAction info, IMassiveKnobActionInstance instance)
        {
            Info = info;
            Instance = instance;
        }
    }
}
