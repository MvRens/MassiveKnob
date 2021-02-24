using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using AudioSwitcher.AudioApi;
using MassiveKnob.Plugin.CoreAudio.OSD;

namespace MassiveKnob.Plugin.CoreAudio.SetVolume
{
    public class DeviceSetVolumeAction : IMassiveKnobAction
    {
        public Guid ActionId { get; } = new Guid("aabd329c-8be5-4d1e-90ab-5114143b21dd");
        public MassiveKnobActionType ActionType { get; } = MassiveKnobActionType.InputAnalog;
        public string Name { get; } = Strings.SetVolumeName;
        public string Description { get; } = Strings.SetVolumeDescription;
        
        
        public IMassiveKnobActionInstance Create()
        {
            return new Instance();
        }
        
        
        private class Instance : IMassiveKnobAnalogAction
        {
            private IMassiveKnobActionContext actionContext;
            private DeviceSetVolumeActionSettings settings;
            private IDevice playbackDevice;


            public void Initialize(IMassiveKnobActionContext context)
            {
                actionContext = context;
                settings = context.GetSettings<DeviceSetVolumeActionSettings>();
                ApplySettings();
            }

            
            public void Dispose()
            {
            }


            private void ApplySettings()
            {
                var coreAudioController = CoreAudioControllerInstance.Acquire();
                playbackDevice = settings.DeviceId.HasValue ? coreAudioController.GetDevice(settings.DeviceId.Value) : null;
            }


            public UserControl CreateSettingsControl()
            {
                var viewModel = new DeviceSetVolumeActionSettingsViewModel(settings);
                viewModel.PropertyChanged += (sender, args) =>
                {
                    if (!viewModel.IsSettingsProperty(args.PropertyName))
                        return;
                    
                    actionContext.SetSettings(settings);
                    ApplySettings();
                };

                return new DeviceSetVolumeActionSettingsView(viewModel);
            }

            
            public async ValueTask AnalogChanged(byte value)
            {
                if (playbackDevice == null)
                    return;
                
                await playbackDevice.SetVolumeAsync(value);

                if (settings.OSD)
                    OSDManager.Show(playbackDevice);
            }
        }
    }
}
