using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MassiveKnob.Plugin.CoreAudio.Actions
{
    public class DeviceVolumeAction : IMassiveKnobAction
    {
        public Guid ActionId { get; } = new Guid("aabd329c-8be5-4d1e-90ab-5114143b21dd");
        public MassiveKnobActionType ActionType { get; } = MassiveKnobActionType.InputAnalog;
        public string Name { get; } = "Set volume";
        public string Description { get; } = "Sets the volume for the selected device, regardless of the current default device.";
        
        
        public IMassiveKnobActionInstance Create(IMassiveKnobActionContext context)
        {
            return new Instance(context);
        }
        
        
        private class Instance : IMassiveKnobAnalogAction
        {
            private readonly Settings settings;

            
            public Instance(IMassiveKnobContext context)
            {
                settings = context.GetSettings<Settings>();
            }
            
            
            public void Dispose()
            {
            }

            
            public UserControl CreateSettingsControl()
            {
                return null;
            }


            public ValueTask AnalogChanged(byte value)
            {
                // TODO set volume
                return default;
            }
        }


        private class Settings
        {

        }
    }
}
