using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using AudioSwitcher.AudioApi;
using Microsoft.Extensions.Logging;

namespace MassiveKnob.Plugin.CoreAudio.SetDefault
{
    public class DeviceSetDefaultAction : IMassiveKnobAction
    {
        public Guid ActionId { get; } = new Guid("b76f1eb7-2419-42b4-9de4-9bfe6f65a841");
        public MassiveKnobActionType ActionType { get; } = MassiveKnobActionType.InputDigital;
        public string Name { get; } = Strings.SetDefaultName;
        public string Description { get; } = Strings.SetDefaultDescription;
        
        
        public IMassiveKnobActionInstance Create(ILogger logger)
        {
            return new Instance();
        }
        
        
        private class Instance : IMassiveKnobDigitalAction
        {
            private IMassiveKnobActionContext actionContext;
            private DeviceSetDefaultActionSettings settings;
            private IDevice playbackDevice;


            public void Initialize(IMassiveKnobActionContext context)
            {
                actionContext = context;
                settings = context.GetSettings<DeviceSetDefaultActionSettings>();
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
                var viewModel = new DeviceSetDefaultActionSettingsViewModel(settings);
                viewModel.PropertyChanged += (sender, args) =>
                {
                    if (!viewModel.IsSettingsProperty(args.PropertyName))
                        return;
                    
                    actionContext.SetSettings(settings);
                    ApplySettings();
                };

                return new DeviceSetDefaultActionSettingsView(viewModel);
            }

            
            public async ValueTask DigitalChanged(bool on)
            {
                if (playbackDevice == null || !on)
                    return;

                if (settings.Playback)
                    await playbackDevice.SetAsDefaultAsync();

                if (settings.Communications)
                    await playbackDevice.SetAsDefaultCommunicationsAsync();


                // TODO OSD for default device
                //if (settings.OSD)
                    //OSDManager.Show(playbackDevice);
            }
        }
    }
}
