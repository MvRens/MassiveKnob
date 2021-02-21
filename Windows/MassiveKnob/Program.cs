using System;
using System.Threading.Tasks;
using MassiveKnob.Model;
using MassiveKnob.Settings;
using MassiveKnob.View;
using MassiveKnob.ViewModel;
using SimpleInjector;

namespace MassiveKnob
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
           MainAsync().GetAwaiter().GetResult();
        }


        private static async Task MainAsync()
        {
            var container = new Container();
            container.Options.EnableAutoVerification = false;

            container.RegisterSingleton<IMassiveKnobOrchestrator, MassiveKnobOrchestrator>();

            container.Register<App>();
            container.Register<SettingsWindow>();
            container.Register<SettingsViewModel>();

            var settings = await SettingsJsonSerializer.Deserialize();
            container.RegisterInstance(settings);

            var pluginManager = new PluginManager();
            pluginManager.Load();
            container.RegisterInstance<IPluginManager>(pluginManager);

            
            var app = container.GetInstance<App>();
            app.Run();
        }
    }
}
