using System;
using System.Windows.Input;

namespace ContactTracingPrototype.Documents
{
    internal class SimpleCommand : ICommand
    {
        public Action Action { get; }

        public SimpleCommand(Action action)
        {
            Action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Action();
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}