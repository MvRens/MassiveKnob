using MassiveKnob.Plugin;

namespace MassiveKnob.Model
{
    public class MassiveKnobOrchestrator : IMassiveKnobOrchestrator
    {
        private readonly Settings.Settings settings;


        public IMassiveKnobDeviceInstance ActiveDeviceInstance { get; private set; }


        public MassiveKnobOrchestrator(Settings.Settings settings)
        {
            this.settings = settings;
        }
        

        public IMassiveKnobDeviceInstance SetActiveDevice(IMassiveKnobDevice device)
        {
            ActiveDeviceInstance?.Dispose();
            ActiveDeviceInstance = device?.Create(new Context(settings));
            
            return ActiveDeviceInstance;
        }



        public class Context : IMassiveKnobContext
        {
            private readonly Settings.Settings settings;
            

            public Context(Settings.Settings settings)
            {
                this.settings = settings;
            }
                    
            public T GetSettings<T>() where T : class, new()
            {
                // TODO
                return default;
            }
            

            public void SetSettings<T>(T settings) where T : class, new()
            {
                // TODO
            }
        }
    }
}
