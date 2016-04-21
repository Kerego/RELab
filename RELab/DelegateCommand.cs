using System;
using System.Windows.Input;

namespace RELab
{
	public class DelegateCommand<T> : ICommand where T : class
	{
		private readonly Predicate<object> _canExecute;
		private readonly Action<T> _execute;

		public event EventHandler CanExecuteChanged;

		public DelegateCommand(Action<T> execute) : this(execute, null)
		{
		}

		public DelegateCommand(Action<T> execute, Predicate<object> canExecute)
		{
			_execute = execute;
			_canExecute = canExecute;
		}

		public bool CanExecute(object parameter)
		{
			if (_canExecute == null)
				return true;

			return _canExecute(parameter);
		}

		public void Execute(object parameter) => _execute.Invoke((parameter as T));

		public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
	}

	public class DelegateCommand : ICommand
	{
		private readonly Predicate<object> _canExecute;
		private readonly Action _execute;

		public event EventHandler CanExecuteChanged;

		public DelegateCommand(Action execute): this(execute, null)
		{
		}

		public DelegateCommand(Action execute, Predicate<object> canExecute)
		{
			_execute = execute;
			_canExecute = canExecute;
		}

		public bool CanExecute(object parameter)
		{
			if (_canExecute == null)
				return true;

			return _canExecute(parameter);
		}

		public void Execute(object parameter) => _execute();

		public void RaiseCanExecuteChanged() =>	CanExecuteChanged?.Invoke(this, EventArgs.Empty);
	}

}
