using Serilog.Events;

namespace MassiveKnob.Settings
{
    public interface ILoggingSwitch
    {
        void SetLogging(bool enabled, LogEventLevel minimumLevel);
    }
}
