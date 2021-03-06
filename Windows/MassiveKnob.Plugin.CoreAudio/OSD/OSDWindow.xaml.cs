using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Animation;

namespace MassiveKnob.Plugin.CoreAudio.OSD
{
    /// <summary>
    /// Interaction logic for OSDWindow.xaml
    /// </summary>
    public partial class OSDWindow
    {
        private bool closeStoryBoardCompleted;

        
        public OSDWindow(OSDWindowViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        
        private void OSDWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var desktopArea = Screen.PrimaryScreen.WorkingArea;
            
            Left = (desktopArea.Width - Width) / 2;
            Top = desktopArea.Bottom - Height - 25;
        }

        
        private void OSDWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (closeStoryBoardCompleted) 
                return;

            ((Storyboard)FindResource("CloseStoryboard")).Begin(this);
            e.Cancel = true;
        }

        
        private void CloseStoryboard_Completed(object sender, EventArgs e)
        {
            closeStoryBoardCompleted = true;
            Close();
        }
    }
}
