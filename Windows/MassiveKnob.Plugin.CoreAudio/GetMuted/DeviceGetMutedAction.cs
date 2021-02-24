using System;
using System.Windows.Controls;
using AudioSwitcher.AudioApi;
using MassiveKnob.Plugin.CoreAudio.OSD;

namespace MassiveKnob.Plugin.CoreAudio.GetMuted
{
    public class DeviceGetMutedAction : IMassiveKnobAction
    {
        public Guid ActionId { get; } = new Guid("86646ca7-f472-4c5a-8d0f-7e5d2d162ab9");
        public MassiveKnobActionType ActionType { get; } = MassiveKnobActionType.OutputDigital;
        public string Name { get; } = Strings.GetMutedName;
        public string Description { get; } = Strings.GetMutedDescription;
        
        
        public IMassiveKnobActionInstance Create()
        {
            return new Instance();
        }
        
        
        private class Instance : IMassiveKnobActionInstance
        {
            private IMassiveKnobActionContext actionContext;
            private DeviceGetMutedActionSettings settings;
            private IDevice playbackDevice;
            private IDisposable deviceChanged;


            public void Initialize(IMassiveKnobActionContext context)
            {
                actionContext = context;
                settings = context.GetSettings<DeviceGetMutedActionSettings>();
                ApplySettings();
            }

            
            public void Dispose()
            {
                deviceChanged?.Dispose();
            }


            private void ApplySettings()
            {
                if (playbackDevice != null && playbackDevice.Id == settings.DeviceId)
                    return;
                
                var coreAudioController = CoreAudioControllerInstance.Acquire();
                playbackDevice = settings.DeviceId.HasValue ? coreAudioController.GetDevice(settings.DeviceId.Value) : null;

                deviceChanged?.Dispose();
                deviceChanged = playbackDevice?.MuteChanged.Subscribe(MuteChanged);
            }


            public UserControl CreateSettingsControl()
            {
                var viewModel = new DeviceGetMutedActionSettingsViewModel(settings);
                viewModel.PropertyChanged += (sender, args) =>
                {
                    if (!viewModel.IsSettingsProperty(args.PropertyName))
                        return;
                    
                    actionContext.SetSettings(settings);
                    ApplySettings();
                };

                return new DeviceGetMutedActionSettingsView(viewModel);
            }


            private void MuteChanged(DeviceMuteChangedArgs args)
            {
                actionContext.SetDigitalOutput(settings.Inverted ? !args.IsMuted : args.IsMuted);
                
                if (settings.OSD)
                    OSDManager.Show(args.Device);
            }
        }
    }
}
