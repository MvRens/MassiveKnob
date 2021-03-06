﻿using System;
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
            private VoiceMeeterGetParameterActionSettingsViewModel viewModel;
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
            }


            private void ApplySettings()
            {
                if (InstanceRegister.Version == RunVoicemeeterParam.None || string.IsNullOrEmpty(settings.Parameter))
                {
                    parameterChanged?.Dispose();
                    parameterChanged = null;
                    return;
                }

                if (parameterChanged == null)
                    parameterChanged = InstanceRegister.SubscribeToParameterChanges(ParametersChanged);

                ParametersChanged();
            }


            public UserControl CreateSettingsControl()
            {
                viewModel = new VoiceMeeterGetParameterActionSettingsViewModel(settings);
                viewModel.PropertyChanged += (sender, args) =>
                {
                    if (!viewModel.IsSettingsProperty(args.PropertyName))
                        return;

                    actionContext.SetSettings(settings);
                    ApplySettings();
                };

                viewModel.Disposed += (sender, args) =>
                {
                    if (sender == viewModel)
                        viewModel = null;
                };

                return new VoiceMeeterGetParameterActionSettingsView(viewModel);
            }


            public void VoiceMeeterVersionChanged()
            {
                viewModel?.VoiceMeeterVersionChanged();

                actionContext.SetSettings(settings);
                ApplySettings();
            }


            private void ParametersChanged()
            {
                if (InstanceRegister.Version == RunVoicemeeterParam.None || string.IsNullOrEmpty(settings.Parameter))
                    return;

                InstanceRegister.InitializeVoicemeeter().ContinueWith(t =>
                {
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

                    actionContext.SetDigitalOutput(settings.Inverted ? !on : on);
                });
            }
        }
    }
}