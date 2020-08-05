// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Microsoft.OData.ModelBuilder
{
    /// <summary>
    /// CollectionPropertyConfiguration represents a CollectionProperty on either an EntityType or ComplexType.
    /// </summary>
    public class CollectionPropertyConfiguration : StructuralPropertyConfiguration
    {
        private Type _elementType;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionPropertyConfiguration"/> class.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="declaringType">The declaring EDM type of the property.</param>
        public CollectionPropertyConfiguration(PropertyInfo property, StructuralTypeConfiguration declaringType)
            : base(property, declaringType)
        {
            if (property == null)
            {
                throw Error.ArgumentNull(nameof(property));
            }


            if (!TypeHelper.IsCollection(property.PropertyType, out _elementType))
            {
                throw Error.Argument("property", SRResources.CollectionPropertiesMustReturnIEnumerable, property.Name, property.DeclaringType.FullName);
            }
        }

        /// <inheritdoc />
        public override PropertyKind Kind => PropertyKind.Collection;

        /// <inheritdoc />
        public override Type RelatedClrType => ElementType;

        /// <summary>
        /// Returns the type of Elements in the Collection
        /// </summary>
        public Type ElementType
        {
            get { return _elementType; }
        }

        /// <summary>
        /// Sets the CollectionProperty to nullable.
        /// </summary>
        public CollectionPropertyConfiguration IsNullable()
        {
            NullableProperty = true;
            return this;
        }

        /// <summary>
        /// Sets the CollectionProperty to required (i.e. non-nullable).
        /// </summary>
        public CollectionPropertyConfiguration IsRequired()
        {
            NullableProperty = false;
            return this;
        }
    }
}
