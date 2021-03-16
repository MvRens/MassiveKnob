using System;
using System.IO;
using System.Text;
using System.Windows;
using MassiveKnob.Core;
using MassiveKnob.Settings;
using Serilog;

namespace MassiveKnob
{
    // TODO (should have) global exception handler - AppDomain.CurrentDomain.UnhandledException
    public static class Program
    {
        [STAThread]
        public static int Main()
        {
            var settings = MassiveKnobSettingsJsonSerializer.Deserialize();

            
            var loggingSwitch = new LoggingSwitch();
            loggingSwitch.SetLogging(settings.Log.Enabled, settings.Log.Level);

            var logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"MassiveKnob", @"Logs");

            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Filter.ByIncludingOnly(loggingSwitch.IsIncluded)
                .Enrich.FromLogContext()
                .WriteTo.File(
                    Path.Combine(logFilePath, @".log"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{@Context}{NewLine}{Exception}")
                .CreateLogger();
            
            
            logger.Information("MassiveKnob starting");


            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var e = (Exception)args.ExceptionObject;
                logger.Error(e, "Unhandled exception: {message}", e.Message);

                MessageBox.Show(
                    "Oops, something went very wrong. Please notify the developer and include this message, you can copy it using Ctrl-C. " +
                    "Preferably also include the log file which can be found at:" + Environment.NewLine + logFilePath +
                    Environment.NewLine + Environment.NewLine +
                    e.Message, "Massive Knob - Fatal error", MessageBoxButton.OK, MessageBoxImage.Error);
            };


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

            
            var container = ContainerBuilder.Create();
            container.RegisterInstance<ILogger>(logger);
            container.RegisterInstance<ILoggingSwitch>(loggingSwitch);
            container.RegisterInstance<IPluginManager>(pluginManager);
            container.RegisterInstance<IMassiveKnobOrchestrator>(orchestrator);

            var app = container.GetInstance<App>();
            app.Run();

            logger.Information("MassiveKnob shutting down");
            orchestrator.Dispose();
            return 0;
        }
    }
}
