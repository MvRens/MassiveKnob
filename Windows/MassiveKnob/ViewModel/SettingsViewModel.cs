using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using MassiveKnob.Core;
using MassiveKnob.Plugin;
using MassiveKnob.Settings;
using MassiveKnob.View.Settings;

namespace MassiveKnob.ViewModel
{
    public class SettingsViewModel : IDisposable, INotifyPropertyChanged
    {
        private readonly Dictionary<SettingsMenuItem, Type> menuItemControls = new Dictionary<SettingsMenuItem, Type>
        {
            { SettingsMenuItem.Device, typeof(SettingsDeviceView) },
            { SettingsMenuItem.AnalogInputs, typeof(SettingsAnalogInputsView) },
            { SettingsMenuItem.DigitalInputs, typeof(SettingsDigitalInputsView) },
            { SettingsMenuItem.AnalogOutputs, typeof(SettingsAnalogOutputsView) },
            { SettingsMenuItem.DigitalOutputs, typeof(SettingsDigitalOutputsView) },
            { SettingsMenuItem.Logging, typeof(SettingsLoggingView) },
            { SettingsMenuItem.Startup, typeof(SettingsStartupView) },
            { SettingsMenuItem.Plugins, typeof(SettingsPluginsView) }
        };

        
        private readonly SimpleInjector.Container container;
        private readonly IMassiveKnobOrchestrator orchestrator;
        private UserControl selectedView;
        private SettingsMenuItem selectedMenuItem;

        private DeviceSpecs? specs;
        private readonly IDisposable activeDeviceSubscription;


        // ReSharper disable UnusedMember.Global - used by WPF Binding
        public SettingsMenuItem SelectedMenuItem
        {
            get => selectedMenuItem;
            set
            {
                if (value == selectedMenuItem)
                    return;

                selectedMenuItem = value;
                OnPropertyChanged();

                if (menuItemControls.TryGetValue(selectedMenuItem, out var viewType))
                    SelectedView = (UserControl)container?.GetInstance(viewType);
                        
                orchestrator?.UpdateSettings(settings =>
                {
                    settings.UI.ActiveMenuItem = selectedMenuItem;
                });
            }
        }

        public UserControl SelectedView
        {
            get => selectedView;
            set
            {
                if (value == selectedView)
                    return;

                selectedView = value;
                OnPropertyChanged();
            }
        }



        //public IList<ActionViewModel> Actions { get; }

        public DeviceSpecs? Specs
        {
            get => specs;
            set
            {
                specs = value;
                OnPropertyChanged();
                OnDependantPropertyChanged("AnalogInputVisibility");
                OnDependantPropertyChanged("DigitalInputVisibility");
                OnDependantPropertyChanged("AnalogOutputVisibility");
                OnDependantPropertyChanged("DigitalOutputVisibility");
                /*
                DisposeInputOutputViewModels(AnalogInputs);
                DisposeInputOutputViewModels(DigitalInputs);
                DisposeInputOutputViewModels(AnalogOutputs);
                DisposeInputOutputViewModels(DigitalOutputs);

                AnalogInputs = Enumerable
                    .Range(0, specs?.AnalogInputCount ?? 0)
                    .Select(i => new InputOutputViewModel(this, orchestrator, MassiveKnobActionType.InputAnalog, i))
                    .ToList();

                DigitalInputs = Enumerable
                    .Range(0, specs?.DigitalInputCount ?? 0)
                    .Select(i => new InputOutputViewModel(this, orchestrator, MassiveKnobActionType.InputDigital, i))
                    .ToList();

                AnalogOutputs = Enumerable
                    .Range(0, specs?.AnalogOutputCount ?? 0)
                    .Select(i => new InputOutputViewModel(this, orchestrator, MassiveKnobActionType.OutputAnalog, i))
                    .ToList();

                DigitalOutputs = Enumerable
                    .Range(0, specs?.DigitalOutputCount ?? 0)
                    .Select(i => new InputOutputViewModel(this, orchestrator, MassiveKnobActionType.OutputDigital, i))
                    .ToList();
                */
            }
        }


        public Visibility AnalogInputVisibility => specs.HasValue && specs.Value.AnalogInputCount > 0
            ? Visibility.Visible
            : Visibility.Collapsed;

        /*
        public IEnumerable<InputOutputViewModel> AnalogInputs
        {
            get => analogInputs;
            set
            {
                analogInputs = value;
                OnPropertyChanged();
            }
        }
        */

