//-----------------------------------------------------------------------------
// <copyright file="DeltaOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData.Deltas;

/// <summary>
/// A class the tracks changes (i.e. the Delta) for a particular <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">T is the type of the instance this delta tracks changes for.</typeparam>
[NonValidatingParameterBinding]
public class Delta<T> : Delta, IDelta, ITypedDelta where T : class
{
    // cache property accessors for this type and all its derived types.
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyAccessor<T>>> _propertyCache
        = new ConcurrentDictionary<Type, Dictionary<string, PropertyAccessor<T>>>();

    private Dictionary<string, PropertyAccessor<T>> _allProperties;
    private List<string> _updatableProperties;

    private HashSet<string> _changedProperties;

    // Nested resources or structures changed at this level.
    private IDictionary<string, object> _deltaNestedResources;

    private T _instance;
    private Type _structuredType;
    private bool _isComplexType;

    private readonly PropertyInfo _dynamicDictionaryPropertyinfo;
    private HashSet<string> _changedDynamicProperties;
    private IDictionary<string, object> _dynamicDictionaryCache;

    /// <summary>
    /// Initializes a new instance of <see cref="Delta{T}"/>.
    /// </summary>
    public Delta()
        : this(typeof(T))
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Delta{T}"/>.
    /// </summary>
    /// <param name="structuralType">The derived entity type or complex type for which the changes would be tracked.
    /// <paramref name="structuralType"/> should be assignable to instances of <typeparamref name="T"/>.
    /// </param>
    public Delta(Type structuralType)
        : this(structuralType, updatableProperties: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Delta{T}"/>.
    /// </summary>
    /// <param name="structuralType">The derived entity type or complex type for which the changes would be tracked.
    /// <paramref name="structuralType"/> should be assignable to instances of <typeparamref name="T"/>.
    /// </param>
    /// <param name="updatableProperties">The set of properties that can be updated or reset. Unknown property
    /// names, including those of dynamic properties, are ignored.</param>
    public Delta(Type structuralType, IEnumerable<string> updatableProperties)
        : this(structuralType, updatableProperties: updatableProperties, dynamicDictionaryPropertyInfo: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Delta{T}"/>.
    /// </summary>
    /// <param name="structuralType">The derived entity type or complex type for which the changes would be tracked.
    /// <paramref name="structuralType"/> should be assignable to instances of <typeparamref name="T"/>.
    /// </param>
    /// <param name="updatableProperties">The set of properties that can be updated or reset. Unknown property
    /// names, including those of dynamic properties, are ignored.</param>
    /// <param name="dynamicDictionaryPropertyInfo">The property info that is used as dictionary of dynamic
    /// properties. <c>null</c> means this entity type is not open.</param>
    public Delta(Type structuralType, IEnumerable<string> updatableProperties,
        PropertyInfo dynamicDictionaryPropertyInfo)
        :this(structuralType, updatableProperties, dynamicDictionaryPropertyInfo, isComplexType: false)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Delta{T}"/>.
    /// </summary>
    /// <param name="structuralType">The derived entity type or complex type for which the changes would be tracked.
    /// <paramref name="structuralType"/> should be assignable to instances of <typeparamref name="T"/>.
    /// </param>
    /// <param name="updatableProperties">The set of properties that can be updated or reset. Unknown property
    /// names, including those of dynamic properties, are ignored.</param>
    /// <param name="dynamicDictionaryPropertyInfo">The property info that is used as dictionary of dynamic
    /// properties. <c>null</c> means this entity type is not open.</param>
    /// <param name="isComplexType">If structuralType is a complex type.</param>
    public Delta(Type structuralType, IEnumerable<string> updatableProperties,
        PropertyInfo dynamicDictionaryPropertyInfo, bool isComplexType)
    {
        _dynamicDictionaryPropertyinfo = dynamicDictionaryPropertyInfo;
        _isComplexType = isComplexType;
        Reset(structuralType);
        InitializeProperties(updatableProperties);
    }

    /// <inheritdoc />
    public override DeltaItemKind Kind => DeltaItemKind.Resource;

    /// <inheritdoc/>
    public virtual Type StructuredType => _structuredType;

    /// <inheritdoc/>
    public virtual Type ExpectedClrType => typeof(T);

    /// <summary>
    /// The list of property names that can be updated.
    /// </summary>
    /// <remarks>When the list is modified, any modified properties that were removed from the list are no longer
    /// considered to be changed.</remarks>
    public IList<string> UpdatableProperties => _updatableProperties;

    /// <summary>
    /// If the StructuralType is a Complex type.
    /// </summary>
    public bool IsComplexType => _isComplexType;

    /// <inheritdoc/>
    public override void Clear()
    {
        Reset(_structuredType);
    }

    /// <inheritdoc/>
    public override bool TrySetPropertyValue(string name, object value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw Error.ArgumentNull(nameof(name));
        }

        if (_dynamicDictionaryPropertyinfo != null)
        {
            // Dynamic property can have the same name as the dynamic property dictionary.
            if (name == _dynamicDictionaryPropertyinfo.Name ||
                !_allProperties.ContainsKey(name))
            {
                if (_dynamicDictionaryCache == null)
                {
                    _dynamicDictionaryCache =
                        GetDynamicPropertyDictionary(_dynamicDictionaryPropertyinfo, _instance, create: true);
                }

                _dynamicDictionaryCache[name] = value;
                _changedDynamicProperties.Add(name);
                return true;
            }
        }

        if (value is IDelta || value is IDeltaSet)
        {
            return TrySetNestedResourceInternal(name, value);
        }
        else
        {
            return TrySetPropertyValueInternal(name, value);
        }
    }

    /// <inheritdoc/>
    public override bool TryGetPropertyValue(string name, out object value)
    {
        if (name == null)
        {
            throw Error.ArgumentNull(nameof(name));
        }

        if (_dynamicDictionaryPropertyinfo != null)
        {
            if (_dynamicDictionaryCache == null)
            {
                _dynamicDictionaryCache =
                    GetDynamicPropertyDictionary(_dynamicDictionaryPropertyinfo, _instance, create: false);
            }

            if (_dynamicDictionaryCache != null && _dynamicDictionaryCache.TryGetValue(name, out value))
            {
                return true;
            }
        }

        if (TryGetNestedPropertyValue(name, out value))
        {
            return true;
        }
        else
        {
            // try to retrieve the value of property.
            PropertyAccessor<T> cacheHit;
            if (_allProperties.TryGetValue(name, out cacheHit))
            {
                value = cacheHit.GetValue(_instance);
                return true;
            }
        }

        value = null;
        return false;
    }

    /// <summary>
    /// Attempts to get the value of the nested Property called <paramref name="name"/> from the underlying resource.
    /// <remarks>
    /// Only properties that exist on Entity can be retrieved.
    /// Only modified nested properties can be retrieved.
    /// The nested Property type will be <see cref="IDelta"/> of its defined type.
    /// </remarks>
    /// </summary>
    /// <param name="name">The name of the nested Property</param>
    /// <param name="value">The value of the nested Property</param>
    /// <returns><c>True</c> if the Property was found and is a nested Property</returns>
    internal bool TryGetNestedPropertyValue(string name, out object value)
    {
        if (name == null)
        {
            throw Error.ArgumentNull(nameof(name));
        }

        if (!_deltaNestedResources.ContainsKey(name))
        {
            value = null;
            return false;
        }

        // This is a nested resource, the value returned must be an IDelta<T>
        // from the dictionary of nested resources to allow the traversal of
        // hierarchies of Delta<T>.
        object deltaNestedResource = _deltaNestedResources[name];

        Contract.Assert(deltaNestedResource != null, "deltaNestedResource != null");

        //If DeltaSet collection, we are handling delta collections so the value will be that itself and no need to get instance value
        if (deltaNestedResource is IDeltaSet)
        {
            value = deltaNestedResource;
            return true;
        }

        Contract.Assert(DeltaHelper.IsDeltaOfT(deltaNestedResource.GetType()));

        value = deltaNestedResource;
        return true;
    }

    /// <inheritdoc/>
    public override bool TryGetPropertyType(string name, out Type type)
    {
        if (name == null)
        {
            throw Error.ArgumentNull(nameof(name));
        }

        if (_dynamicDictionaryPropertyinfo != null)
        {
            if (_dynamicDictionaryCache == null)
            {
                _dynamicDictionaryCache =
                    GetDynamicPropertyDictionary(_dynamicDictionaryPropertyinfo, _instance, create: false);
            }

            object dynamicValue;
            if (_dynamicDictionaryCache != null &&
                _dynamicDictionaryCache.TryGetValue(name, out dynamicValue))
            {
                if (dynamicValue == null)
                {
                    type = null;
                    return false;
                }

                type = dynamicValue.GetType();
                return true;
            }
        }

        PropertyAccessor<T> value;
        if (_allProperties.TryGetValue(name, out value))
        {
            type = value.Property.PropertyType;
            return true;
        }

        type = null;
        return false;
    }

    /// <summary>
    /// Returns the instance that holds all the changes (and original values) being tracked by this Delta.
    /// </summary>
    public T GetInstance()
    {
        return _instance;
    }

    /// <summary>
    /// Returns the known properties that have been modified through this <see cref="Delta"/> as an
    /// <see cref="IEnumerable{T}" /> of property Names.
    /// Includes the structural properties at current level.
    /// Does not include the names of the changed dynamic properties.
    /// </summary>
    public override IEnumerable<string> GetChangedPropertyNames()
    {
        return _changedProperties.Intersect(_updatableProperties).Concat(_deltaNestedResources.Keys);
    }

    /// <summary>
    /// Returns the known properties that have not been modified through this <see cref="Delta"/> as an
    /// <see cref="IEnumerable{T}" /> of property Names. Does not include the names of the changed dynamic
    /// properties.
    /// </summary>
    public override IEnumerable<string> GetUnchangedPropertyNames()
    {
        // UpdatableProperties could include arbitrary strings, filter by _allProperties
        return _updatableProperties.Intersect(_allProperties.Keys).Except(GetChangedPropertyNames());
    }

    /// <summary>
    /// Gets the nested resources changed at this level.
    /// </summary>
    public override IDictionary<string, object> GetDeltaNestedNavigationProperties()
    {
        return _deltaNestedResources;
    }

    /// <summary>
    /// Copies the changed property values from the underlying entity (accessible via <see cref="GetInstance()" />)
    /// to the <paramref name="original"/> entity recursively.
    /// </summary>
    /// <param name="original">The entity to be updated.</param>
    public void CopyChangedValues(T original)
    {
        if (original == null)
        {
            throw Error.ArgumentNull(nameof(original));
        }

        // Delta parameter type cannot be derived type of original
        // to prevent unrecognizable information from being applied to original resource.
        if (!_structuredType.IsAssignableFrom(original.GetType()))
        {
            throw Error.Argument(nameof(original), SRResources.DeltaTypeMismatch, _structuredType, original.GetType());
        }

        RuntimeHelpers.EnsureSufficientExecutionStack();

        // For regular non-structural properties at current level.
        PropertyAccessor<T>[] propertiesToCopy =
            _changedProperties.Intersect(_updatableProperties).Select(s => _allProperties[s]).ToArray();
        foreach (PropertyAccessor<T> propertyToCopy in propertiesToCopy)
        {
            propertyToCopy.Copy(_instance, original);
        }

        CopyChangedDynamicValues(original);

        // For nested resources.
        foreach (string nestedResourceName in _deltaNestedResources.Keys)
        {
            // Patch for each nested resource changed under this T.
            dynamic deltaNestedResource = _deltaNestedResources[nestedResourceName];

            if (deltaNestedResource is IDeltaSet)
            {
                // TODO: That's the bulk insert OData Path handler feature,
                // See the comments in https://github.com/OData/AspNetCoreOData/issues/748
                // So far, Let's skip DeltaSet and figure it out later.
                continue;
            }

            dynamic originalNestedResource = null;
            if (!TryGetPropertyRef(original, nestedResourceName, out originalNestedResource))
            {
                throw Error.Argument(nestedResourceName, SRResources.DeltaNestedResourceNameNotFound,
                    nestedResourceName, original.GetType());
            }

            if (originalNestedResource == null)
            {
                // When patching original target of null value, directly set nested resource.
                dynamic deltaObject = _deltaNestedResources[nestedResourceName];
                dynamic instance = deltaObject.GetInstance();

                // Recursively patch up the instance with the nested resources.
                deltaObject.CopyChangedValues(instance);

                _allProperties[nestedResourceName].SetValue(original, instance);
            }
            else
            {
                // Recursively patch the subtree.
                bool isDeltaType = DeltaHelper.IsDeltaOfT(deltaNestedResource.GetType());
                Contract.Assert(isDeltaType, nestedResourceName + "'s corresponding value should be Delta<T> type but is not.");

                Type newType = deltaNestedResource.StructuredType;
                Type originalType = originalNestedResource.GetType();

                if (deltaNestedResource.IsComplexType && newType != originalType)
                {
                    originalNestedResource = ReAssignComplexDerivedType(originalNestedResource, newType, originalType, deltaNestedResource.ExpectedClrType);
                    _structuredType.GetProperty(nestedResourceName).SetValue(original, (object)originalNestedResource);
                }

                deltaNestedResource.CopyChangedValues(originalNestedResource);
            }
        }
    }

    /// <summary>
    /// Copies the unchanged property values from the underlying entity (accessible via <see cref="GetInstance()" />)
    /// to the <paramref name="original"/> entity.
    /// </summary>
    /// <param name="original">The entity to be updated.</param>
    public void CopyUnchangedValues(T original)
    {
        if (original == null)
        {
            throw Error.ArgumentNull(nameof(original));
        }

        if (!_structuredType.IsInstanceOfType(original))
        {
            throw Error.Argument(nameof(original), SRResources.DeltaTypeMismatch, _structuredType, original.GetType());
        }

        IEnumerable<PropertyAccessor<T>> propertiesToCopy = GetUnchangedPropertyNames().Select(s => _allProperties[s]);
        foreach (PropertyAccessor<T> propertyToCopy in propertiesToCopy)
        {
            propertyToCopy.Copy(_instance, original);
        }

        CopyUnchangedDynamicValues(original);
    }

    /// <summary>
    /// Overwrites the <paramref name="original"/> entity with the changes tracked by this Delta.
    /// <remarks>The semantics of this operation are equivalent to a HTTP PATCH operation, hence the name.</remarks>
    /// </summary>
    /// <param name="original">The entity to be updated.</param>
    /// <returns>The updated entity.</returns>
    public T Patch(T original)
    {
        if (IsComplexType)
        {
            original = ReAssignComplexDerivedType(original, _structuredType, original.GetType(), ExpectedClrType) as T;
        }

        CopyChangedValues(original);

        return original;
    }

    /// <summary>
    /// Overwrites the <paramref name="original"/> entity with the values stored in this Delta.
    /// <remarks>The semantics of this operation are equivalent to a HTTP PUT operation, hence the name.</remarks>
    /// </summary>
    /// <param name="original">The entity to be updated.</param>
    public void Put(T original)
    {
        CopyChangedValues(original);
        CopyUnchangedValues(original);
    }

    private dynamic ReAssignComplexDerivedType(dynamic originalValue, Type newType, Type originalType, Type declaredType)
    {
        // As per OASIS discussion, changing a complex type from 1 derived type to another is allowed if both derived type have a common ancestor and the property
        // is declared in terms of a common ancestor. The logic below checks for a common ancestor. Create a new object of the derived type in delta request.
        // And copy the common properties.

        if (newType == originalType)
        {
            return originalValue;
        }

        Type newBaseType = newType;
        HashSet<Type> newBaseTypes = new HashSet<Type>();

        //Iterate till you find the declaring base type and add all that to hashset
        while (newBaseType != null && newBaseType != declaredType)
        {
            newBaseTypes.Add(newBaseType);
            newBaseType = newBaseType.BaseType;
        }

        newBaseTypes.Add(declaredType);

        //Here original type is the type for original (T) resource.
        //We will keep  going to base types and finally will get the Common Basetype for the derived complex types in to the originalType variable.

        //The new Original type, means the new complex type (T) which will replace the current complex type.
        dynamic newOriginalNestedResource = originalValue;

        while (originalType != null)
        {
            if (newBaseTypes.Contains(originalType))
            {
                //Now originalType = common base type of the derived complex types.
                //OriginalNested Resource = T(of current Complex type). We are creating newOriginalNestedResource (T - new complex type).
                newOriginalNestedResource = Activator.CreateInstance(newType);

                //Here we get all the properties of common base type and get value from original complex type(T) and
                //copy it to the new complex type newOriginalNestedResource(came as a part of Delta)

                foreach (PropertyInfo property in originalType.GetProperties())
                {
                    object value = property.GetValue(originalValue);
                    property.SetValue(newOriginalNestedResource, value);
                }

                break;
            }

            originalType = originalType.BaseType;
        }

        return newOriginalNestedResource;
    }

    private static void CopyDynamicPropertyDictionary(IDictionary<string, object> source,
        IDictionary<string, object> dest, PropertyInfo dynamicPropertyInfo, T targetEntity)
    {
        Contract.Assert(source != null);
        Contract.Assert(dynamicPropertyInfo != null);
        Contract.Assert(targetEntity != null);

        if (source.Count == 0)
        {
            if (dest != null)
            {
                dest.Clear();
            }
        }
        else
        {
            if (dest == null)
            {
                dest = GetDynamicPropertyDictionary(dynamicPropertyInfo, targetEntity, create: true);
            }
            else
            {
                dest.Clear();
            }

            foreach (KeyValuePair<string, object> item in source)
            {
                dest.Add(item);
            }
        }
    }

    private static IDictionary<string, object> GetDynamicPropertyDictionary(PropertyInfo propertyInfo,
        T entity, bool create)
    {
        Contract.Assert(propertyInfo != null);
        Contract.Assert(entity != null);

        object propertyValue = propertyInfo.GetValue(entity);
        if (propertyValue != null)
        {
            return (IDictionary<string, object>)propertyValue;
        }

        if (create)
        {
            if (!propertyInfo.CanWrite)
            {
                throw Error.InvalidOperation(SRResources.CannotSetDynamicPropertyDictionary, propertyInfo.Name,
                        entity.GetType().FullName);
            }
            IDictionary<string, object> newPropertyValue = new Dictionary<string, object>();

            propertyInfo.SetValue(entity, newPropertyValue);
            return newPropertyValue;
        }

        return null;
    }

    /// <summary>
    /// Attempts to get the property by the specified name.
    /// </summary>
    /// <param name="structural">The structural object.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="propertyRef">Output for property value.</param>
    /// <returns>true if the property is found; false otherwise.</returns>
    private static bool TryGetPropertyRef(T structural, string propertyName,
        out dynamic propertyRef)
    {
        propertyRef = null;
        PropertyInfo propertyInfo = structural.GetType().GetProperty(propertyName);
        if (propertyInfo != null)
        {
            propertyRef = propertyInfo.GetValue(structural, null);
            return true;
        }

        return false;
    }

    private void Reset(Type structuralType)
    {
        if (structuralType == null)
        {
            throw Error.ArgumentNull(nameof(structuralType));
        }

        if (!typeof(T).IsAssignableFrom(structuralType))
        {
            throw Error.InvalidOperation(SRResources.DeltaEntityTypeNotAssignable, structuralType, typeof(T));
        }

        _instance = Activator.CreateInstance(structuralType) as T;
        _changedProperties = new HashSet<string>();
        _deltaNestedResources = new Dictionary<string, object>();
        _structuredType = structuralType;

        _changedDynamicProperties = new HashSet<string>();
        _dynamicDictionaryCache = null;
    }

    private void InitializeProperties(IEnumerable<string> updatableProperties)
    {
        _allProperties = _propertyCache.GetOrAdd(
            _structuredType,
            (backingType) => backingType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => !IsIgnoredProperty(backingType.GetCustomAttributes(typeof(DataContractAttribute), inherit: true).Any(), p) && (p.GetSetMethod() != null || TypeHelper.IsCollection(p.PropertyType)) && p.GetGetMethod() != null)
                .Select<PropertyInfo, PropertyAccessor<T>>(p => new FastPropertyAccessor<T>(p))
                .ToDictionary(p => p.Property.Name));

        if (updatableProperties != null)
        {
            _updatableProperties = updatableProperties.Intersect(_allProperties.Keys).ToList();
        }
        else
        {
            _updatableProperties = new List<string>(_allProperties.Keys);
        }

        if (_dynamicDictionaryPropertyinfo != null)
        {
            _updatableProperties.Remove(_dynamicDictionaryPropertyinfo.Name);
        }
    }

