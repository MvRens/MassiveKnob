using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using MassiveKnob.Model;
using MassiveKnob.Plugin;

namespace MassiveKnob.ViewModel
{
    // TODO (nice to have) better design-time version
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly IMassiveKnobOrchestrator orchestrator;
        private DeviceViewModel selectedDevice;
        private UserControl settingsControl;

        private DeviceSpecs? specs;
        private IEnumerable<InputOutputViewModel> analogInputs;
        private IEnumerable<InputOutputViewModel> digitalInputs;
        private IEnumerable<InputOutputViewModel> analogOutputs;
        private IEnumerable<InputOutputViewModel> digitalOutputs;


        // ReSharper disable UnusedMember.Global - used by WPF Binding
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
                var deviceInfo = orchestrator.SetActiveDevice(value?.Device);

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

                AnalogInputs = Enumerable
                    .Range(0, specs?.AnalogInputCount ?? 0)
                    .Select(i => new InputOutputViewModel(this, orchestrator, MassiveKnobActionType.InputAnalog, i));

                DigitalInputs = Enumerable
                    .Range(0, specs?.DigitalInputCount ?? 0)
                    .Select(i => new InputOutputViewModel(this, orchestrator, MassiveKnobActionType.InputDigital, i));

                AnalogOutputs = Enumerable
                    .Range(0, specs?.AnalogOutputCount ?? 0)
                    .Select(i => new InputOutputViewModel(this, orchestrator, MassiveKnobActionType.OutputAnalog, i));

                DigitalOutputs = Enumerable
                    .Range(0, specs?.DigitalOutputCount ?? 0)
                    .Select(i => new InputOutputViewModel(this, orchestrator, MassiveKnobActionType.OutputDigital, i));
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
        // ReSharper restore UnusedMember.Global



        public SettingsViewModel(IPluginManager pluginManager, IMassiveKnobOrchestrator orchestrator)
        {
            this.orchestrator = orchestrator;

            orchestrator.ActiveDeviceSubject.Subscribe(info => { Specs = info.Specs; });


            Devices = pluginManager.GetDevicePlugins()
                .SelectMany(dp => dp.Devices.Select(d => new DeviceViewModel(dp, d)))
                .ToList();

            var allActions = new List<ActionViewModel>
            {
                new ActionViewModel(null, null)
            };

            allActions.AddRange(
                pluginManager.GetActionPlugins()
                    .SelectMany(ap => ap.Actions.Select(a => new ActionViewModel(ap, a))));
            
            Actions = allActions;

            if (orchestrator.ActiveDevice == null)
                return;

            selectedDevice = Devices.Single(d => d.Device.DeviceId == orchestrator.ActiveDevice.Info.DeviceId);
            SettingsControl = orchestrator.ActiveDevice.Instance.CreateSettingsControl();
            Specs = orchestrator.ActiveDevice.Specs;
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
}
