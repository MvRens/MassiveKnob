using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Runtime.CompilerServices;

namespace MassiveKnob.Plugin.SerialDevice.Settings
{
    public class SerialDeviceSettingsViewModel : INotifyPropertyChanged
    {
        private readonly SerialDeviceSettings settings;
        private IEnumerable<string> serialPorts;
        public event PropertyChangedEventHandler PropertyChanged;


        // ReSharper disable UnusedMember.Global - used by WPF Binding
        public IEnumerable<string> SerialPorts
        {
            get => serialPorts;
            set
            {
                serialPorts = value;
                OnPropertyChanged();
            }
        }


        public string PortName
        {
            get => settings.PortName;
            set
            {
                if (value == settings.PortName)
                    return;

                settings.PortName = value;
                OnPropertyChanged();
            }
        }
        

        public int BaudRate
        {
            get => settings.BaudRate;
            set
            {
                if (value == settings.BaudRate)
                    return;

                settings.BaudRate = value;
                OnPropertyChanged();
            }
        }
        // ReSharper restore UnusedMember.Global


        public SerialDeviceSettingsViewModel(SerialDeviceSettings settings)
        {
            this.settings = settings;

            serialPorts = SerialPort.GetPortNames();
            
            // TODO subscribe to device notification to refresh list
        }


        public bool IsSettingsProperty(string propertyName)
        {
            return propertyName != nameof(SerialPorts);
        }
        
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
