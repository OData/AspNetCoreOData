// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.OData.Deltas
{
    /// <summary>
    /// Base class for delta link.
    /// </summary>
    internal abstract class DeltaLinkBase<T> : ITypedDelta, IDeltaLinkBase where T : class
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DeltaLinkBase{T}"/>.
        /// </summary>
        protected DeltaLinkBase()
            : this(typeof(T))
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DeltaLinkBase{T}"/>.
        /// </summary>
        /// <param name="structuralType">The derived structural type for which the changes would be tracked.</param>
        protected DeltaLinkBase(Type structuralType)
        {
            if (structuralType == null)
            {
                throw Error.ArgumentNull(nameof(structuralType));
            }

            if (!typeof(T).IsAssignableFrom(structuralType))
            {
                throw Error.InvalidOperation(SRResources.DeltaEntityTypeNotAssignable, structuralType, typeof(T));
            }

            StructuredType = structuralType;
        }

        /// <inheritdoc />
        public abstract DeltaItemKind Kind { get; }

        /// <summary>
        ///  Gets the actual type of the structural object for which the changes are tracked.
        /// </summary>
        public virtual Type StructuredType { get; }

        /// <summary>
        /// Gets the expected type of the entity for which the changes are tracked.
        /// </summary>
        public virtual Type ExpectedClrType => typeof(T);

        /// <summary>
        /// The Uri of the entity from which the relationship is defined, which may be absolute or relative.
        /// </summary>
        public Uri Source { get; set; }

        /// <summary>
        /// The Uri of the related entity, which may be absolute or relative.
        /// </summary>
        public Uri Target { get; set; }

        /// <summary>
        /// The name of the relationship property on the parent object.
        /// </summary>
        public string Relationship { get; set; }
    }
}