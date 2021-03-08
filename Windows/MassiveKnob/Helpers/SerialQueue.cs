using System;
using System.Threading.Tasks;

// Original source: https://github.com/Gentlee/SerialQueue
// ReSharper disable UnusedMember.Global - public API

namespace MassiveKnob.Helpers
{
    public class SerialQueue
    {
        private readonly object locker = new object();
        private readonly WeakReference<Task> lastTaskWeakRef = new WeakReference<Task>(null);

        public Task Enqueue(Action action)
        {
            return Enqueue(() =>
            {
                action();
                return true;
            });
        }

        public Task<T> Enqueue<T>(Func<T> function)
        {
            lock (locker)
            {
                var resultTask = lastTaskWeakRef.TryGetTarget(out var lastTask) 
                    ? lastTask.ContinueWith(_ => function(), TaskContinuationOptions.ExecuteSynchronously) 
                    : Task.Run(function);

                lastTaskWeakRef.SetTarget(resultTask);

                return resultTask;
            }
        }

        public Task Enqueue(Func<Task> asyncAction)
        {
            lock (locker)
            {
                var resultTask = lastTaskWeakRef.TryGetTarget(out var lastTask) 
                    ? lastTask.ContinueWith(_ => asyncAction(), TaskContinuationOptions.ExecuteSynchronously).Unwrap() 
                    : Task.Run(asyncAction);

                lastTaskWeakRef.SetTarget(resultTask);

                return resultTask;
            }
        }

        public Task<T> Enqueue<T>(Func<Task<T>> asyncFunction)
        {
            lock (locker)
            {
                var resultTask = lastTaskWeakRef.TryGetTarget(out var lastTask) 
                    ? lastTask.ContinueWith(_ => asyncFunction(), TaskContinuationOptions.ExecuteSynchronously).Unwrap() 
                    : Task.Run(asyncFunction);

                lastTaskWeakRef.SetTarget(resultTask);

                return resultTask;
            }
        }
    }
}
