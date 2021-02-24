using System.Threading;
using System.Windows;
using AudioSwitcher.AudioApi;

namespace MassiveKnob.Plugin.CoreAudio.OSD
{
    public static class OSDManager
    {
        private const int OSDTimeout = 2500;
        
        private static OSDWindowViewModel windowViewModel;
        private static Window window;
        private static Timer hideTimer;

        public static void Show(IDevice device)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (window == null)
                {
                    windowViewModel = new OSDWindowViewModel();
                    window = new OSDWindow(windowViewModel);
                    
                    hideTimer = new Timer(state =>
                    {
                        Hide();
                    }, null, OSDTimeout, Timeout.Infinite);
                }
                else
                    hideTimer.Change(OSDTimeout, Timeout.Infinite);

                windowViewModel.SetDevice(device);
                window.Show();
            });
        }

        private static void Hide()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                window?.Close();
                window = null;
                windowViewModel = null;

                hideTimer?.Dispose();
                hideTimer = null;
            });
        }
    }
}
