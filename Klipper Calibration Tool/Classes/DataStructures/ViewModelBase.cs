using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Klipper_Calibration_Tool.Classes.DataStructures
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        #region private data

        //property dependency mapping for the current class
        //Dictionary Key represents a Property name, and the Value is a hashset containing all properties dependent on it.
        private readonly Dictionary<string, HashSet<string>> _dependencyDictionary = new Dictionary<string, HashSet<string>>();

        #endregion

        /// <summary>
        /// Registers a dependency between properties in the derived class.
        /// When parent property throws a change, an additional property change event is generated for all dependent properties.
        /// A check for cyclic dependency exists, but isn't fully tested.
        /// </summary>
        /// <param name="dependentProperty">Property which is dependent on another.</param>
        /// <param name="parentProperty">Property that is depended upon.</param>
        /// <returns>Indicates whether registration was successful.
        /// False in the event of a detected cyclic dependency.</returns>
        protected bool RegisterDependency(string dependentProperty, string parentProperty)
        {
            if(ContainsRefrenceTo(parentProperty,dependentProperty))
                throw new Exception("Cyclic dependency detected");

            return DependentsOf(parentProperty).Add(dependentProperty);
        }

        /// <summary>
        /// Deregister property as a dependency of the parent property
        /// </summary>
        /// <param name="dependentProperty">property that should no longer be called on parent property change</param>
        /// <param name="parentProperty">parent property</param>
        /// <returns>Whether removal was successful.</returns>
        protected bool DeregisterDependency(string dependentProperty, string parentProperty)
        {
            return _dependencyDictionary.ContainsKey(parentProperty) && _dependencyDictionary[parentProperty].Remove(dependentProperty);
        }

        #region private helper functions

        /// <summary>
        /// Recursively check for existance of a property in the dependency tree of a target property.
        /// This is an attempt at a cyclic redundancy check.
        /// If a cyclic redundancy already exists, calling this may cause an infinite loop.
        /// </summary>
        /// <param name="queryProperty">Property to search for</param>
        /// <param name="targetProperty">Property tree to search</param>
        /// <returns>Whether current property or any of its decendents contains a reference to the property in question</returns>
        private bool ContainsRefrenceTo(string queryProperty, string targetProperty)
        {
            HashSet<string> decendents = DependentsOf(targetProperty);
            if (decendents.Contains(queryProperty)) return true;

            foreach (string decendent in decendents)
            {
                if (ContainsRefrenceTo(queryProperty, decendent))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Query dependency tree to retrieve the hashset of dependencies for a given property.
        /// For convenience sake, this function will also initialize empty hashsets for any
        /// non-existant properties and is currently the only way that happens.
        /// </summary>
        /// <param name="propertyName">Property to query for dependents</param>
        /// <returns>Hash set containing the current dependents. This is the Value object in the private dictionary.</returns>
        private HashSet<string> DependentsOf(string propertyName)
        {
            if(!_dependencyDictionary.ContainsKey(propertyName))
                _dependencyDictionary[propertyName] = new HashSet<string>();

            return _dependencyDictionary[propertyName];
        }

        #endregion

        #region INotifyPropertyChanged

        /// <summary>
        /// Event required for INotifyPropertyChanged interface
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Convenience function for use within property setters. Sets value and pushes property change notification when not equal.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storage">Backing storage field (generally private) to be set</param>
        /// <param name="value">Value to assign to the field</param>
        /// <param name="propertyName">Property name to advertise on change.</param>
        /// <returns></returns>
        protected bool SetAndNotify<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Utility function used for pushing out notifications.
        /// Should be called from within set{} method of properties in the derived class.
        /// When this method is called without arguments, it will use the name of the caller as the property name.
        /// As such, it can be called without arguments from within a property set{} method to use the property name automatically.
        /// </summary>
        /// <param name="propertyName">Property name to be used in the Change event.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) return;
       
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            foreach (string childPropertyName in DependentsOf(propertyName))
            {
                OnPropertyChanged(childPropertyName);
            }
        }

        #endregion
    }
}