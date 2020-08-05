// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Microsoft.OData.ModelBuilder
{
    /// <summary>
    /// Represents the configuration for a complex property of a structural type (an entity type or a complex type).
    /// </summary>
    public class ComplexPropertyConfiguration : StructuralPropertyConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexPropertyConfiguration"/> class.
        /// </summary>
        /// <param name="property">The property of the configuration.</param>
        /// <param name="declaringType">The declaring type of the property.</param>
        public ComplexPropertyConfiguration(PropertyInfo property, StructuralTypeConfiguration declaringType)
            : base(property, declaringType)
        {
        }

        /// <inheritdoc />
        public override PropertyKind Kind => PropertyKind.Complex;

        /// <inheritdoc />
        public override Type RelatedClrType => PropertyInfo.PropertyType;

        /// <summary>
        /// Marks the current complex property as nullable.
        /// </summary>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public ComplexPropertyConfiguration IsNullable()
        {
            NullableProperty = true;
            return this;
        }

        /// <summary>
        /// Marks the current complex property as required (non-nullable).
        /// </summary>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public ComplexPropertyConfiguration IsRequired()
        {
            NullableProperty = false;
            return this;
        }
    }
}
