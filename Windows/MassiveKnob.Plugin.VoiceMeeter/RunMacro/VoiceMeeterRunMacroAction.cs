using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Extensions.Logging;
using Voicemeeter;

namespace MassiveKnob.Plugin.VoiceMeeter.RunMacro
{
    public class VoiceMeeterRunMacroAction : IMassiveKnobAction
    {
        public Guid ActionId { get; } = new Guid("3bf41e96-9418-4a0e-ba5f-580e0b94dcce");
        public MassiveKnobActionType ActionType { get; } = MassiveKnobActionType.InputDigital;
        public string Name { get; } = Strings.RunMacroName;
        public string Description { get; } = Strings.RunMacroDescription;
        
        
        public IMassiveKnobActionInstance Create(ILogger logger)
        {
            return new Instance();
        }
        
        
        private class Instance : IMassiveKnobDigitalAction, IVoiceMeeterAction
        {
            private IMassiveKnobActionContext actionContext;
            private VoiceMeeterRunMacroActionSettings settings;
            private VoiceMeeterRunMacroActionSettingsViewModel viewModel;


            public void Initialize(IMassiveKnobActionContext context)
            {
                actionContext = context;
                settings = context.GetSettings<VoiceMeeterRunMacroActionSettings>();
                
                InstanceRegister.Register(this);
            }

            
            public void Dispose()
            {
                InstanceRegister.Unregister(this);
            }


            public UserControl CreateSettingsControl()
            {
                viewModel = new VoiceMeeterRunMacroActionSettingsViewModel(settings);
                viewModel.PropertyChanged += (sender, args) =>
                {
                    if (!viewModel.IsSettingsProperty(args.PropertyName))
                        return;
                    
                    actionContext.SetSettings(settings);
                };

                viewModel.Disposed += (sender, args) =>
                {
                    if (sender == viewModel)
                        viewModel = null;
                };

                return new VoiceMeeterRunMacroActionSettingsView(viewModel);
            }

            
            public async ValueTask DigitalChanged(bool on)
            {
                if (!on)
                    return;

                if (settings.Version == RunVoicemeeterParam.None || string.IsNullOrEmpty(settings.Script))
                    return;

                await InstanceRegister.InitializeVoicemeeter();
                global::VoiceMeeter.Remote.SetParameters(settings.Script);
            }

            
            public void VoiceMeeterVersionChanged()
            {
                viewModel?.VoiceMeeterVersionChanged();
                actionContext.SetSettings(settings);
            }
        }
    }
}
