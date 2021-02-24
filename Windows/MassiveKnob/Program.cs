using System;
using System.Text;
using System.Windows;
using MassiveKnob.Model;
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
        public static int Main()
        {
            var pluginManager = new PluginManager();

            var messages = new StringBuilder();
            pluginManager.Load((exception, filename) =>
            {
                messages.AppendLine($"{filename}: {exception.Message}");
            });

            if (messages.Length > 0)
            {
                MessageBox.Show($"Error while loading plugins:\r\n\r\n{messages}", "Massive Knob", MessageBoxButton.OK, MessageBoxImage.Error);
                return 1;
            }
            
            var orchestrator = new MassiveKnobOrchestrator(pluginManager);
            orchestrator.Load();


            var container = new Container();
            container.Options.EnableAutoVerification = false;

            container.RegisterInstance<IPluginManager>(pluginManager);
            container.RegisterInstance<IMassiveKnobOrchestrator>(orchestrator);

            container.Register<App>();
            container.Register<SettingsWindow>();
            container.Register<SettingsViewModel>();

            
            var app = container.GetInstance<App>();
            app.Run();

            orchestrator.Dispose();
            return 0;
        }
    }
}
