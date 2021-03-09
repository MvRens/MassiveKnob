using System;
using System.Collections.Generic;
using System.Linq;
using MassiveKnob.Core;

namespace MassiveKnob.ViewModel.Settings
{
    public class SettingsPluginsViewModel
    {
        // ReSharper disable UnusedMember.Global - used by WPF Binding
        public IEnumerable<PluginViewModel> Plugins { get; protected set; }
        // ReSharper restore UnusedMember.Global


        public SettingsPluginsViewModel(IPluginManager pluginManager)
        {
            // Design-time support
            if (pluginManager == null)
                return;

            Plugins = pluginManager.GetPlugins()
                .Select(p => new PluginViewModel(p.Plugin.Name, p.Plugin.Description, p.Filename))
                .OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }
    }   


    public class SettingsPluginsViewModelDesignTime : SettingsPluginsViewModel
    {
        public SettingsPluginsViewModelDesignTime()
            : base(null)
        {
            Plugins = new[]
            {
                new PluginViewModel("Plugin without description", null, "D:\\Does\\Not\\Exist.dll"),
                new PluginViewModel("Design-time plugin", "Fake plugin only visible at design-time.", "C:\\Does\\Not\\Exist.dll")
            };
        }
    }
}
