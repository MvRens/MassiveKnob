using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MassiveKnob.Hardware
{
    public interface IAudioDevice
    {
        Guid Id { get; }
        string DisplayName { get; }

        Task SetVolume(int volume);
    }
    
    
    public interface IAudioDeviceManager : IDisposable
    {
        Task<IEnumerable<IAudioDevice>> GetDevices();
        Task<IAudioDevice> GetDeviceById(Guid deviceId);
    }


    public interface IAudioDeviceManagerFactory
    {
        IAudioDeviceManager Create();
    }
}
