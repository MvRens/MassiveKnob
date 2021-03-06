using Serilog.Events;

namespace MassiveKnob.ViewModel
{
    public class LoggingLevelViewModel
    {
        public LogEventLevel Level { get; }
        public string Name { get; }
        public string Description { get; }


        public LoggingLevelViewModel(LogEventLevel level, string name, string description)
        {
            Level = level;
            Name = name;
            Description = description;
        }
    }

}
