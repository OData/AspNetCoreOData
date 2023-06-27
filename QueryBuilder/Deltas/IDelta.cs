using System;
using System.Collections.Generic;

namespace ODataQueryBuilder.Deltas
{
    /// <summary>
    /// <see cref="IDelta" /> allows and tracks changes to an object.
    /// </summary>
    public interface IDelta : IDeltaSetItem
    {
        /// <summary>
        /// Returns the Properties that have been modified through this IDelta as an
        /// enumerable of Property Names
        /// </summary>
        IEnumerable<string> GetChangedPropertyNames();

        /// <summary>
        /// Returns the Properties that have not been modified through this IDelta as an
        /// enumerable of Property Names
        /// </summary>
        IEnumerable<string> GetUnchangedPropertyNames();

        /// <summary>
        /// Attempts to set the Property called <paramref name="name"/> to the <paramref name="value"/> specified.
        /// </summary>
        /// <param name="name">The name of the Property</param>
        /// <param name="value">The new value of the Property</param>
        /// <returns>Returns <c>true</c> if successful and <c>false</c> if not.</returns>
        bool TrySetPropertyValue(string name, object value);

        /// <summary>
        /// Attempts to get the value of the Property called <paramref name="name"/> from the underlying Entity.
        /// </summary>
        /// <param name="name">The name of the Property</param>
        /// <param name="value">The value of the Property</param>
        /// <returns>Returns <c>true</c> if the Property was found and <c>false</c> if not.</returns>
        bool TryGetPropertyValue(string name, out object value);

        /// <summary>
        /// Attempts to get the <see cref="Type"/> of the Property called <paramref name="name"/> from the underlying Entity.
        /// </summary>
        /// <param name="name">The name of the Property</param>
        /// <param name="type">The type of the Property</param>
        /// <returns>Returns <c>true</c> if the Property was found and <c>false</c> if not.</returns>
        bool TryGetPropertyType(string name, out Type type);

        /// <summary>
        /// Clears the <see cref="IDelta" />.
        /// </summary>
        void Clear();
    }
}
