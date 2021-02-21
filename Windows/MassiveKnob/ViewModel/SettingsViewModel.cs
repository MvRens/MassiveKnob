using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using MassiveKnob.Model;
using MassiveKnob.Plugin;

namespace MassiveKnob.ViewModel
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly Settings.Settings settings;
        private readonly IMassiveKnobOrchestrator orchestrator;
        private DeviceViewModel selectedDevice;
        private UserControl settingsControl;


        public IEnumerable<DeviceViewModel> Devices { get; }
        public DeviceViewModel SelectedDevice
        {
            get => selectedDevice;

            set
            {
                if (value == selectedDevice)
                    return;

                selectedDevice = value;
                var deviceInstance = orchestrator.SetActiveDevice(value?.Device);
                
                if (value == null)
                    settings.Device = null;
                else
                {
                    settings.Device = new Settings.Settings.DeviceSettings
                    {
                        PluginId = value.Plugin.PluginId,
                        DeviceId = value.Device.DeviceId,
                        Settings = null
                    };
                }

                OnPropertyChanged();

                SettingsControl = deviceInstance?.CreateSettingsControl();
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




        public SettingsViewModel(IPluginManager pluginManager, Settings.Settings settings, IMassiveKnobOrchestrator orchestrator)
        {
            this.settings = settings;
            this.orchestrator = orchestrator;

            Devices = pluginManager.GetDevicePlugins().SelectMany(dp => dp.Devices.Select(d => new DeviceViewModel(dp, d)));

            if (settings.Device != null)
                SelectedDevice = Devices.FirstOrDefault(d =>
                    d.Plugin.PluginId == settings.Device.PluginId &&
                    d.Device.DeviceId == settings.Device.DeviceId);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        

        public class DeviceViewModel
        {
            // ReSharper disable UnusedMember.Global - used by WPF Binding
            public string Name => Device.Name;
            public string Description => Device.Description;
            // ReSharper restore UnusedMember.Global

            public IMassiveKnobDevicePlugin Plugin { get; }
            public IMassiveKnobDevice Device { get; }


            public DeviceViewModel(IMassiveKnobDevicePlugin plugin, IMassiveKnobDevice device)
            {
                Plugin = plugin;
                Device = device;
            }
        }
    }
}
