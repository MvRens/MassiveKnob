using System;
using System.Windows.Forms;
using MassiveKnob.Forms;
using MassiveKnob.Hardware;
using SimpleInjector;
using SimpleInjector.Diagnostics;

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
            var container = BuildContainer();
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(container.GetInstance<SettingsForm>());
        }


        private static Container BuildContainer()
        {
            var container = new Container();
            container.Options.EnableAutoVerification = false;
            
            container.Register<SettingsForm>();
            container.GetRegistration(typeof(SettingsForm))?.Registration
                .SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, "Windows Form implements IDisposable");

            container.Register<IAudioDeviceManagerFactory, CoreAudioDeviceManagerFactory>();
            
            // For testing without the hardware:
            container.Register<IMassiveKnobHardwareFactory>(() => new MockMassiveKnobHardwareFactory(3, TimeSpan.FromSeconds(1), 25));
            
            return container;
        }
    }
}
