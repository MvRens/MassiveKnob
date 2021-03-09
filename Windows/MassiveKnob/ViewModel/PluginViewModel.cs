using System.Windows;

namespace MassiveKnob.ViewModel
{
    public class PluginViewModel
    {
        // ReSharper disable UnusedMember.Global - used by WPF Binding
        public string Name { get; }
        public string Description { get; }
        public string Filename { get; }

        public Visibility DescriptionVisibility => string.IsNullOrEmpty(Description) ? Visibility.Collapsed : Visibility.Visible;
        // ReSharper restore UnusedMember.Global


        public PluginViewModel(string name, string description, string filename)
        {
            Name = name;
            Description = description;
            Filename = filename;
        }
    }
}
