using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AppDesktop.Base
{
    /// <summary>
    /// ViewModel base class.
    /// </summary>
    public abstract class Mvvm : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        #region Fields

        private readonly ConcurrentDictionary<string, IEnumerable<string>> _validationErrors = new ConcurrentDictionary<string, IEnumerable<string>>();

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets a value that indicates whether the entity has validation errors.
        /// </summary>
        /// <returns>true if the entity currently has validation errors; otherwise, false.</returns>
        public bool HasErrors => _validationErrors.Count > 0;

        #endregion Properties

        #region Events

        /// <summary>
        /// Called when a property has changed.
        /// </summary>
        /// <param name="propertyName">Name of changed property.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Call to raise PropertyChanged event.
        /// </summary>
        /// <param name="sender">Name of changed property.</param>
        /// <param name="propertyChangedEventArgs"></param>
        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            PropertyChanged?.Invoke(sender, propertyChangedEventArgs);
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Call to raise ErrorsChanged event.
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Occurs when the validation errors have changed for a property or for the entire entity.
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        #endregion Events

        #region Services

        /// <summary>
        /// Sets the value of the target property.
        /// </summary>
        /// <typeparam name="TProperty">Type if the property</typeparam>
        /// <param name="store">Reference to the backing field of the property that may be changed.</param>
        /// <param name="value">The new value that shall be assigned to the property.</param>
        /// <param name="propertyName">Name of changed property.</param>
        /// <returns>Return true if property value has changed; otherwise false</returns>
        /// <remarks>
        /// The current approach is to check whether the value that should be assigned to the property is equal the current value.
        /// If true this method returns false and no PropertyChanged event is raised. Else it assigns the new value, raises the PropertyChanged event and returns true.
        /// </remarks>
        protected virtual bool SetProperty<TProperty>(ref TProperty store, TProperty value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<TProperty>.Default.Equals(store, value))
                return false;

            store = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Sets the value of the target property.
        /// </summary>
        /// <typeparam name="TProperty">Type if the property</typeparam>
        /// <param name="store">Reference to the backing field of the property that may be changed.</param>
        /// <param name="value">The new value that shall be assigned to the property.</param>
        /// <param name="validationRule">Property validation rule.</param>
        /// <param name="propertyName">Name of changed property.</param>
        /// <returns>Return true if property value has changed; otherwise false</returns>
        /// <remarks>
        /// The current approach is to check whether the value that should be assigned to the property is equal the current value.
        /// If true this method returns false and no PropertyChanged event is raised. Else it assigns the new value, raises the PropertyChanged event and returns true.
        /// </remarks>
        protected virtual bool SetProperty<TProperty>(ref TProperty store, TProperty value, Func<TProperty, IEnumerable<string>> validationRule, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<TProperty>.Default.Equals(store, value))
                return false;

            store = value;
            ValidatePropertyAsync(propertyName, value, validationRule);
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Sets the value of the target property and additional properties.
        /// </summary>
        /// <typeparam name="TProperty">Type if the property</typeparam>
        /// <param name="store">Reference to the backing field of the property that may be changed.</param>
        /// <param name="value">The new value that shall be assigned to the property.</param>
        /// <param name="propertyName">Name of changed property.</param>
        /// <param name="linkedProperties">A list of additional property names for which the PropertyChanged event should be raised.</param>
        /// <returns>Return true if property value has changed; otherwise false</returns>
        /// <remarks>
        /// The current approach is to check whether the value that should be assigned to the property is equal the current value.
        /// If true this method returns false and no PropertyChanged event is raised. Else it assigns the new value, raises the PropertyChanged event and returns true.
        /// </remarks>
        protected virtual bool SetProperty<TProperty>(ref TProperty store, TProperty value, IEnumerable<string> linkedProperties, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<TProperty>.Default.Equals(store, value))
                return false;

            store = value;
            OnPropertyChanged(propertyName);

            if (linkedProperties == null)
                return true;

            foreach (string linkedProperty in linkedProperties)
                OnPropertyChanged(linkedProperty);

            return true;
        }

        /// <summary>
        /// Sets the value of the target property and additional properties.
        /// </summary>
        /// <typeparam name="TProperty">Type if the property</typeparam>
        /// <param name="store">Reference to the backing field of the property that may be changed.</param>
        /// <param name="value">The new value that shall be assigned to the property.</param>
        /// <param name="validationRule">Property validation rule.</param>
        /// <param name="propertyName">Name of changed property.</param>
        /// <param name="linkedProperties">A list of additional property names for which the PropertyChanged event should be raised.</param>
        /// <returns>Return true if property value has changed; otherwise false</returns>
        /// <remarks>
        /// The current approach is to check whether the value that should be assigned to the property is equal the current value.
        /// If true this method returns false and no PropertyChanged event is raised. Else it assigns the new value, raises the PropertyChanged event and returns true.
        /// </remarks>
        protected virtual bool SetProperty<TProperty>(ref TProperty store, TProperty value, IEnumerable<string> linkedProperties, Func<TProperty, IEnumerable<string>> validationRule, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<TProperty>.Default.Equals(store, value))
                return false;

            store = value;
            ValidatePropertyAsync(propertyName, value, validationRule);
            OnPropertyChanged(propertyName); //Update and validate main property

            if (linkedProperties == null)
                return true;

            /*
             * Current state:
             * - linked properties are not validated
             * - update of linked properties is performed even when main property validation detects error
             * - Exception thrown by bad validation rule bubbles up
             */
            foreach (string linkedProperty in linkedProperties) //Update linked properties
                OnPropertyChanged(linkedProperty);

            return true;
        }

        /// <summary>
        /// Gets the validation errors for a specified property or for the entire entity.
        /// </summary>
        /// <returns>The validation errors for the property or entity.</returns>
        /// <param name="propertyName">The name of the property to retrieve validation errors for; or null or <see cref="F:System.String.Empty" />, to retrieve entity-level errors.</param>
        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || !_validationErrors.ContainsKey(propertyName))
                return null;

            return _validationErrors[propertyName];
        }

        /// <summary>
        /// Refresh all bindings connected to this ViewModel.
        /// </summary>
        public void RefreshAllBindings()
        {
            OnPropertyChanged(string.Empty);
        }

        #endregion Services

        #region Private

        /// <summary>
        /// Validates a property asynchronously on background thread pool.
        /// </summary>
        /// <param name="propertyName">Name of property to validate.</param>
        /// <param name="value">Value that should be validated.</param>
        /// <param name="validationRule">Property validation function.</param>
        protected async void ValidatePropertyAsync<TProperty>(string propertyName, TProperty value, Func<TProperty, IEnumerable<string>> validationRule)
        {
            IEnumerable<string> validationResult = await Task.Run(() => validationRule(value));

            bool changed;
            if (validationResult.Any())
            {
                _validationErrors[propertyName] = validationResult;
                changed = true;
            }
            else
            {
                changed = _validationErrors.TryRemove(propertyName, out validationResult);
            }

            // Raise event only when there were changes
            if (changed)
                OnErrorsChanged(propertyName);
        }

        #endregion Private
    }
}