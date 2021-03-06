using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Extensions.Logging;
using Voicemeeter;

namespace MassiveKnob.Plugin.VoiceMeeter.GetParameter
{
    public class VoiceMeeterGetParameterAction : IMassiveKnobAction
    {
        public Guid ActionId { get; } = new Guid("4904fffb-aaec-4f19-88bb-49f6ed38c3ec");
        public MassiveKnobActionType ActionType { get; } = MassiveKnobActionType.OutputDigital;
        public string Name { get; } = Strings.GetParameterName;
        public string Description { get; } = Strings.GetParameterDescription;
        
        
        public IMassiveKnobActionInstance Create(ILogger logger)
        {
            return new Instance();
        }
        
        
        private class Instance : IMassiveKnobActionInstance, IVoiceMeeterAction
        {
            private IMassiveKnobActionContext actionContext;
            private VoiceMeeterGetParameterActionSettings settings;
            private Parameters parameters;
            private IDisposable parameterChanged;


            public void Initialize(IMassiveKnobActionContext context)
            {
                actionContext = context;
                settings = context.GetSettings<VoiceMeeterGetParameterActionSettings>();
                ApplySettings();

                InstanceRegister.Register(this);
            }


            public void Dispose()
            {
                InstanceRegister.Unregister(this);
                parameterChanged?.Dispose();
                parameters?.Dispose();
            }


            private void ApplySettings()
            {
                if (InstanceRegister.Version == RunVoicemeeterParam.None)
                    return;

                if (parameters == null)
                    parameters = new Parameters();

                if (string.IsNullOrEmpty(settings.Parameter))
                {
                    parameterChanged?.Dispose();
                    parameterChanged = null;
                }

                if (parameterChanged == null)
                    parameterChanged = parameters.Subscribe(x => ParametersChanged());

                // TODO directly update output depending on value
                /*
                if (playbackDevice != null)
                    actionContext.SetDigitalOutput(settings.Inverted ? !playbackDevice.IsMuted : playbackDevice.IsMuted);
                */
            }


            public UserControl CreateSettingsControl()
            {
                var viewModel = new VoiceMeeterGetParameterActionSettingsViewModel(settings);
                viewModel.PropertyChanged += (sender, args) =>
                {
                    if (!viewModel.IsSettingsProperty(args.PropertyName))
                        return;
                    
                    actionContext.SetSettings(settings);
                    ApplySettings();
                };

                return new VoiceMeeterGetParameterActionSettingsView(viewModel);
            }
        

            public void VoiceMeeterVersionChanged()
            {
                // TODO update viewModel
                // TODO reset parameterChanged subscription

                actionContext.SetSettings(settings);
            }


            private void ParametersChanged()
            {
                if (InstanceRegister.Version == RunVoicemeeterParam.None || string.IsNullOrEmpty(settings.Parameter))
                    return;

                // TODO if another task is already running, wait / chain
                // TODO only start task if not yet initialized
                Task.Run(async () =>
                {
                    await InstanceRegister.InitializeVoicemeeter();
                    bool on;

                    if (float.TryParse(settings.Value, out var settingsFloatValue))
                    {
                        try
                        {
                            // Even on/off values are returned as floating point "1.000" in text form,
                            // so try to compare in native format first
                            var floatValue = global::VoiceMeeter.Remote.GetParameter(settings.Parameter);
                            on = Math.Abs(settingsFloatValue - floatValue) < 0.001;
                        }
                        catch
                        {
                            // Fall back to text comparison
                            var value = global::VoiceMeeter.Remote.GetTextParameter(settings.Parameter);
                            on = string.Equals(value, settings.Value, StringComparison.InvariantCultureIgnoreCase);
                        }
                    }
                    else
                    {
                        var value = global::VoiceMeeter.Remote.GetTextParameter(settings.Parameter);
                        on = string.Equals(value, settings.Value, StringComparison.InvariantCultureIgnoreCase);
                    }

                    // TODO check specific parameter for changes, not just any parameter
                    actionContext.SetDigitalOutput(settings.Inverted ? !on : on);
                });
            }
        }
    }
}
