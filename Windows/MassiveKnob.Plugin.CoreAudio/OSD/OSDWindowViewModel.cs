using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using AudioSwitcher.AudioApi;

namespace MassiveKnob.Plugin.CoreAudio.OSD
{
    public class OSDWindowViewModel : INotifyPropertyChanged
    {
        // ReSharper disable UnusedMember.Global - used by WPF Binding
        private string deviceName;
        public string DeviceName
        {
            get => deviceName;
            set
            {
                if (value == deviceName)
                    return;

                deviceName = value;
                OnPropertyChanged();
            }
        }

        private int volume;
        public int Volume
        {
            get => volume;
            set
            {
                if (value == volume)
                    return;

                volume = value;
                OnPropertyChanged();
                OnDependantPropertyChanged(nameof(VolumeLowVisibility));
                OnDependantPropertyChanged(nameof(VolumeMediumVisibility));
                OnDependantPropertyChanged(nameof(VolumeHighVisibility));
                OnDependantPropertyChanged(nameof(VolumeIndicatorLeft));
            }
        }


        private bool isMuted;
        public bool IsMuted
        {
            get => isMuted;
            set
            {
                if (value == isMuted)
                    return;

                isMuted = value;
                OnPropertyChanged();
                OnDependantPropertyChanged(nameof(IsMutedVisibility));
                OnDependantPropertyChanged(nameof(IsNotMutedVisibility));
            }
        }


        public Visibility IsMutedVisibility => IsMuted ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsNotMutedVisibility => IsMuted ? Visibility.Collapsed : Visibility.Visible;
        public Visibility VolumeLowVisibility => Volume > 0 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility VolumeMediumVisibility => Volume > 33 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility VolumeHighVisibility => Volume > 66 ? Visibility.Visible : Visibility.Collapsed;

        public int VolumeIndicatorLeft => Volume * 3;
        // ReSharper enable UnusedMember.Global


        public void SetDevice(IDevice device)
        {
            DeviceName = device.FullName;
            Volume = (int)device.Volume;
            IsMuted = device.IsMuted;
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
}
