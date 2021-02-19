using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;

namespace MassiveKnob.Hardware
{
    public class CoreAudioDeviceManager : IAudioDeviceManager
    {
        private readonly Lazy<CoreAudioController> audioController = new Lazy<CoreAudioController>();
        private List<IAudioDevice> devices;


        public void Dispose()
        {
            if (audioController.IsValueCreated)
                audioController.Value.Dispose();
        }

        
        public async Task<IEnumerable<IAudioDevice>> GetDevices()
        {
            return devices ?? (devices = (await audioController.Value.GetPlaybackDevicesAsync())
                .Select(device => new AudioDevice(device) as IAudioDevice)
                .ToList());
        }

        
        public Task<IAudioDevice> GetDeviceById(Guid deviceId)
        {
            return Task.FromResult(devices?.FirstOrDefault(device => device.Id == deviceId));
        }


        private class AudioDevice : IAudioDevice
        {
            private readonly IDevice device;

            public Guid Id { get; }
            public string DisplayName { get; }


            public AudioDevice(IDevice device)
            {
                this.device = device;
                Id = device.Id;
                
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

                DisplayName = string.Format(displayFormat, device.FullName);
            }


            public Task SetVolume(int volume)
            {
                return device.SetVolumeAsync(volume);
            }
        }
    }


    public class CoreAudioDeviceManagerFactory : IAudioDeviceManagerFactory
    {
        public IAudioDeviceManager Create()
        {
            return new CoreAudioDeviceManager();
        }
    }
}
