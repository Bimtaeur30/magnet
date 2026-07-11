using System;

namespace Mvvm
{
    public interface IRelayCommand
    {
        event Action CanExecuteChanged;
        bool CanExecute();
        void Execute();
        void NotifyCanExecuteChanged();
    }

    public sealed class RelayCommand : IRelayCommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public event Action CanExecuteChanged;

        public bool CanExecute()
        {
            return canExecute == null || canExecute();
        }

        public void Execute()
        {
            if (CanExecute())
            {
                execute();
            }
        }

        public void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke();
        }
    }
}