    private static bool IsIgnoredProperty(bool isTypeDataContract, PropertyInfo propertyInfo)
    {
        //This is for Ignoring the property that matches below criteria
        //1. Its marked as NotMapped
        //2. Its a datacontract type but property is not marked as datamember
        //3. Its marked with IgnoreDataMember (but not where types datacontract and property marked with datamember)

        bool hasNotMappedAttr = propertyInfo.GetCustomAttributes(typeof(NotMappedAttribute), inherit: true).Any();

        if (hasNotMappedAttr)
        {
            return true;
        }

        if (isTypeDataContract)
        {
            return !propertyInfo.GetCustomAttributes(typeof(DataMemberAttribute), inherit: true).Any();
        }

        return propertyInfo.GetCustomAttributes(typeof(IgnoreDataMemberAttribute), inherit: true).Any();
    }

    // Copy changed dynamic properties and leave the unchanged dynamic properties
    private void CopyChangedDynamicValues(T targetEntity)
    {
        if (_dynamicDictionaryPropertyinfo == null)
        {
            return;
        }

        if (_dynamicDictionaryCache == null)
        {
            _dynamicDictionaryCache =
                GetDynamicPropertyDictionary(_dynamicDictionaryPropertyinfo, _instance, create: false);
        }

        IDictionary<string, object> fromDictionary = _dynamicDictionaryCache;
        if (fromDictionary == null)
        {
            return;
        }

        IDictionary<string, object> toDictionary =
            GetDynamicPropertyDictionary(_dynamicDictionaryPropertyinfo, targetEntity, create: false);

        IDictionary<string, object> tempDictionary = toDictionary != null
            ? new Dictionary<string, object>(toDictionary)
            : new Dictionary<string, object>();

        foreach (string dynamicPropertyName in _changedDynamicProperties)
        {
            object dynamicPropertyValue = fromDictionary[dynamicPropertyName];

            // a dynamic property value equal to null, it means to remove this dynamic property
            if (dynamicPropertyValue == null)
            {
                tempDictionary.Remove(dynamicPropertyName);
            }
            else
            {
                if (dynamicPropertyValue is IDelta)
                {
                    dynamic deltaObject = dynamicPropertyValue;
                    dynamic instance = deltaObject.GetInstance();

                    deltaObject.CopyChangedValues(instance);
                    tempDictionary[dynamicPropertyName] = instance;
                }
                else
                {
                    tempDictionary[dynamicPropertyName] = dynamicPropertyValue;
                }
            }
        }

        CopyDynamicPropertyDictionary(tempDictionary, toDictionary, _dynamicDictionaryPropertyinfo,
            targetEntity);
    }

