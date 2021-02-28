using System;
using System.Windows.Controls;
using AudioSwitcher.AudioApi;
using MassiveKnob.Plugin.CoreAudio.OSD;
using Microsoft.Extensions.Logging;

namespace MassiveKnob.Plugin.CoreAudio.GetVolume
{
    // TODO send out initial volume after proper initialization
    
    public class DeviceGetVolumeAction : IMassiveKnobAction
    {
        public Guid ActionId { get; } = new Guid("6ebf91af-8240-4a75-9729-c6a1eb60dcba");
        public MassiveKnobActionType ActionType { get; } = MassiveKnobActionType.OutputAnalog;
        public string Name { get; } = Strings.GetVolumeName;
        public string Description { get; } = Strings.GetVolumeDescription;
        
        
        public IMassiveKnobActionInstance Create(ILogger logger)
        {
            return new Instance();
        }
        
        
        private class Instance : IMassiveKnobActionInstance
        {
            private IMassiveKnobActionContext actionContext;
            private DeviceGetVolumeActionSettings settings;
            private IDevice playbackDevice;
            private IDisposable deviceChanged;


            public void Initialize(IMassiveKnobActionContext context)
            {
                actionContext = context;
                settings = context.GetSettings<DeviceGetVolumeActionSettings>();
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
                deviceChanged = playbackDevice?.VolumeChanged.Subscribe(VolumeChanged);
            }


            public UserControl CreateSettingsControl()
            {
                var viewModel = new DeviceGetVolumeActionSettingsViewModel(settings);
                viewModel.PropertyChanged += (sender, args) =>
                {
                    if (!viewModel.IsSettingsProperty(args.PropertyName))
                        return;
                    
                    actionContext.SetSettings(settings);
                    ApplySettings();
                };

                return new DeviceGetVolumeActionSettingsView(viewModel);
            }


            private void VolumeChanged(DeviceVolumeChangedArgs args)
            {
                actionContext.SetAnalogOutput((byte)args.Volume);

                if (settings.OSD)
                    OSDManager.Show(args.Device);
            }
        }
    }
}
