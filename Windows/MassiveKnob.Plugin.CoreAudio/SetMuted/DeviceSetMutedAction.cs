using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using AudioSwitcher.AudioApi;
using MassiveKnob.Plugin.CoreAudio.OSD;

namespace MassiveKnob.Plugin.CoreAudio.SetMuted
{
    public class DeviceSetMutedAction : IMassiveKnobAction
    {
        public Guid ActionId { get; } = new Guid("032eb405-a1df-4178-b2d5-6cf556305a8c");
        public MassiveKnobActionType ActionType { get; } = MassiveKnobActionType.InputDigital;
        public string Name { get; } = Strings.SetMutedName;
        public string Description { get; } = Strings.SetMutedDescription;
        
        
        public IMassiveKnobActionInstance Create()
        {
            return new Instance();
        }
        
        
        private class Instance : IMassiveKnobDigitalAction
        {
            private IMassiveKnobActionContext actionContext;
            private DeviceSetMutedActionSettings settings;
            private IDevice playbackDevice;


            public void Initialize(IMassiveKnobActionContext context)
            {
                actionContext = context;
                settings = context.GetSettings<DeviceSetMutedActionSettings>();
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
                var viewModel = new DeviceSetMutedActionSettingsViewModel(settings);
                viewModel.PropertyChanged += (sender, args) =>
                {
                    if (!viewModel.IsSettingsProperty(args.PropertyName))
                        return;
                    
                    actionContext.SetSettings(settings);
                    ApplySettings();
                };

                return new DeviceSetMutedActionSettingsView(viewModel);
            }

            
            public async ValueTask DigitalChanged(bool on)
            {
                if (playbackDevice == null)
                    return;

                if (settings.Toggle)
                {
                    if (!on)
                        return;

                    await playbackDevice.SetMuteAsync(!playbackDevice.IsMuted);
                }
                else
                    await playbackDevice.SetMuteAsync(settings.SetInverted ? !on : on);


                if (settings.OSD)
                    OSDManager.Show(playbackDevice);
            }
        }
    }
}
