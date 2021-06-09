using System;
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
            Application.Current?.Dispatcher.Invoke(() =>
            {
                if (window == null)
                {
                    windowViewModel = new OSDWindowViewModel();
                    window = new OSDWindow(windowViewModel);
                    window.Closed += WindowOnClosed;
                    
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

        
        private static void WindowOnClosed(object sender, EventArgs e)
        {
            // ReSharper disable once PossibleUnintendedReferenceComparison - it's intended.
            if (sender != window) 
                return;
            
            hideTimer?.Dispose();
            hideTimer = null;

            window = null;
        }


        private static void Hide()
        {
            Application.Current?.Dispatcher.Invoke(() =>
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
