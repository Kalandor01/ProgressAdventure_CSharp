using System;
using System.Windows.Input;

namespace PAVisualizer
{
    internal class Command : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        private readonly Action _command;

        public Command(Action command)
        {
            _command = command;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _command();
        }
    }
}
