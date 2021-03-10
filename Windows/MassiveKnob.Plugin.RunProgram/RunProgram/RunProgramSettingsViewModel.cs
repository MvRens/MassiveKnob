using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MassiveKnob.Plugin.RunProgram.RunProgram
{
    public class RunProgramSettingsViewModel : INotifyPropertyChanged
    {
        private readonly RunProgramSettings settings;
        

        // ReSharper disable UnusedMember.Global - used by WPF Binding
        public string Filename
        {
            get => settings.Filename;
            set
            {
                if (value == settings.Filename)
                    return;

                settings.Filename = value;
                OnPropertyChanged();
            }
        }

        public string Arguments
        {
            get => settings.Arguments;
            set
            {
                if (value == settings.Arguments)
                    return;

                settings.Arguments = value;
                OnPropertyChanged();
            }
        }
        // ReSharper restore UnusedMember.Global


        public RunProgramSettingsViewModel(RunProgramSettings settings)
        {
            this.settings = settings;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
