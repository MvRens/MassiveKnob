using System.Windows;
using MassiveKnob.Plugin;

namespace MassiveKnob.ViewModel
{
    public class DeviceViewModel
    {
        // ReSharper disable UnusedMember.Global - used by WPF Binding
        public string Name => Device.Name;
        public string Description => Device.Description;

        public Visibility DescriptionVisibility => string.IsNullOrEmpty(Description) ? Visibility.Collapsed : Visibility.Visible;
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
