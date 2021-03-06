using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MassiveKnob.Plugin.EmulatorDevice.Settings
{
    public class EmulatorDeviceSettingsViewModel : INotifyPropertyChanged
    {
        private readonly EmulatorDeviceSettings settings;
        public event PropertyChangedEventHandler PropertyChanged;


        // ReSharper disable UnusedMember.Global - used by WPF Binding
        public int AnalogInputCount
        {
            get => settings.AnalogInputCount;
            set
            {
                if (value == settings.AnalogInputCount)
                    return;

                settings.AnalogInputCount = value;
                OnPropertyChanged();
            }
        }
        

        public int DigitalInputCount
        {
            get => settings.DigitalInputCount;
            set
            {
                if (value == settings.DigitalInputCount)
                    return;

                settings.DigitalInputCount = value;
                OnPropertyChanged();
            }
        }


        public int AnalogOutputCount
        {
            get => settings.AnalogOutputCount;
            set
            {
                if (value == settings.AnalogOutputCount)
                    return;

                settings.AnalogOutputCount = value;
                OnPropertyChanged();
            }
        }


        public int DigitalOutputCount
        {
            get => settings.DigitalOutputCount;
            set
            {
                if (value == settings.DigitalOutputCount)
                    return;

                settings.DigitalOutputCount = value;
                OnPropertyChanged();
            }
        }
        // ReSharper restore UnusedMember.Global


        public EmulatorDeviceSettingsViewModel(EmulatorDeviceSettings settings)
        {
            this.settings = settings;
        }
        
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
