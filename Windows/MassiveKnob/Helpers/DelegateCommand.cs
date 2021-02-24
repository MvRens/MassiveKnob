/*
using System;
using System.Windows.Input;

namespace MassiveKnob.Helpers
{
    public class DelegateCommand : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;


        public DelegateCommand(Action execute) : this(execute, null)
        {
        }


        public DelegateCommand(Action execute, Func<bool> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }


        public bool CanExecute(object parameter)
        {
            return canExecute?.Invoke() ?? true;
        }


        public void Execute(object parameter)
        {
            execute();
        }


        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
    

    public class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> execute;
        private readonly Predicate<T> canExecute;


        public DelegateCommand(Action<T> execute) : this(execute, null)
        {
        }


        public DelegateCommand(Action<T> execute, Predicate<T> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }


        public bool CanExecute(object parameter)
        {
            return canExecute?.Invoke((T)parameter) ?? true;
        }


        public void Execute(object parameter)
        {
            execute((T)parameter);
        }


        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
*/