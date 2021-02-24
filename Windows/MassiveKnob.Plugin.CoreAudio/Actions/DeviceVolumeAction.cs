using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using AudioSwitcher.AudioApi;
using MassiveKnob.Plugin.CoreAudio.Settings;

namespace MassiveKnob.Plugin.CoreAudio.Actions
{
    public class DeviceVolumeAction : IMassiveKnobAction
    {
        public Guid ActionId { get; } = new Guid("aabd329c-8be5-4d1e-90ab-5114143b21dd");
        public MassiveKnobActionType ActionType { get; } = MassiveKnobActionType.InputAnalog;
        public string Name { get; } = "Set volume";
        public string Description { get; } = "Sets the volume for the selected device, regardless of the current default device.";
        
        
        public IMassiveKnobActionInstance Create()
        {
            return new Instance();
        }
        
        
        private class Instance : IMassiveKnobAnalogAction
        {
            private IMassiveKnobActionContext actionContext;
            private DeviceVolumeActionSettings settings;
            private IDevice playbackDevice;


            public void Initialize(IMassiveKnobActionContext context)
            {
                actionContext = context;
                settings = context.GetSettings<DeviceVolumeActionSettings>();
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
                var viewModel = new DeviceVolumeActionSettingsViewModel(settings);
                viewModel.PropertyChanged += (sender, args) =>
                {
                    if (!viewModel.IsSettingsProperty(args.PropertyName))
                        return;
                    
                    actionContext.SetSettings(settings);
                    ApplySettings();
                };

                return new DeviceVolumeActionSettingsView(viewModel);
            }

            
            public async ValueTask AnalogChanged(byte value)
            {
                if (playbackDevice == null)
                    return;
                
                await playbackDevice.SetVolumeAsync(value);
            }
        }
    }
}
