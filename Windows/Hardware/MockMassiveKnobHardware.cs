using System;
using System.Threading;
using System.Threading.Tasks;

namespace MassiveKnob.Hardware
{
    public class MockMassiveKnobHardware : AbstractMassiveKnobHardware
    {
        private readonly int knobCount;
        private readonly TimeSpan volumeChangeInterval;
        private readonly int maxVolume;
        private Timer changeVolumeTimer;
        private readonly Random random = new Random();

        
        public MockMassiveKnobHardware(int knobCount, TimeSpan volumeChangeInterval, int maxVolume)
        {
            this.knobCount = knobCount;
            this.volumeChangeInterval = volumeChangeInterval;
            this.maxVolume = maxVolume;
        }
        
        
        public override async Task TryConnect()
        {
            if (changeVolumeTimer != null)
                return;
            
            await Task.Delay(2000);
            
            Observers.Connected(knobCount);
            changeVolumeTimer = new Timer(
                state =>
                {
                    Observers.VolumeChanged(random.Next(0, knobCount), random.Next(0, maxVolume));
                },
                null,
                volumeChangeInterval,
                volumeChangeInterval);
        }
        

        public override Task Disconnect()
        {
            changeVolumeTimer?.Dispose();
            return Task.CompletedTask;
        }
    }
    
    
    public class MockMassiveKnobHardwareFactory : IMassiveKnobHardwareFactory
    {
        private readonly int knobCount;
        private readonly TimeSpan volumeChangeInterval;
        private readonly int maxVolume;

        public MockMassiveKnobHardwareFactory(int knobCount, TimeSpan volumeChangeInterval, int maxVolume)
        {
            this.knobCount = knobCount;
            this.volumeChangeInterval = volumeChangeInterval;
            this.maxVolume = maxVolume;
        }
        
        
        public IMassiveKnobHardware Create(string serialPort)
        {
            return new MockMassiveKnobHardware(knobCount, volumeChangeInterval, maxVolume);
        }
    }
}
