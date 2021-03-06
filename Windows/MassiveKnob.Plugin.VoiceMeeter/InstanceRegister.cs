using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voicemeeter;

namespace MassiveKnob.Plugin.VoiceMeeter
{
    public static class InstanceRegister
    {
        private static readonly object InstancesLock = new object();
        private static readonly HashSet<IVoiceMeeterAction> Instances = new HashSet<IVoiceMeeterAction>();
        private static Task initializeTask;

        
        // The VoiceMeeter Remote only connects to one instance, so all actions need to be in sync
        private static RunVoicemeeterParam version;
        public static RunVoicemeeterParam Version
        {
            get => version;
            set
            {
                if (value == version)
                    return;
                
                version = value;
                Notify(action => action.VoiceMeeterVersionChanged());

                initializeTask = Task.Run(async () =>
                {
                    await global::VoiceMeeter.Remote.Initialize(version);
                });
            }
        }


        public static Task InitializeVoicemeeter()
        {
            return initializeTask ?? Task.CompletedTask;
        }


        public static void Register(IVoiceMeeterAction instance)
        {
            lock (InstancesLock)
            {
                Instances.Add(instance);
            }
        }


        public static void Unregister(IVoiceMeeterAction instance)
        {
            lock (InstancesLock)
            {
                Instances.Remove(instance);
            }
        }


        public static void Notify(Action<IVoiceMeeterAction> action)
        {
            lock (InstancesLock)
            {
                foreach (var instance in Instances)
                    action(instance);
            }
        }
    }
}
