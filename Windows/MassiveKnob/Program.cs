using System;
using System.IO;
using System.Text;
using System.Windows;
using MassiveKnob.Core;
using MassiveKnob.Settings;
using MassiveKnob.View;
using MassiveKnob.ViewModel;
using Serilog;
using SimpleInjector;

namespace MassiveKnob
{
    public static class Program
    {
        [STAThread]
        public static int Main()
        {
            var settings = MassiveKnobSettingsJsonSerializer.Deserialize();

            var loggingSwitch = new LoggingSwitch();
            loggingSwitch.SetLogging(settings.Log.Enabled, settings.Log.Level);

            var logger = new LoggerConfiguration()
                .Filter.ByIncludingOnly(loggingSwitch.IsIncluded)
                .Enrich.FromLogContext()
                .WriteTo.File(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"MassiveKnob", @"Logs", @".log"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{@Context}{NewLine}{Exception}")
                .CreateLogger();
            
            
            logger.Information("MassiveKnob starting");
            var pluginManager = new PluginManager(logger);

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
            
            var orchestrator = new MassiveKnobOrchestrator(pluginManager, logger, settings);
            orchestrator.Load();


            var container = new Container();
            container.Options.EnableAutoVerification = false;

            container.RegisterInstance(logger);
            container.RegisterInstance<ILoggingSwitch>(loggingSwitch);
            container.RegisterInstance<IPluginManager>(pluginManager);
            container.RegisterInstance<IMassiveKnobOrchestrator>(orchestrator);

            container.Register<App>();
            container.Register<SettingsWindow>();
            container.Register<SettingsViewModel>();

            
            var app = container.GetInstance<App>();
            app.Run();

            logger.Information("MassiveKnob shutting down");
            orchestrator.Dispose();
            return 0;
        }
    }
}
