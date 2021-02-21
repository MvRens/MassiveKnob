using System;
using System.Windows.Controls;
using MassiveKnob.Plugin.MockDevice.Settings;

namespace MassiveKnob.Plugin.MockDevice.Devices
{
    public class MockDevice : IMassiveKnobDevice
    {
        public Guid DeviceId { get; } = new Guid("e1a4977a-abf4-4c75-a17d-fd8d3a8451ff");
        public string Name { get; } = "Mock device";
        public string Description { get; } = "Emulates the actual device but does not communicate with anything.";
        
        public IMassiveKnobDeviceInstance Create(IMassiveKnobContext context)
        {
            return new Instance(context);
        }


        private class Instance : IMassiveKnobDeviceInstance
        {
            public Instance(IMassiveKnobContext context)
            {
                // TODO read settings
            }


            public void Dispose()
            {
            }

            
            public UserControl CreateSettingsControl()
            {
                // TODO pass context
                return new MockDeviceSettings();
            }
        }


        private class Settings
        {
            // TODO interval, etc.
        }
    }
}
