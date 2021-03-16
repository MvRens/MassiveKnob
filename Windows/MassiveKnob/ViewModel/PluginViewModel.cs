using System.Windows;

namespace MassiveKnob.ViewModel
{
    public class PluginViewModel
    {
        // ReSharper disable UnusedMember.Global - used by WPF Binding
        public string Name { get; }
        public string Description { get; }
        public string Filename { get; }
        public string Author { get; }
        public string Url { get; }

        public Visibility DescriptionVisibility => string.IsNullOrEmpty(Description) ? Visibility.Collapsed : Visibility.Visible;
        public Visibility AuthorVisibility => string.IsNullOrEmpty(Author) ? Visibility.Collapsed : Visibility.Visible;
        public Visibility UrlVisibility => string.IsNullOrEmpty(Url) ? Visibility.Collapsed : Visibility.Visible;
        // ReSharper restore UnusedMember.Global


        public PluginViewModel(string name, string description, string filename, string author, string url)
        {
            Name = name;
            Description = description;
            Filename = filename;
            Author = author;
            Url = url;
        }
    }
}
