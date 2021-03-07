using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using MassiveKnob.Plugin.VoiceMeeter.Base;

namespace MassiveKnob.Plugin.VoiceMeeter.RunMacro
{
    public class VoiceMeeterRunMacroActionSettingsViewModel : BaseVoiceMeeterSettingsViewModel<VoiceMeeterRunMacroActionSettings>, IDisposable
    {
        private readonly Subject<bool> throttledScriptChanged = new Subject<bool>();
        private readonly IDisposable scriptChangedSubscription;
        
        // ReSharper disable UnusedMember.Global - used by WPF Binding
        public string Script
        {
            get => Settings.Script;
            set
            {
                if (value == Settings.Script)
                    return;

                Settings.Script = value;
                throttledScriptChanged.OnNext(true);
            }
        }
        // ReSharper restore UnusedMember.Global


        // ReSharper disable once SuggestBaseTypeForParameter - by design
        public VoiceMeeterRunMacroActionSettingsViewModel(VoiceMeeterRunMacroActionSettings settings) : base(settings)
        {
            scriptChangedSubscription = throttledScriptChanged
                .Throttle(TimeSpan.FromSeconds(1))
                .Subscribe(b =>
                {
                    OnDependantPropertyChanged(nameof(Script));
                });
        }


        public override void Dispose()
        {
            scriptChangedSubscription?.Dispose();
            throttledScriptChanged?.Dispose();
        }
    }
}
