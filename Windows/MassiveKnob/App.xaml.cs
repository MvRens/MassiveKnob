using System.Diagnostics;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using MassiveKnob.View;
using SimpleInjector;

namespace MassiveKnob
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private readonly Container container;
        private TaskbarIcon notifyIcon;

        private SettingsWindow settingsWindow;

        
        public App(Container container)
        {
            this.container = container;
            InitializeComponent();
        }
        
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
            Debug.Assert(notifyIcon != null, nameof(notifyIcon) + " != null");
        }


        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon?.Dispose();
            
            base.OnExit(e);
        }



        private void ShowSettings()
        {
            if (settingsWindow == null)
            {
                settingsWindow = container.GetInstance<SettingsWindow>();
                settingsWindow.Closed += (sender, args) => { settingsWindow = null; };
                settingsWindow.Show();
            }
            else
            {
                settingsWindow.WindowState = WindowState.Normal;
                settingsWindow.Activate();
            }
        }
        

        private void NotifyIconMenuSettingsClick(object sender, RoutedEventArgs e)
        {
            ShowSettings();
        }
        

        private void NotifyIconTrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            ShowSettings();
        }

        
        private void NotifyIconMenuQuitClick(object sender, RoutedEventArgs e)
        {
            Shutdown();
        }
    }
}