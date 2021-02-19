using System.Collections.Generic;
using System.Threading.Tasks;

namespace MassiveKnob.Hardware
{
    public abstract class AbstractMassiveKnobHardware : IMassiveKnobHardware
    {
        protected ObserverProxy Observers = new ObserverProxy();
        
        public void AttachObserver(IMassiveKnobHardwareObserver observer)
        {
            Observers.AttachObserver(observer);
        }

        public void DetachObserver(IMassiveKnobHardwareObserver observer)
        {
            Observers.DetachObserver(observer);
        }


        public abstract Task TryConnect();
        public abstract Task Disconnect();


        public class ObserverProxy : IMassiveKnobHardwareObserver
        {
            private readonly List<IMassiveKnobHardwareObserver> observers = new List<IMassiveKnobHardwareObserver>();

            
            public void AttachObserver(IMassiveKnobHardwareObserver observer)
            {
                observers.Add(observer);
            }

            public void DetachObserver(IMassiveKnobHardwareObserver observer)
            {
                observers.Remove(observer);
            }
            

            public void Connected(int knobCount)
            {
                foreach (var observer in observers)
                    observer.Connected(knobCount);
            }
            

            public void Disconnected()
            {
                foreach (var observer in observers)
                    observer.Disconnected();
            }


            public void VolumeChanged(int knob, int volume)
            {
                foreach (var observer in observers)
                    observer.VolumeChanged(knob, volume);
            }
        }
    }
}
