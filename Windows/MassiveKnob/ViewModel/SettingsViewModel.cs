using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MassiveKnob.Core;
using MassiveKnob.Plugin;
using MassiveKnob.Settings;
using MassiveKnob.View.Settings;
using Microsoft.Win32;
using Serilog.Events;

namespace MassiveKnob.ViewModel
{
    // TODO (code quality) split ViewModel for individual views, create viewmodel using container
    // TODO (nice to have) installed plugins list
    public class SettingsViewModel : IDisposable, INotifyPropertyChanged
    {
        private readonly Dictionary<SettingsMenuItem, Type> menuItemControls = new Dictionary<SettingsMenuItem, Type>
        {
            { SettingsMenuItem.Device, typeof(DeviceView) },
            { SettingsMenuItem.AnalogInputs, typeof(AnalogInputsView) },
            { SettingsMenuItem.DigitalInputs, typeof(DigitalInputsView) },
            { SettingsMenuItem.AnalogOutputs, typeof(AnalogOutputsView) },
            { SettingsMenuItem.DigitalOutputs, typeof(DigitalOutputsView) },
            { SettingsMenuItem.Logging, typeof(LoggingView) },
            { SettingsMenuItem.Startup, typeof(StartupView) }
        };



        private readonly IMassiveKnobOrchestrator orchestrator;
        private readonly ILoggingSwitch loggingSwitch;
        private DeviceViewModel selectedDevice;
        private UserControl selectedView;
        private SettingsMenuItem selectedMenuItem;
        private UserControl settingsControl;

        private DeviceSpecs? specs;
        private IEnumerable<InputOutputViewModel> analogInputs;
        private IEnumerable<InputOutputViewModel> digitalInputs;
        private IEnumerable<InputOutputViewModel> analogOutputs;
        private IEnumerable<InputOutputViewModel> digitalOutputs;

        private readonly IDisposable activeDeviceSubscription;
        private readonly IDisposable deviceStatusSubscription;

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
                    SelectedView = (UserControl) Activator.CreateInstance(viewType);

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



        public IList<DeviceViewModel> Devices { get; }
        public IList<ActionViewModel> Actions { get; }


        public DeviceViewModel SelectedDevice
        {
            get => selectedDevice;
            set
            {
                if (value == selectedDevice)
                    return;

                selectedDevice = value;
                var deviceInfo = orchestrator?.SetActiveDevice(value?.Device);

                OnPropertyChanged();

                SettingsControl = deviceInfo?.Instance.CreateSettingsControl();
            }
        }

        public UserControl SettingsControl
        {
            get => settingsControl;
            set
            {
                if (value == settingsControl)
                    return;

                if (settingsControl is IDisposable disposable)
                    disposable.Dispose();

                settingsControl = value;
                OnPropertyChanged();
            }
        }

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
            }
        }


        public Visibility AnalogInputVisibility => specs.HasValue && specs.Value.AnalogInputCount > 0
            ? Visibility.Visible
            : Visibility.Collapsed;

        public IEnumerable<InputOutputViewModel> AnalogInputs
        {
            get => analogInputs;
            set
            {
                analogInputs = value;
                OnPropertyChanged();
            }
        }

        public Visibility DigitalInputVisibility => specs.HasValue && specs.Value.DigitalInputCount > 0
            ? Visibility.Visible
            : Visibility.Collapsed;

        public IEnumerable<InputOutputViewModel> DigitalInputs
        {
            get => digitalInputs;
            set
            {
                digitalInputs = value;
                OnPropertyChanged();
            }
        }

        public Visibility AnalogOutputVisibility => specs.HasValue && specs.Value.AnalogOutputCount > 0
            ? Visibility.Visible
            : Visibility.Collapsed;

        public IEnumerable<InputOutputViewModel> AnalogOutputs
        {
            get => analogOutputs;
            set
            {
                analogOutputs = value;
                OnPropertyChanged();
            }
        }

        public Visibility DigitalOutputVisibility => specs.HasValue && specs.Value.DigitalOutputCount > 0
            ? Visibility.Visible
            : Visibility.Collapsed;

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


        public string ConnectionStatusText
        {
            get
            {
                if (orchestrator == null)
                    return "Design-time";

                switch (orchestrator.DeviceStatus)
                {
                    case MassiveKnobDeviceStatus.Disconnected: 
                        return Strings.DeviceStatusDisconnected;
                    
                    case MassiveKnobDeviceStatus.Connecting: 
                        return Strings.DeviceStatusConnecting;

                    case MassiveKnobDeviceStatus.Connected:
                        return Strings.DeviceStatusConnected;
                    
                    default:
                        return null;
                }
            }
        }

        public Brush ConnectionStatusColor
        {
            get
            {
                if (orchestrator == null)
                    return Brushes.Fuchsia;

                switch (orchestrator.DeviceStatus)
                {
                    case MassiveKnobDeviceStatus.Disconnected:
                        return Brushes.DarkRed;

                    case MassiveKnobDeviceStatus.Connecting:
                        return Brushes.Orange;

                    case MassiveKnobDeviceStatus.Connected:
                        return Brushes.ForestGreen;

                    default:
                        return null;
                }
            }
        }
        // ReSharper restore UnusedMember.Global


        public SettingsViewModel(IPluginManager pluginManager, IMassiveKnobOrchestrator orchestrator, ILoggingSwitch loggingSwitch)
        {
            this.orchestrator = orchestrator;
            this.loggingSwitch = loggingSwitch;
            
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
            deviceStatusSubscription = orchestrator.DeviceStatusSubject.Subscribe(status =>
            {
                OnDependantPropertyChanged(nameof(ConnectionStatusColor));
                OnDependantPropertyChanged(nameof(ConnectionStatusText));
            });


            Devices = pluginManager.GetDevicePlugins()
                .SelectMany(dp => dp.Devices.Select(d => new DeviceViewModel(dp, d)))
                .OrderBy(d => d.Name.ToLower())
                .ToList();

            var allActions = new List<ActionViewModel>
            {
                new ActionViewModel(null, null)
            };

            allActions.AddRange(
                pluginManager.GetActionPlugins()
                    .SelectMany(ap => ap.Actions.Select(a => new ActionViewModel(ap, a)))
                    .OrderBy(a => a.Name.ToLower()));
            
            Actions = allActions;

            if (orchestrator.ActiveDevice != null)
            {
                selectedDevice = Devices.Single(d => d.Device.DeviceId == orchestrator.ActiveDevice.Info.DeviceId);
                SettingsControl = orchestrator.ActiveDevice.Instance.CreateSettingsControl();
                Specs = orchestrator.ActiveDevice.Specs;
            }


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
        }


        public void Dispose()
        {
            if (SettingsControl is IDisposable disposable)
                disposable.Dispose();

            DisposeInputOutputViewModels(AnalogInputs);
            DisposeInputOutputViewModels(DigitalInputs);
            DisposeInputOutputViewModels(AnalogOutputs);
            DisposeInputOutputViewModels(DigitalOutputs);
            
            activeDeviceSubscription?.Dispose();
            deviceStatusSubscription?.Dispose();
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
        public SettingsViewModelDesignTime() : base(null, null, null)
        {
            Specs = new DeviceSpecs(2, 2, 2, 2);
        }
    }
}
