using System;
using AudioSwitcher.AudioApi.CoreAudio;

namespace MassiveKnob.Plugin.CoreAudio
{
    public static class CoreAudioControllerInstance
    {
        private static readonly Lazy<CoreAudioController> Instance = new Lazy<CoreAudioController>();
        
        
        public static CoreAudioController Acquire()
        {
            return Instance.Value;
        }
    }
}
