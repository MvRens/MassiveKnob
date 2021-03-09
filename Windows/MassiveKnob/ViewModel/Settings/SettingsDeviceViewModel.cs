using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;
using MassiveKnob.Core;

namespace MassiveKnob.ViewModel.Settings
{
    public class SettingsDeviceViewModel : IDisposable, INotifyPropertyChanged
    {
        private readonly IMassiveKnobOrchestrator orchestrator;
        private DeviceViewModel selectedDevice;
        private UserControl settingsControl;

        private readonly IDisposable deviceStatusSubscription;

        // ReSharper disable UnusedMember.Global - used by WPF Binding
        public IList<DeviceViewModel> Devices { get; }


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


        public SettingsDeviceViewModel(IPluginManager pluginManager, IMassiveKnobOrchestrator orchestrator)
        {
            this.orchestrator = orchestrator;
            
            // For design-time support
            if (orchestrator == null)
                return;

            deviceStatusSubscription = orchestrator.DeviceStatusSubject.Subscribe(status =>
            {
                OnDependantPropertyChanged(nameof(ConnectionStatusColor));
                OnDependantPropertyChanged(nameof(ConnectionStatusText));
            });


            Devices = pluginManager.GetDevicePlugins()
                .SelectMany(dp => dp.Devices.Select(d => new DeviceViewModel(dp, d)))
                .OrderBy(d => d.Name.ToLower())
                .ToList();

            if (orchestrator.ActiveDevice == null) 
                return;

            selectedDevice = Devices.Single(d => d.Device.DeviceId == orchestrator.ActiveDevice.Info.DeviceId);
            SettingsControl = orchestrator.ActiveDevice.Instance.CreateSettingsControl();
        }


        public void Dispose()
        {
            if (SettingsControl is IDisposable disposable)
                disposable.Dispose();

            deviceStatusSubscription?.Dispose();
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


    public class SettingsDeviceViewModelDesignTime : SettingsDeviceViewModel
    {
        public SettingsDeviceViewModelDesignTime() : base(null, null)
        {
        }
    }
}
