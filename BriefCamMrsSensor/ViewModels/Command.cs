using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BriefCamMrsSensor.ViewModels
{
    internal class Command : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        private readonly Action action;
        private readonly Action<object> actionParam;
        private readonly Func<bool> condition;

        public bool CanExecute(object parameter)
        {
            return condition();
        }

        public void Execute(object parameter)
        {
            if (action != null)
            {
                action();
            }
            else
            {
                actionParam(parameter);
            }
        }

        public Command(Action action, Func<bool> condition)
        {
            this.action = action;
            this.condition = condition;
        }

        public Command(Action<object> action, Func<bool> condition)
        {
            actionParam = action;
            this.condition = condition;
        }
        public Command(Action action)
        {
            this.action = action;
            this.condition = () => true;
        }
        public Command(Action<object> action)
        {
            actionParam = action;
            this.condition = () => true;
        }
    }
}
