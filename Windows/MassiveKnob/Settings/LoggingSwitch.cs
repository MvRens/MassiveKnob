using System;
using Serilog.Core;
using Serilog.Events;

namespace MassiveKnob.Settings
{
    public class LoggingSwitch : LoggingLevelSwitch, ILoggingSwitch
    {
        private bool enabled;
        private LogEventLevel minimumLevel;
        
        
        public bool IsIncluded(LogEvent logEvent)
        {
            return enabled && logEvent.Level >= minimumLevel;
        }
        

        // ReSharper disable ParameterHidesMember
        public void SetLogging(bool enabled, LogEventLevel minimumLevel)
        {
            this.enabled = enabled;
            this.minimumLevel = minimumLevel;
        }
    }
}
