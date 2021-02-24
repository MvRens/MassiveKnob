using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using AudioSwitcher.AudioApi;

namespace MassiveKnob.Plugin.CoreAudio.Base
{
    public class BaseDeviceSettingsViewModel<T> : BaseDeviceSettingsViewModel where T : BaseDeviceSettings
    {
        protected new T Settings => (T)base.Settings;

        public BaseDeviceSettingsViewModel(T settings) : base(settings)
        {
        }
    }



    public class BaseDeviceSettingsViewModel : INotifyPropertyChanged
    {
        protected readonly BaseDeviceSettings Settings;
        public event PropertyChangedEventHandler PropertyChanged;

        private IList<PlaybackDeviceViewModel> playbackDevices;
        private PlaybackDeviceViewModel selectedDevice;

        // ReSharper disable UnusedMember.Global - used by WPF Binding
        public IList<PlaybackDeviceViewModel> PlaybackDevices
        {
            get => playbackDevices;
            set
            {
                playbackDevices = value;
                OnPropertyChanged();
            }
        }

        public PlaybackDeviceViewModel SelectedDevice
        {
            get => selectedDevice;
            set
            {
                if (value == selectedDevice)
                    return;

                selectedDevice = value;
                Settings.DeviceId = selectedDevice?.Id;
                OnPropertyChanged();
            }
        }


        public bool OSD
        {
            get => Settings.OSD;
            set
            {
                if (value == Settings.OSD)
                    return;

                Settings.OSD = value;
                OnPropertyChanged();
            }
        }
        // ReSharper restore UnusedMember.Global


        public BaseDeviceSettingsViewModel(BaseDeviceSettings settings)
        {
            Settings = settings;

            Task.Run(async () =>
            {
                var coreAudioController = CoreAudioControllerInstance.Acquire();
                var devices = await coreAudioController.GetPlaybackDevicesAsync();
                var deviceViewModels = devices
                    .OrderBy(d => d.FullName)
                    .Select(PlaybackDeviceViewModel.FromDevice)
                    .ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    PlaybackDevices = deviceViewModels;
                    SelectedDevice = deviceViewModels.SingleOrDefault(d => d.Id == settings.DeviceId);
                });
            });
        }


        public virtual bool IsSettingsProperty(string propertyName)
        {
            return propertyName != nameof(PlaybackDevices);
        }


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class PlaybackDeviceViewModel
    {
        public Guid Id { get; private set; }
        public string DisplayName { get; private set; }


        public static PlaybackDeviceViewModel FromDevice(IDevice device)
        {
            string displayFormat;

            if ((device.State & DeviceState.Disabled) != 0)
                displayFormat = Strings.DeviceDisplayNameDisabled;
            else if ((device.State & DeviceState.Unplugged) != 0)
                displayFormat = Strings.DeviceDisplayNameUnplugged;
            else if ((device.State & DeviceState.NotPresent) != 0)
                displayFormat = Strings.DeviceDisplayNameNotPresent;
            else if ((device.State & DeviceState.Active) == 0)
                displayFormat = Strings.DeviceDisplayNameInactive;
            else
                displayFormat = Strings.DeviceDisplayNameActive;

            return new PlaybackDeviceViewModel
            {
                Id = device.Id,
                DisplayName = string.Format(displayFormat, device.FullName)
            };
        }
    }
}