        public Visibility DigitalInputVisibility => specs.HasValue && specs.Value.DigitalInputCount > 0
            ? Visibility.Visible
            : Visibility.Collapsed;

        /*
        public IEnumerable<InputOutputViewModel> DigitalInputs
        {
            get => digitalInputs;
            set
            {
                digitalInputs = value;
                OnPropertyChanged();
            }
        }
        */

        public Visibility AnalogOutputVisibility => specs.HasValue && specs.Value.AnalogOutputCount > 0
            ? Visibility.Visible
            : Visibility.Collapsed;

        /*
        public IEnumerable<InputOutputViewModel> AnalogOutputs
        {
            get => analogOutputs;
            set
            {
                analogOutputs = value;
                OnPropertyChanged();
            }
        }
        */

        public Visibility DigitalOutputVisibility => specs.HasValue && specs.Value.DigitalOutputCount > 0
            ? Visibility.Visible
            : Visibility.Collapsed;

        /*
        public IEnumerable<InputOutputViewModel> DigitalOutputs
        {
            get => digitalOutputs;
            set
            {
                digitalOutputs = value;
                OnPropertyChanged();
            }
        }

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


        private bool runAtStartup;
        public bool RunAtStartup
        {
            get => runAtStartup;
            set
            {
                if (value == runAtStartup)
                    return;

                runAtStartup = value;
                OnPropertyChanged();

                ApplyRunAtStartup();
            }
        }
        */
        // ReSharper restore UnusedMember.Global


        public SettingsViewModel(SimpleInjector.Container container, /*IPluginManager pluginManager, */IMassiveKnobOrchestrator orchestrator/*, ILoggingSwitch loggingSwitch*/)
        {
            this.container = container;
            this.orchestrator = orchestrator;
            //this.loggingSwitch = loggingSwitch;
            
            // For design-time support
            if (orchestrator == null)
                return;

            var activeMenuItem = orchestrator.GetSettings().UI.ActiveMenuItem;
            if (activeMenuItem == SettingsMenuItem.None)
                activeMenuItem = SettingsMenuItem.Device;
            
            SelectedMenuItem = activeMenuItem;

            
            activeDeviceSubscription = orchestrator.ActiveDeviceSubject.Subscribe(info =>
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    Specs = info.Specs;
                });
            });

            if (orchestrator.ActiveDevice != null)
                Specs = orchestrator.ActiveDevice.Specs;


            /*
            var allActions = new List<ActionViewModel>
            {
                new ActionViewModel(null, null)
            };

            allActions.AddRange(
                pluginManager.GetActionPlugins()
                    .SelectMany(ap => ap.Actions.Select(a => new ActionViewModel(ap, a)))
                    .OrderBy(a => a.Name.ToLower()));
            
            Actions = allActions;



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


            var runKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);
            runAtStartup = runKey?.GetValue("MassiveKnob") != null;
            */
        }


        public void Dispose()
        {
            /*
            DisposeInputOutputViewModels(AnalogInputs);
            DisposeInputOutputViewModels(DigitalInputs);
            DisposeInputOutputViewModels(AnalogOutputs);
            DisposeInputOutputViewModels(DigitalOutputs);
            */
            
            activeDeviceSubscription?.Dispose();
        }

        /*
        private void ApplyLoggingSettings()
        {
            orchestrator?.UpdateSettings(settings =>
            {
                settings.Log.Enabled = LoggingEnabled;
                settings.Log.Level = SelectedLoggingLevel.Level;
            });
            
            loggingSwitch?.SetLogging(LoggingEnabled, selectedLoggingLevel.Level);
        }


        private void ApplyRunAtStartup()
        {
            var runKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            Debug.Assert(runKey != null, nameof(runKey) + " != null");

            if (RunAtStartup)
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                Debug.Assert(entryAssembly != null, nameof(entryAssembly) + " != null");

                runKey.SetValue("MassiveKnob", new Uri(entryAssembly.CodeBase).LocalPath);
            }
            else
            {
                runKey.DeleteValue("MassiveKnob", false);
            }
        }


        private static void DisposeInputOutputViewModels(IEnumerable<InputOutputViewModel> viewModels)
        {
            if (viewModels == null)
                return;
            
            foreach (var viewModel in viewModels)
                viewModel.Dispose();
        }
        */


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnDependantPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class SettingsViewModelDesignTime : SettingsViewModel
    {
        public SettingsViewModelDesignTime() : base(null, null)
        {
            Specs = new DeviceSpecs(2, 2, 2, 2);
        }
    }
}
