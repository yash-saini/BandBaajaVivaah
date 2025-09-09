using System.Windows.Input;

namespace BandBaajaVivaah.WPF.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?>? _execute;
        private readonly Func<object, Task> _executeAsync;
        private readonly Predicate<object?> _canExecute;
        private bool _isExecuting;

        public RelayCommand(Func<object, Task> executeAsync, Predicate<object?> canExecute = null)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute;
        }

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return !_isExecuting && (_canExecute == null || _canExecute(parameter));
        }

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            try
            {
                _isExecuting = true;
                // Raise the CanExecuteChanged event to disable the command while it's executing
                CommandManager.InvalidateRequerySuggested();

                if (_executeAsync != null)
                {
                    await _executeAsync(parameter);
                }
                else
                {
                    _execute?.Invoke(parameter);
                }
            }
            finally
            {
                _isExecuting = false;
                // Raise the CanExecuteChanged event to re-enable the command
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}
