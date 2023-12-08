//-----------------------------------------------------------------------------
// <copyright file="DeltaDeletedResourceOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Deltas
{
    /// <summary>
    /// <see cref="DeltaDeletedResource{T}" /> allows and tracks changes to a delta deleted resource.
    /// </summary>
    public class DeltaDeletedResource<T> : Delta<T>, IDeltaDeletedResource where T: class
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DeltaDeletedResource{T}"/>.
        /// </summary>
        public DeltaDeletedResource()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DeltaDeletedResource{T}"/>.
        /// </summary>
        /// <param name="structuralType">The derived entity type which the changes would be tracked.
        /// <paramref name="structuralType"/> should be assignable to instances of <typeparamref name="T"/>.
        /// </param>
        public DeltaDeletedResource(Type structuralType)
            : base(structuralType)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DeltaDeletedResource{T}"/>.
        /// </summary>
        /// <param name="structuralType">The derived entity type for which the changes would be tracked.
        /// <paramref name="structuralType"/> should be assignable to instances of <typeparamref name="T"/>.</param>
        /// <param name="updatableProperties">The set of properties that can be updated or reset. Unknown property
        /// names, including those of dynamic properties, are ignored.</param>
        public DeltaDeletedResource(Type structuralType, IEnumerable<string> updatableProperties)
            : this(structuralType, updatableProperties: updatableProperties, dynamicDictionaryPropertyInfo: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DeltaDeletedResource{T}"/>.
        /// </summary>
        /// <param name="structuralType">The derived entity type which the changes would be tracked.
        /// <paramref name="structuralType"/> should be assignable to instances of <typeparamref name="T"/>.</param>
        /// <param name="updatableProperties">The set of properties that can be updated or reset. Unknown property
        /// names, including those of dynamic properties, are ignored.</param>
        /// <param name="dynamicDictionaryPropertyInfo">The property info that is used as dictionary of dynamic
        /// properties. <c>null</c> means this entity type is not open.</param>
        public DeltaDeletedResource(Type structuralType, IEnumerable<string> updatableProperties, PropertyInfo dynamicDictionaryPropertyInfo)
            : base(structuralType, updatableProperties, dynamicDictionaryPropertyInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DeltaDeletedResource{T}"/>.
        /// </summary>
        /// <param name="structuralType">The derived entity type which the changes would be tracked.
        /// <paramref name="structuralType"/> should be assignable to instances of <typeparamref name="T"/>.</param>
        /// <param name="updatableProperties">The set of properties that can be updated or reset. Unknown property
        /// names, including those of dynamic properties, are ignored.</param>
        /// <param name="dynamicDictionaryPropertyInfo">The property info that is used as dictionary of dynamic
        /// <param name="isComplexType">If structuralType is a complex type.</param>
        /// properties. <c>null</c> means this entity type is not open.</param>
        public DeltaDeletedResource(Type structuralType, IEnumerable<string> updatableProperties, PropertyInfo dynamicDictionaryPropertyInfo, bool isComplexType)
            : base(structuralType, updatableProperties, dynamicDictionaryPropertyInfo, isComplexType)
        {
        }

        /// <inheritdoc />
        public Uri Id { get; set; }

        /// <inheritdoc />
        public DeltaDeletedEntryReason? Reason { get; set; }

        /// <inheritdoc />
        public override DeltaItemKind Kind => DeltaItemKind.DeletedResource;
    }
}