    // Missing dynamic structural properties MUST be removed or set to null in *Put*
    private void CopyUnchangedDynamicValues(T targetEntity)
    {
        if (_dynamicDictionaryPropertyinfo == null)
        {
            return;
        }

        if (_dynamicDictionaryCache == null)
        {
            _dynamicDictionaryCache =
                GetDynamicPropertyDictionary(_dynamicDictionaryPropertyinfo, _instance, create: false);
        }

        IDictionary<string, object> toDictionary =
                GetDynamicPropertyDictionary(_dynamicDictionaryPropertyinfo, targetEntity, create: false);

        if (_dynamicDictionaryCache == null)
        {
            if (toDictionary != null)
            {
                toDictionary.Clear();
            }
        }
        else
        {
            IDictionary<string, object> tempDictionary = toDictionary != null
                ? new Dictionary<string, object>(toDictionary)
                : new Dictionary<string, object>();

            List<string> removedSet = tempDictionary.Keys.Except(_changedDynamicProperties).ToList();

            foreach (string name in removedSet)
            {
                tempDictionary.Remove(name);
            }

            CopyDynamicPropertyDictionary(tempDictionary, toDictionary, _dynamicDictionaryPropertyinfo,
                targetEntity);
        }
    }

    private bool TrySetPropertyValueInternal(string name, object value)
    {
        Debug.Assert(name != null, "Argument name is null");

        if (!(_allProperties.ContainsKey(name) && _updatableProperties.Contains(name)))
        {
            return false;
        }

        PropertyAccessor<T> cacheHit = _allProperties[name];

        if (value == null && !cacheHit.Property.PropertyType.IsNullable())
        {
            return false;
        }

        Type propertyType = cacheHit.Property.PropertyType;
        if (value != null && !TypeHelper.IsCollection(propertyType) && !propertyType.IsAssignableFrom(value.GetType()))
        {
            return false;
        }

        cacheHit.SetValue(_instance, value);
        _changedProperties.Add(name);
        return true;
    }

    private bool TrySetNestedResourceInternal(string name, object deltaNestedResource)
    {
        Debug.Assert(name != null, "Argument name is null");

        if (!(_allProperties.ContainsKey(name) && _updatableProperties.Contains(name)))
        {
            return false;
        }

        if (_deltaNestedResources.ContainsKey(name))
        {
            // Ignore duplicated nested resource.
            return false;
        }

        if (!(deltaNestedResource is IDeltaSet))
        {
            PropertyAccessor<T> cacheHit = _allProperties[name];
            // Get the Delta<{NestedResourceType}>._instance using Reflection.
            FieldInfo field = deltaNestedResource.GetType().GetField("_instance", BindingFlags.NonPublic | BindingFlags.Instance);
            Contract.Assert(field != null, "field != null");
            cacheHit.SetValue(_instance, field.GetValue(deltaNestedResource));
        }

        // Add the nested resource in the hierarchy.
        // Note: We shouldn't add the structural properties to the <code>_changedProperties</code>, which
        // is used for keeping track of changed non-structural properties at current level.
        _deltaNestedResources[name] = deltaNestedResource;

        return true;
    }
}
