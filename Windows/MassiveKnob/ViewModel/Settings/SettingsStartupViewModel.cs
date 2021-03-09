using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Win32;

namespace MassiveKnob.ViewModel.Settings
{
    public class SettingsStartupViewModel : INotifyPropertyChanged
    {
        public const string RunKey = @"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        public const string RunValue = @"MassiveKnob";

        // ReSharper disable UnusedMember.Global - used by WPF Binding
        private bool runAtStartup;
        public bool RunAtStartup
        {
            get => runAtStartup;
            set
            {
                if (value == runAtStartup)
                    return;

                runAtStartup = value;
                OnPropertyChanged();

                ApplyRunAtStartup();
            }
        }
        // ReSharper restore UnusedMember.Global


        public SettingsStartupViewModel()
        {
            var runKey = Registry.CurrentUser.OpenSubKey(RunKey, false);
            runAtStartup = runKey?.GetValue(RunValue) != null;
        }


        private void ApplyRunAtStartup()
        {
            var runKey = Registry.CurrentUser.OpenSubKey(RunKey, true);
            Debug.Assert(runKey != null, nameof(runKey) + " != null");

            if (RunAtStartup)
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                Debug.Assert(entryAssembly != null, nameof(entryAssembly) + " != null");

                runKey.SetValue(RunValue, new Uri(entryAssembly.CodeBase).LocalPath);
            }
            else
            {
                runKey.DeleteValue(RunValue, false);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
