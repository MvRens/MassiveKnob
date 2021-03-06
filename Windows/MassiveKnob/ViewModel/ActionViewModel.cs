using System.Windows;
using MassiveKnob.Plugin;

namespace MassiveKnob.ViewModel
{
    public class ActionViewModel
    {
        // ReSharper disable UnusedMember.Global - used by WPF Binding
        public string Name => RepresentsNull ? Strings.ActionNotConfigured : Action.Name;
        public string Description => RepresentsNull ? null : Action.Description;

        public Visibility DescriptionVisibility => string.IsNullOrEmpty(Description) ? Visibility.Collapsed : Visibility.Visible;
        // ReSharper restore UnusedMember.Global

        public IMassiveKnobActionPlugin Plugin { get; }
        public IMassiveKnobAction Action { get; }

        public bool RepresentsNull => Action == null;



        public ActionViewModel(IMassiveKnobActionPlugin plugin, IMassiveKnobAction action)
        {
            Plugin = plugin;
            Action = action;
        }
    }

}
