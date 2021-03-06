﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Voicemeeter;

namespace MassiveKnob.Plugin.VoiceMeeter.Base
{
    public class BaseVoiceMeeterSettingsViewModel<T> : BaseVoiceMeeterSettingsViewModel where T : BaseVoiceMeeterSettings
    {
        protected new T Settings => (T)base.Settings;

        public BaseVoiceMeeterSettingsViewModel(T settings) : base(settings)
        {
        }
    }



    public class BaseVoiceMeeterSettingsViewModel : INotifyPropertyChanged, IDisposable
    {
        protected readonly BaseVoiceMeeterSettings Settings;
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Disposed;

        // ReSharper disable UnusedMember.Global - used by WPF Binding
        public IList<VoiceMeeterVersionViewModel> Versions { get; }

        private VoiceMeeterVersionViewModel selectedVersion;
        public VoiceMeeterVersionViewModel SelectedVersion
        {
            get => selectedVersion;
            set
            {
                if (value == selectedVersion)
                    return;

                selectedVersion = value;
                OnPropertyChanged();

                Settings.Version = value?.Version ?? RunVoicemeeterParam.None;
            }
        }
        // ReSharper restore UnusedMember.Global


        public BaseVoiceMeeterSettingsViewModel(BaseVoiceMeeterSettings settings)
        {
            Settings = settings;

            Versions = new List<VoiceMeeterVersionViewModel>
            {
                new VoiceMeeterVersionViewModel(RunVoicemeeterParam.Voicemeeter, "VoiceMeeter Standard"),
                new VoiceMeeterVersionViewModel(RunVoicemeeterParam.VoicemeeterBanana, "VoiceMeeter Banana"),
                new VoiceMeeterVersionViewModel(RunVoicemeeterParam.VoicemeeterPotato, "VoiceMeeter Potato")
            };

            UpdateSelectedVersion();
        }

        
        private void UpdateSelectedVersion()
        {
            selectedVersion = Versions.SingleOrDefault(v => v.Version == Settings.Version) ?? Versions.First();
        }


        public virtual bool IsSettingsProperty(string propertyName)
        {
            // SelectedVersion already trigger a VoiceMeeterVersionChanged for all instances, 
            // which causes the settings to be stored
            return propertyName != nameof(Versions) && propertyName != nameof(SelectedVersion);
        }


        public virtual void VoiceMeeterVersionChanged()
        {
            UpdateSelectedVersion();
            OnDependantPropertyChanged(nameof(SelectedVersion));
        }


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnDependantPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        
        public virtual void Dispose()
        {
            Disposed?.Invoke(this, EventArgs.Empty);
        }
    }

    public class VoiceMeeterVersionViewModel
    {
        public RunVoicemeeterParam Version { get; }
        public string DisplayName { get; }


        public VoiceMeeterVersionViewModel(RunVoicemeeterParam version, string displayName)
        {
            Version = version;
            DisplayName = displayName;
        }
    }
}
