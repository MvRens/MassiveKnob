using System;
using System.Windows.Controls;
using AudioSwitcher.AudioApi;
using MassiveKnob.Plugin.CoreAudio.OSD;
using Microsoft.Extensions.Logging;

namespace MassiveKnob.Plugin.CoreAudio.GetDefault
{
    public class DeviceGetDefaultAction : IMassiveKnobAction
    {
        public Guid ActionId { get; } = new Guid("3c427e28-493f-489f-abb3-1a7ef23ca6c9");
        public MassiveKnobActionType ActionType { get; } = MassiveKnobActionType.OutputDigital;
        public string Name { get; } = Strings.GetDefaultName;
        public string Description { get; } = Strings.GetDefaultDescription;
        
        
        public IMassiveKnobActionInstance Create(ILogger logger)
        {
            return new Instance();
        }
        
        
        private class Instance : IMassiveKnobActionInstance
        {
            private IMassiveKnobActionContext actionContext;
            private DeviceGetDefaultActionSettings settings;
            private IDevice playbackDevice;
            private IDisposable deviceChanged;


            public void Initialize(IMassiveKnobActionContext context)
            {
                actionContext = context;
                settings = context.GetSettings<DeviceGetDefaultActionSettings>();
                ApplySettings();
            }

            
            public void Dispose()
            {
                deviceChanged?.Dispose();
            }


            private void ApplySettings()
            {
                if (playbackDevice == null || playbackDevice.Id != settings.DeviceId)
                {
                    var coreAudioController = CoreAudioControllerInstance.Acquire();
                    playbackDevice = settings.DeviceId.HasValue
                        ? coreAudioController.GetDevice(settings.DeviceId.Value)
                        : null;

                    deviceChanged?.Dispose();
                    deviceChanged = playbackDevice?.PropertyChanged.Subscribe(PropertyChanged);
                }

                CheckActive();
            }


            public UserControl CreateSettingsControl()
            {
                var viewModel = new DeviceGetDefaultActionSettingsViewModel(settings);
                viewModel.PropertyChanged += (sender, args) =>
                {
                    if (!viewModel.IsSettingsProperty(args.PropertyName))
                        return;
                    
                    actionContext.SetSettings(settings);
                    ApplySettings();
                };

                return new DeviceGetDefaultActionSettingsView(viewModel);
            }


            private void PropertyChanged(DevicePropertyChangedArgs args)
            {
                if (args.ChangedType != DeviceChangedType.DefaultChanged)
                    return;

                CheckActive();
                
                // TODO (should have) OSD for changing default
                //if (settings.OSD)
                    //OSDManager.Show(args.Device);
            }
            
            
            private void CheckActive()
            {
                if (playbackDevice == null)
                    return;

                var isDefault = (settings.Playback && playbackDevice.IsDefaultDevice) ||
                                (settings.Communications && playbackDevice.IsDefaultCommunicationsDevice);
                
                actionContext.SetDigitalOutput(settings.Inverted ? !isDefault : isDefault);
            }
        }
    }
}
