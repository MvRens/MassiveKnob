using System.Windows;
using System.Windows.Controls;

namespace MassiveKnob.ViewModel
{
    public static class MenuItemProperties
    {
        public static string GetText(DependencyObject obj) { return (string) obj.GetValue(TextProperty); }
        public static void SetText(DependencyObject obj, string value) { obj.SetValue(TextProperty, value); }

        public static readonly DependencyProperty TextProperty = 
            DependencyProperty.RegisterAttached("Text", typeof(string), typeof(MenuItemProperties), new FrameworkPropertyMetadata("Menu item"));


        public static Viewbox GetIcon(DependencyObject obj) { return (Viewbox)obj.GetValue(IconProperty); }
        public static void SetIcon(DependencyObject obj, Viewbox value) { obj.SetValue(IconProperty, value); }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.RegisterAttached("Icon", typeof(Viewbox), typeof(MenuItemProperties), new FrameworkPropertyMetadata(null));
    }
}
