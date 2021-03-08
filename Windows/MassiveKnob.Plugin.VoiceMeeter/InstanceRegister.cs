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

        private static readonly object SubscribersLock = new object();
        private static Parameters parameters;
        private static IDisposable parametersSubscriber;
        private static readonly List<Action> ParameterSubscriberActions = new List<Action>();

        private static IDisposable voicemeeterClient;

        
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
                    voicemeeterClient?.Dispose();
                    voicemeeterClient = await global::VoiceMeeter.Remote.Initialize(version);
                });
            }
        }


        public static Task InitializeVoicemeeter()
        {
            return initializeTask ?? Task.CompletedTask;
        }
        
        
        // For the same reason, we can only subscribe to the parameters once, as they will not be "dirty"
        // for other subscribers otherwise
        public static IDisposable SubscribeToParameterChanges(Action action)
        {
            lock (SubscribersLock)
            {
                if (parameters == null)
                {
                    parameters = new Parameters();
                    parametersSubscriber = parameters.Subscribe(x => NotifyParameterSubscribers());
                }
                
                ParameterSubscriberActions.Add(action);
                return new ParametersSubscriber(action);
            }
        }
        
        
        private static void NotifyParameterSubscribers()
        {
            Action[] subscribers;
            
            lock (SubscribersLock)
            {
                subscribers = ParameterSubscriberActions.ToArray();
            }

            foreach (var subscriber in subscribers)
                subscriber();
        }


        private static void RemoveParameterSubscriber(Action action)
        {
            lock (SubscribersLock)
            {
                // ReSharper disable once InvertIf
                if (ParameterSubscriberActions.Remove(action) && ParameterSubscriberActions.Count == 0)
                {
                    parametersSubscriber.Dispose();
                    parametersSubscriber = null;
                        
                    parameters.Dispose();
                    parameters = null;
                }
            }
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
                // ReSharper disable once InvertIf
                if (Instances.Remove(instance) && Instances.Count == 0)
                {
                    voicemeeterClient?.Dispose();
                    voicemeeterClient = null;
                }
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


        private class ParametersSubscriber : IDisposable
        {
            private readonly Action action;

            
            public ParametersSubscriber(Action action)
            {
                this.action = action;
            }
            
            
            public void Dispose()
            {
                InstanceRegister.RemoveParameterSubscriber(action);
            }
        }
    }
}
