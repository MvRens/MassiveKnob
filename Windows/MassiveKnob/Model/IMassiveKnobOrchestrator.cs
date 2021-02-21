using MassiveKnob.Plugin;

namespace MassiveKnob.Model
{
    public interface IMassiveKnobOrchestrator
    {
        IMassiveKnobDeviceInstance ActiveDeviceInstance { get; }

        IMassiveKnobDeviceInstance SetActiveDevice(IMassiveKnobDevice device);
    }
}
