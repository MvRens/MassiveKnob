using System;
using System.Windows.Controls;
using MassiveKnob.Plugin.SerialDevice.Settings;
using MassiveKnob.Plugin.SerialDevice.Worker;

namespace MassiveKnob.Plugin.SerialDevice.Devices
{
    public class SerialDevice : IMassiveKnobDevice
    {
        public Guid DeviceId { get; } = new Guid("65255f25-d8f6-426b-8f12-cf03c44a1bf5");
        public string Name { get; } = "Serial device";
        public string Description { get; } = "A Serial (USB) device which implements the Massive Knob Protocol.";

        public IMassiveKnobDeviceInstance Create()
        {
            return new Instance();
        }


        private class Instance : IMassiveKnobDeviceInstance
        {
            private IMassiveKnobDeviceContext deviceContext;
            private SerialDeviceSettings settings;
            private SerialWorker worker;

            public void Initialize(IMassiveKnobDeviceContext context)
            {
                deviceContext = context;
                settings = deviceContext.GetSettings<SerialDeviceSettings>();

                worker = new SerialWorker(context);
                ApplySettings();
            }


            public void Dispose()
            {
                worker.Dispose();
            }


            private void ApplySettings()
            {
                worker.Connect(settings.PortName, settings.BaudRate);
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
        }
    }
}
