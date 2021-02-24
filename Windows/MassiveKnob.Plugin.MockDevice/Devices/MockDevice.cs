using System;
using System.Threading;
using System.Windows.Controls;
using MassiveKnob.Plugin.MockDevice.Settings;

namespace MassiveKnob.Plugin.MockDevice.Devices
{
    public class MockDevice : IMassiveKnobDevice
    {
        public Guid DeviceId { get; } = new Guid("e1a4977a-abf4-4c75-a17d-fd8d3a8451ff");
        public string Name { get; } = "Mock device";
        public string Description { get; } = "Emulates the actual device but does not communicate with anything.";

        public IMassiveKnobDeviceInstance Create()
        {
            return new Instance();
        }


        private class Instance : IMassiveKnobDeviceInstance
        {
            private IMassiveKnobDeviceContext deviceContext;
            private MockDeviceSettings settings;
            private Timer inputChangeTimer;

            private int reportedAnalogInputCount;
            private int reportedDigitalInputCount;
            private readonly Random random = new Random();
            
            
            public void Initialize(IMassiveKnobDeviceContext context)
            {
                deviceContext = context;
                settings = deviceContext.GetSettings<MockDeviceSettings>();
                
                ApplySettings();
            }


            public void Dispose()
            {
                inputChangeTimer?.Dispose();
            }


            private void ApplySettings()
            {
                if (settings.AnalogCount != reportedAnalogInputCount ||
                    settings.DigitalCount != reportedDigitalInputCount)
                {
                    deviceContext.Connected(new DeviceSpecs(settings.AnalogCount, settings.DigitalCount, 0, 0));

                    reportedAnalogInputCount = settings.AnalogCount;
                    reportedDigitalInputCount = settings.DigitalCount;
                }


                var interval = TimeSpan.FromSeconds(Math.Max(settings.Interval, 1));

                if (inputChangeTimer == null)
                    inputChangeTimer = new Timer(Tick, null, interval, interval);
                else
                    inputChangeTimer.Change(interval, interval);
            }

            
            public UserControl CreateSettingsControl()
            {
                var viewModel = new MockDeviceSettingsViewModel(settings);
                viewModel.PropertyChanged += (sender, args) =>
                {
                    deviceContext.SetSettings(settings);
                    ApplySettings();
                };
                
                return new MockDeviceSettingsView(viewModel);
            }
            
            
            private void Tick(object state)
            {
                var totalInputCount = reportedAnalogInputCount + reportedDigitalInputCount;
                if (totalInputCount == 0)
                    return;
                
                var changeInput = random.Next(0, totalInputCount);
                
                if (changeInput < reportedAnalogInputCount)
                    deviceContext.AnalogChanged(changeInput, (byte)random.Next(0, 101));
                else
                    deviceContext.DigitalChanged(changeInput - reportedAnalogInputCount, random.Next(0, 2) == 1);
            }
        }
    }
}
