using System;
using System.Windows.Controls;
using MassiveKnob.Plugin.SerialDevice.Settings;
using MassiveKnob.Plugin.SerialDevice.Worker;
using Microsoft.Extensions.Logging;

namespace MassiveKnob.Plugin.SerialDevice.Devices
{
    public class SerialDevice : IMassiveKnobDevice
    {
        public Guid DeviceId { get; } = new Guid("65255f25-d8f6-426b-8f12-cf03c44a1bf5");
        public string Name { get; } = "Serial device";
        public string Description { get; } = "A Serial (USB) device which implements the Massive Knob Protocol.";

        public IMassiveKnobDeviceInstance Create(ILogger logger)
        {
            return new Instance(logger);
        }


        private class Instance : IMassiveKnobDeviceInstance
        {
            private readonly ILogger logger;
            private IMassiveKnobDeviceContext deviceContext;
            private SerialDeviceSettings settings;
            private SerialWorker worker;
            
            
            public Instance(ILogger logger)
            {
                this.logger = logger;
            }
            

            public void Initialize(IMassiveKnobDeviceContext context)
            {
                deviceContext = context;
                settings = deviceContext.GetSettings<SerialDeviceSettings>();

                worker = new SerialWorker(context, logger);
                ApplySettings();
            }


            public void Dispose()
            {
                worker.Dispose();
            }


            private void ApplySettings()
            {
                worker.Connect(settings.PortName, settings.BaudRate, settings.DtrEnable);
            }

            
            public UserControl CreateSettingsControl()
            {
                var viewModel = new SerialDeviceSettingsViewModel(settings);
                viewModel.PropertyChanged += (sender, args) =>
                {
                    if (!viewModel.IsSettingsProperty(args.PropertyName))
                        return;
                    
                    deviceContext.SetSettings(settings);
                    ApplySettings();
                };
                
                return new SerialDeviceSettingsView(viewModel);
            }

            
            public void SetAnalogOutput(int analogOutputIndex, byte value)
            {
                // TODO Support SetAnalogOutput
            }

            public void SetDigitalOutput(int digitalOutputIndex, bool @on)
            {
                // TODO Support SetDigitalOutput
            }
        }
    }
}
