using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AppDesktop.Base
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class CommandBase : ICommand
    {

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Defines the conditions that determine whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public abstract bool CanExecute(object parameter);

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public abstract void Execute(object parameter);

    }


    /// <summary>
    /// Basic DelegateCommand without any parameter.
    /// </summary>
    public class DelegateCommand : CommandBase
    {

        #region Fields

        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/>.
        /// </summary>
        /// <param name="execute">Action to execute.</param>
        /// <param name="canExecute">Predicate to check before execution.</param>
        public DelegateCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute), "Cannot be null.");
            _canExecute = canExecute;
        }
        #endregion

        #region ICommand Members

        /// <summary>
        /// Defines the conditions that determine whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public override bool CanExecute(object parameter) => _canExecute == null || _canExecute();

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public override void Execute(object parameter) => _execute();

        #endregion

    }


    /// <summary>
    /// DelegateCommand with generic 'Execute' and 'CanExecute' command parameter type.
    /// </summary>
    /// <typeparam name="TParam">Type of command parameter.</typeparam>
    public class DelegateCommand<TParam> : CommandBase
    {

        #region Fields

        private readonly Action<TParam> _execute;
        private readonly Func<TParam, bool> _canExecute;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/>.
        /// </summary>
        /// <param name="execute">Action to execute.</param>
        /// <param name="canExecute">Predicate to check before execution.</param>
        public DelegateCommand(Action<TParam> execute, Func<TParam, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute), "Cannot be null.");
            _canExecute = canExecute;
        }

        #endregion

        #region ICommand Members

        /// <summary>
        /// Defines the conditions that determine whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public override bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((TParam)parameter);
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public override void Execute(object parameter)
        {
            _execute((TParam)parameter);
        }

        #endregion

    }


    /// <summary>
    /// DelegateCommand with generic 'Execute' and 'CanExecute' event and command parameter type.
    /// </summary>
    /// <typeparam name="TParam1">Type of event parameter passed by TriggerAction.</typeparam>
    /// <typeparam name="TParam2">Type of command parameter passed by TriggerAction.</typeparam>
    public class DelegateCommand<TParam1, TParam2> : CommandBase
    {

        #region Fields

        private readonly Action<TParam1, TParam2> _execute;
        private readonly Func<TParam1, TParam2, bool> _canExecute;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/>.
        /// </summary>
        /// <param name="execute">Action to execute.</param>
        /// <param name="canExecute">Predicate to check before execution.</param>
        public DelegateCommand(Action<TParam1, TParam2> execute, Func<TParam1, TParam2, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute), "Cannot be null.");
            _canExecute = canExecute;
        }

        #endregion

        #region ICommand Members

        /// <summary>
        /// Defines the conditions that determine whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public override bool CanExecute(object parameter)
        {
            (object item1, object item2) = ((object, object))parameter;
            return _canExecute == null || _canExecute((TParam1)item1, (TParam2)item2);
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public override void Execute(object parameter)
        {
            (object item1, object item2) = ((object, object))parameter;
            _execute((TParam1)item1, (TParam2)item2);
        }

        #endregion

    }
}
