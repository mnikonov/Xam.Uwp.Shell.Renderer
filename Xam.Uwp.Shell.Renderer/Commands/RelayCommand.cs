namespace Xam.Uwp.Shell.Renderer.Commands
{
    #region Usings
    using System;
    using System.Windows.Input;

    #endregion

    internal class RelayCommand<T> : ICommand
    {
        #region Fields

        private readonly Func<T, bool> canExecute;

        private readonly Action<T> execute;

        #endregion

        #region Constructors

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        #endregion

        #region Events

        public event EventHandler CanExecuteChanged;

        #endregion
        
        #region Public Methods

        public bool CanExecute(object parameter)
        {
            if (canExecute == null) return true;

            if (parameter is T value) return canExecute(value);

            return false;
        }

        public void Execute(object parameter)
        {
            if (parameter is T value) execute(value);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}