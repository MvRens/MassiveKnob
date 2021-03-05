using System;
using System.Windows.Controls;
using MassiveKnob.Plugin.EmulatorDevice.Settings;
using Microsoft.Extensions.Logging;

namespace MassiveKnob.Plugin.EmulatorDevice.Devices
{
    public class EmulatorDevice : IMassiveKnobDevice
    {
        public Guid DeviceId { get; } = new Guid("e1a4977a-abf4-4c75-a17d-fd8d3a8451ff");
        public string Name { get; } = "Emulator";
        public string Description { get; } = "Emulates an actual device but does not communicate with anything.";

        public IMassiveKnobDeviceInstance Create(ILogger logger)
        {
            return new Instance();
        }


        private class Instance : IMassiveKnobDeviceInstance
        {
            private IMassiveKnobDeviceContext deviceContext;
            private EmulatorDeviceSettings settings;

            private DeviceSpecs reportedSpecs;
            private EmulatorDeviceWindow window;
            private EmulatorDeviceWindowViewModel windowViewModel;
            
            
            public void Initialize(IMassiveKnobDeviceContext context)
            {
                deviceContext = context;
                settings = deviceContext.GetSettings<EmulatorDeviceSettings>();

                windowViewModel = new EmulatorDeviceWindowViewModel(settings, context);
                window = new EmulatorDeviceWindow(windowViewModel);
                ApplySettings();
            }


            public void Dispose()
            {
                window.Close();
            }


            private void ApplySettings()
            {
                if (settings.AnalogInputCount != reportedSpecs.AnalogInputCount ||
                    settings.DigitalInputCount != reportedSpecs.DigitalInputCount ||
                    settings.AnalogOutputCount != reportedSpecs.AnalogOutputCount ||
                    settings.DigitalOutputCount != reportedSpecs.DigitalOutputCount)
                {
                    reportedSpecs = new DeviceSpecs(
                        settings.AnalogInputCount, settings.DigitalInputCount,
                        settings.AnalogOutputCount, settings.DigitalOutputCount);
                    
                    deviceContext.Connected(reportedSpecs);
                }

                windowViewModel.ApplySettings();
                window.Show();
            }

            
            public UserControl CreateSettingsControl()
            {
                var viewModel = new EmulatorDeviceSettingsViewModel(settings);
                viewModel.PropertyChanged += (sender, args) =>
                {
                    deviceContext.SetSettings(settings);
                    ApplySettings();
                };
                
                return new EmulatorDeviceSettingsView(viewModel);
            }

            
            public void SetAnalogOutput(int analogOutputIndex, byte value)
            {
                if (analogOutputIndex >= windowViewModel.AnalogOutputCount)
                    return;

                windowViewModel.AnalogOutputs[analogOutputIndex].AnalogValue = value;
            }

            
            public void SetDigitalOutput(int digitalOutputIndex, bool on)
            {
                if (digitalOutputIndex >= windowViewModel.DigitalOutputCount)
                    return;

                windowViewModel.DigitalOutputs[digitalOutputIndex].DigitalValue = on;
            }
        }
    }
}
