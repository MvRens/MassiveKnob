using System.Threading.Tasks;

namespace MassiveKnob.Hardware
{
    public interface IMassiveKnobHardwareObserver
    {
        void Connecting();
        void Connected(int knobCount);
        void Disconnected();

        void VolumeChanged(int knob, int volume);
        // void ButtonPress(int index); -- for switching the active device, TBD
    }


    public interface IMassiveKnobHardware
    {
        void AttachObserver(IMassiveKnobHardwareObserver observer);
        void DetachObserver(IMassiveKnobHardwareObserver observer);

        Task TryConnect();
        Task Disconnect();
        // Task SetActiveKnob(int knob); -- for providing LED feedback when switching the active device, TBD
    }


    public interface IMassiveKnobHardwareFactory
    {
        IMassiveKnobHardware Create(string portName);
    }
}
