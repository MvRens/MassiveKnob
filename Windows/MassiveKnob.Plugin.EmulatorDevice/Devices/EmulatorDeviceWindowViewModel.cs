using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MassiveKnob.Plugin.EmulatorDevice.Settings;

namespace MassiveKnob.Plugin.EmulatorDevice.Devices
{
    public class EmulatorDeviceWindowViewModel : INotifyPropertyChanged
    {
        private readonly EmulatorDeviceSettings settings;
        private readonly IMassiveKnobDeviceContext context;
        public event PropertyChangedEventHandler PropertyChanged;


        // ReSharper disable UnusedMember.Global - used by WPF Binding
        private int analogInputCount;
        public int AnalogInputCount
        {
            get => analogInputCount;
            set
            {
                if (value == analogInputCount)
                    return;

                analogInputCount = value;
                OnPropertyChanged();

                AnalogInputs = Enumerable.Range(0, AnalogInputCount)
                    .Select(i => new InputOutputViewModel(context, MassiveKnobActionType.InputAnalog, i))
                    .ToList();
            }
        }

        private IList<InputOutputViewModel> analogInputs;
        public IList<InputOutputViewModel> AnalogInputs
        {
            get => analogInputs;
            set
            {
                analogInputs = value;
                OnPropertyChanged();
            }
        }


        private int digitalInputCount;
        public int DigitalInputCount
        {
            get => digitalInputCount;
            set
            {
                if (value == digitalInputCount)
                    return;

                digitalInputCount = value;
                OnPropertyChanged();

                DigitalInputs = Enumerable.Range(0, DigitalInputCount)
                    .Select(i => new InputOutputViewModel(context, MassiveKnobActionType.InputDigital, i))
                    .ToList();
            }
        }

        private IList<InputOutputViewModel> digitalInputs;
        public IList<InputOutputViewModel> DigitalInputs
        {
            get => digitalInputs;
            set
            {
                digitalInputs = value;
                OnPropertyChanged();
            }
        }


        private int analogOutputCount;
        public int AnalogOutputCount
        {
            get => analogOutputCount;
            set
            {
                if (value == analogOutputCount)
                    return;

                analogOutputCount = value;
                OnPropertyChanged();

                AnalogOutputs = Enumerable.Range(0, AnalogOutputCount)
                    .Select(i => new InputOutputViewModel(context, MassiveKnobActionType.OutputAnalog, i))
                    .ToList();
            }
        }


        private IList<InputOutputViewModel> analogOutputs;
        public IList<InputOutputViewModel> AnalogOutputs
        {
            get => analogOutputs;
            set
            {
                analogOutputs = value;
                OnPropertyChanged();
            }
        }


        private int digitalOutputCount;
        public int DigitalOutputCount
        {
            get => digitalOutputCount;
            set
            {
                if (value == digitalOutputCount)
                    return;

                digitalOutputCount = value;
                OnPropertyChanged();

                DigitalOutputs = Enumerable.Range(0, DigitalOutputCount)
                    .Select(i => new InputOutputViewModel(context, MassiveKnobActionType.OutputDigital, i))
                    .ToList();
            }
        }

        private IList<InputOutputViewModel> digitalOutputs;
        public IList<InputOutputViewModel> DigitalOutputs
        {
            get => digitalOutputs;
            set
            {
                digitalOutputs = value;
                OnPropertyChanged();
            }
        }
        // ReSharper restore UnusedMember.Global


        
        public EmulatorDeviceWindowViewModel(EmulatorDeviceSettings settings, IMassiveKnobDeviceContext context)
        {
            this.settings = settings;
            this.context = context;
            
            ApplySettings();
        }


        public void ApplySettings()
        {
            AnalogInputCount = settings.AnalogInputCount;
            DigitalInputCount = settings.DigitalInputCount;
            AnalogOutputCount = settings.AnalogOutputCount;
            DigitalOutputCount = settings.DigitalOutputCount;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        

        public class InputOutputViewModel : INotifyPropertyChanged
        {
            private readonly IMassiveKnobDeviceContext context;
            public MassiveKnobActionType ActionType { get; }
            public int Index { get; }

            public string DisplayName
            {
                get
                {
                    switch (ActionType)
                    {
                        case MassiveKnobActionType.InputAnalog:
                            return $"Analog input #{Index + 1}";
                        
                        case MassiveKnobActionType.InputDigital:
                            return $"Digital input #{Index + 1}";

                        case MassiveKnobActionType.OutputAnalog:
                            return $"Analog output #{Index + 1}";

                        case MassiveKnobActionType.OutputDigital:
                            return $"Digital output #{Index + 1}";

                        default:
                            return (Index + 1).ToString();
                    }
                }
            }


            private byte analogValue;
            public byte AnalogValue
            {
                get => analogValue;
                set
                {
                    analogValue = value;
                    OnPropertyChanged();

                    if (ActionType == MassiveKnobActionType.InputAnalog)
                        // Context can be null in DesignTime
                        context?.AnalogChanged(Index, analogValue);
                }
            }

            private bool digitalValue;
            public bool DigitalValue
            {
                get => digitalValue;
                set
                {
                    digitalValue = value;
                    OnPropertyChanged();
                    OnDependantPropertyChanged("DigitalValueDisplayText");

                    if (ActionType == MassiveKnobActionType.InputDigital)
                        context?.DigitalChanged(Index, digitalValue);
                }
            }

            public string DigitalValueDisplayText => DigitalValue ? "On" : "Off";


            public InputOutputViewModel(IMassiveKnobDeviceContext context, MassiveKnobActionType actionType, int index)
            {
                this.context = context;
                ActionType = actionType;
                Index = index;
            }

            
            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            protected void OnDependantPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    
    public class EmulatorDeviceWindowViewModelDesignTime : EmulatorDeviceWindowViewModel
    {
        public EmulatorDeviceWindowViewModelDesignTime() : base(
            new EmulatorDeviceSettings
            {
                AnalogInputCount = 2,
                DigitalInputCount = 2,
                AnalogOutputCount = 2,
                DigitalOutputCount = 2
            }, null)
        {
        }
    }
}
