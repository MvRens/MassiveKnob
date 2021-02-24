using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MassiveKnob.Plugin.MockDevice.Settings
{
    public class MockDeviceSettingsViewModel : INotifyPropertyChanged
    {
        private readonly MockDeviceSettings settings;
        public event PropertyChangedEventHandler PropertyChanged;


        // ReSharper disable UnusedMember.Global - used by WPF Binding
        public int AnalogCount
        {
            get => settings.AnalogCount;
            set
            {
                if (value == settings.AnalogCount)
                    return;

                settings.AnalogCount = value;
                OnPropertyChanged();
            }
        }
        

        public int DigitalCount
        {
            get => settings.DigitalCount;
            set
            {
                if (value == settings.DigitalCount)
                    return;

                settings.DigitalCount = value;
                OnPropertyChanged();
            }
        }
        
        
        public int Interval
        {
            get => settings.Interval;
            set
            {
                if (value == settings.Interval)
                    return;

                settings.Interval = value;
                OnPropertyChanged();
            }
        }
        // ReSharper restore UnusedMember.Global


        public MockDeviceSettingsViewModel(MockDeviceSettings settings)
        {
            this.settings = settings;
        }
        
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
