using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using MassiveKnob.Core;
using MassiveKnob.Settings;
using Serilog.Events;

namespace MassiveKnob.ViewModel.Settings
{
    public class SettingsLoggingViewModel : INotifyPropertyChanged
    {
        private readonly IMassiveKnobOrchestrator orchestrator;
        private readonly ILoggingSwitch loggingSwitch;

        // ReSharper disable UnusedMember.Global - used by WPF Binding
        public IList<LoggingLevelViewModel> LoggingLevels { get; }

        private LoggingLevelViewModel selectedLoggingLevel;
        public LoggingLevelViewModel SelectedLoggingLevel
        {
            get => selectedLoggingLevel;
            set
            {
                if (value == selectedLoggingLevel)
                    return;

                selectedLoggingLevel = value;
                OnPropertyChanged();

                ApplyLoggingSettings();
            }
        }


        private bool loggingEnabled;
        public bool LoggingEnabled
        {
            get => loggingEnabled;
            set
            {
                if (value == loggingEnabled)
                    return;

                loggingEnabled = value;
                OnPropertyChanged();

                ApplyLoggingSettings();
            }
        }


        // TODO (code quality) do not hardcode path here
        public string LoggingOutputPath { get; } = string.Format(Strings.LoggingOutputPath,
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"MassiveKnob",
                @"Logs"));
        // ReSharper restore UnusedMember.Global


        public SettingsLoggingViewModel(IMassiveKnobOrchestrator orchestrator, ILoggingSwitch loggingSwitch)
        {
            this.orchestrator = orchestrator;
            this.loggingSwitch = loggingSwitch;
            
            // For design-time support
            if (orchestrator == null)
                return;


            var logSettings = orchestrator.GetSettings().Log;
            LoggingLevels = new List<LoggingLevelViewModel>
            {
                new LoggingLevelViewModel(LogEventLevel.Error, Strings.LoggingLevelError, Strings.LoggingLevelErrorDescription),
                new LoggingLevelViewModel(LogEventLevel.Warning, Strings.LoggingLevelWarning, Strings.LoggingLevelWarningDescription),
                new LoggingLevelViewModel(LogEventLevel.Information, Strings.LoggingLevelInformation, Strings.LoggingLevelInformationDescription),
                new LoggingLevelViewModel(LogEventLevel.Verbose, Strings.LoggingLevelVerbose, Strings.LoggingLevelVerboseDescription)
            };

            selectedLoggingLevel = LoggingLevels.SingleOrDefault(l => l.Level == logSettings.Level)
                                   ?? LoggingLevels.Single(l => l.Level == LogEventLevel.Information);
            loggingEnabled = logSettings.Enabled;
        }


        private void ApplyLoggingSettings()
        {
            orchestrator?.UpdateSettings(settings =>
            {
                settings.Log.Enabled = LoggingEnabled;
                settings.Log.Level = SelectedLoggingLevel.Level;
            });
            
            loggingSwitch?.SetLogging(LoggingEnabled, selectedLoggingLevel.Level);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
