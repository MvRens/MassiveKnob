using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using Dapplo.Windows.Devices;
using Dapplo.Windows.Devices.Enums;

namespace MassiveKnob.Plugin.SerialDevice.Settings
{
    public class SerialDeviceSettingsViewModel : IDisposable, INotifyPropertyChanged, IObserver<DeviceNotificationEvent>
    {
        private readonly SerialDeviceSettings settings;
        private IList<string> serialPorts;
        private readonly IDisposable deviceSubscription;
        public event PropertyChangedEventHandler PropertyChanged;


        // ReSharper disable UnusedMember.Global - used by WPF Binding
        public IList<string> SerialPorts
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
                if (value == settings.PortName || value == null)
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


        public bool DtrEnable
        {
            get => settings.DtrEnable;
            set
            {
                if (value == settings.DtrEnable)
                    return;

                settings.DtrEnable = value;
                OnPropertyChanged();
            }
        }
        // ReSharper restore UnusedMember.Global


        public SerialDeviceSettingsViewModel(SerialDeviceSettings settings)
        {
            this.settings = settings;

            serialPorts = SerialPort.GetPortNames();
            deviceSubscription = DeviceNotification.OnNotification.Subscribe(this);
        }


        public void Dispose()
        {
            deviceSubscription.Dispose();
        }


        public bool IsSettingsProperty(string propertyName)
        {
            return propertyName != nameof(SerialPorts);
        }
        
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        
        public void OnNext(DeviceNotificationEvent value)
        {
            if ((value.EventType == DeviceChangeEvent.DeviceArrival ||
                 value.EventType == DeviceChangeEvent.DeviceRemoveComplete) &&
                value.Is(DeviceBroadcastDeviceType.DeviceInterface))
            {
                SerialPorts = SerialPort.GetPortNames();
            }
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }
    }
}
