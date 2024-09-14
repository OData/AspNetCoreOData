//-----------------------------------------------------------------------------
// <copyright file="TypedEdmStructuredObject.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Value;

/// <summary>
/// Represents an <see cref="IEdmStructuredObject"/> backed by a CLR object with a one-to-one mapping.
/// </summary>
internal abstract class TypedEdmStructuredObject : IEdmStructuredObject
{
    private static readonly ConcurrentDictionary<(string, Type), Func<object, object>> _propertyGetterCache =
        new ConcurrentDictionary<(string, Type), Func<object, object>>(new PropertyGetterCacheEqualityComparer());

    private IEdmStructuredTypeReference _edmType;
    private Type _type;

    /// <summary>
    /// Initializes a new instance of the <see cref="TypedEdmStructuredObject"/> class.
    /// </summary>
    /// <param name="instance">The backing CLR instance.</param>
    /// <param name="edmType">The <see cref="IEdmStructuredType"/> of this object.</param>
    /// <param name="edmModel">The <see cref="IEdmModel"/>.</param>
    protected TypedEdmStructuredObject(object instance, IEdmStructuredTypeReference edmType, IEdmModel edmModel)
    {
        Contract.Assert(edmType != null);

        Instance = instance;
        _edmType = edmType;
        _type = instance == null ? null : instance.GetType();
        Model = edmModel;
    }

    /// <summary>
    /// Gets the backing CLR object.
    /// </summary>
    public object Instance { get; private set; }

    /// <summary>
    /// Gets the EDM model.
    /// </summary>
    public IEdmModel Model { get; private set; }

    /// <inheritdoc/>
    public IEdmTypeReference GetEdmType()
    {
        return _edmType;
    }

    /// <inheritdoc/>
    public bool TryGetPropertyValue(string propertyName, out object value)
    {
        if (Instance == null)
        {
            value = null;
            return false;
        }

        Contract.Assert(_type != null);

        Func<object, object> getter = GetOrCreatePropertyGetter(_type, propertyName, _edmType, Model);
        if (getter == null)
        {
            value = null;
            return false;
        }
        else
        {
            value = getter(Instance);
            return true;
        }
    }

    internal static Func<object, object> GetOrCreatePropertyGetter(
        Type type,
        string propertyName,
        IEdmStructuredTypeReference edmType,
        IEdmModel model)
    {
        (string, Type) key = (propertyName, type);
        Func<object, object> getter;

        if (!_propertyGetterCache.TryGetValue(key, out getter))
        {
            IEdmProperty property = edmType.FindProperty(propertyName);
            if (property != null && model != null)
            {
                propertyName = model.GetClrPropertyName(property) ?? propertyName;
            }

            getter = CreatePropertyGetter(type, propertyName);
            _propertyGetterCache[key] = getter;
        }

        return getter;
    }

    private static Func<object, object> CreatePropertyGetter(Type type, string propertyName)
    {
        PropertyInfo property = type.GetProperty(propertyName);

        if (property == null)
        {
            return null;
        }

        var helper = new PropertyHelper(property);

        return helper.GetValue;
    }
}

/// <summary>
/// A custom equality comparer for the property getter cache. 
/// </summary>
internal class PropertyGetterCacheEqualityComparer : IEqualityComparer<(string, Type)>
{
    public bool Equals((string, Type) x, (string, Type) y)
    {
        return x.Item1 == y.Item1 && x.Item2 == y.Item2;
    }

    /// <summary>
    /// This method overrides the default GetHashCode() implementation
    /// for a tuple of (string, Type) to provide a more effective hash code.
    /// </summary>
    /// <param name="obj">The tuple object to calculate a hash code for</param>
    /// <returns>The calculated hash code. </returns>
    public int GetHashCode((string, Type) obj)
    {
        unchecked
        {
            // The choice of 19 as the initial prime number is arbitrary but common in most hash code implementations.
            // Multyplying by a prime number helps to reduce the chance of collisions.
            // The hashcode of each tuple element is combined with the calculated hash to create
            // a more unique hash code for the tuple.
            int hash = 19;
            hash = hash * 23 + obj.Item1.GetHashCode();
            hash = hash * 23 + obj.Item2.GetHashCode();

            return hash;
        }
    }
}
