using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using MassiveKnob.Core;
using MassiveKnob.Plugin;

namespace MassiveKnob.ViewModel.Settings
{
    public class BaseSettingsInputOutputViewModel : IDisposable, INotifyPropertyChanged
    {
        private readonly IMassiveKnobOrchestrator orchestrator;

        private readonly MassiveKnobActionType inputOutputType;
        private DeviceSpecs? specs;
        private readonly IDisposable activeDeviceSubscription;
        private IEnumerable<InputOutputViewModel> inputOutputs;


        // ReSharper disable UnusedMember.Global - used by WPF Binding
        public IList<ActionViewModel> Actions { get; }

        public DeviceSpecs? Specs
        {
            get => specs;
            set
            {
                specs = value;
                OnPropertyChanged();

                DisposeInputOutputViewModels();
                int inputOutputCount;
                
                switch (inputOutputType)
                {
                    case MassiveKnobActionType.InputAnalog:
                        inputOutputCount = specs?.AnalogInputCount ?? 0;
                        break;
                    
                    case MassiveKnobActionType.InputDigital:
                        inputOutputCount = specs?.DigitalInputCount ?? 0;
                        break;
                    
                    case MassiveKnobActionType.OutputAnalog:
                        inputOutputCount = specs?.AnalogOutputCount ?? 0;
                        break;

                    case MassiveKnobActionType.OutputDigital:
                        inputOutputCount = specs?.DigitalOutputCount ?? 0;
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                InputOutputs = Enumerable
                    .Range(0, inputOutputCount)
                    .Select(i => new InputOutputViewModel(Actions, orchestrator, inputOutputType, i))
                    .ToList();
            }
        }


        public IEnumerable<InputOutputViewModel> InputOutputs
        {
            get => inputOutputs;
            set
            {
                inputOutputs = value;
                OnPropertyChanged();
            }
        }
        // ReSharper restore UnusedMember.Global


        public BaseSettingsInputOutputViewModel(IPluginManager pluginManager, IMassiveKnobOrchestrator orchestrator, MassiveKnobActionType inputOutputType)
        {
            this.orchestrator = orchestrator;
            this.inputOutputType = inputOutputType;

            // For design-time support
            if (orchestrator == null)
                return;
            
            
            var allActions = new List<ActionViewModel>
            {
                new ActionViewModel(null, null)
            };

            allActions.AddRange(
                pluginManager.GetActionPlugins()
                    .SelectMany(ap => ap.Actions.Select(a => new ActionViewModel(ap, a)))
                    .OrderBy(a => a.Name.ToLower()));
            
            Actions = allActions;


            activeDeviceSubscription = orchestrator.ActiveDeviceSubject.Subscribe(info =>
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    Specs = info.Specs;
                });
            });

            if (orchestrator.ActiveDevice != null)
                Specs = orchestrator.ActiveDevice.Specs;
        }


        public void Dispose()
        {
            DisposeInputOutputViewModels();
            activeDeviceSubscription?.Dispose();
        }


        private void DisposeInputOutputViewModels()
        {
            if (inputOutputs == null)
                return;
            
            foreach (var viewModel in inputOutputs)
                viewModel.Dispose();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    
    public class BaseSettingsInputOutputViewModelDesignTime : BaseSettingsInputOutputViewModel
    {
        public BaseSettingsInputOutputViewModelDesignTime()
            : base(null, null, MassiveKnobActionType.InputAnalog)
        {
            Specs = new DeviceSpecs(2, 2, 2, 2);
        }
    }    
}
