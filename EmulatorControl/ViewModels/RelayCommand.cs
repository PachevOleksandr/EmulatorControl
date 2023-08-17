using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EmulatorControl.ViewModels
{
    internal sealed class RelayCommand : ICommand
    {
        readonly Action<object?> _execute;
        readonly Predicate<object?> _canExecute;

        public RelayCommand(Action<object?> execute)
        {
            _execute = execute;
            _canExecute = bool (object? o) => true;
        }

        public RelayCommand(Action<object?> execute,
                            Predicate<object?> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            if (CanExecute(parameter))
                _execute(parameter);
        }
    }

}
