//-----------------------------------------------------------------------------
// <copyright file="EdmEnumObject.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData.Abstracts;

namespace Microsoft.AspNetCore.OData.Formatter.Value
{
    /// <summary>
    /// Represents an <see cref="IEdmEnumObject"/> with no backing CLR <see cref="Type"/>.
    /// </summary>
    [NonValidatingParameterBinding]
    public class EdmEnumObject : IEdmEnumObject
    {
        private readonly IEdmType _edmType;

        /// <summary>
        /// Gets the value of the enumeration type.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets whether the enum object is nullable or not.
        /// </summary>
        public bool IsNullable { get; set; }

         /// <summary>
        /// Initializes a new instance of the <see cref="EdmEnumObject"/> class.
        /// </summary>
        /// <param name="edmType">The <see cref="IEdmEnumType"/> of this object.</param>
        /// <param name="value">The value of the enumeration type.</param>
        public EdmEnumObject(IEdmEnumType edmType, string value)
            : this(edmType, value, isNullable: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmEnumObject"/> class.
        /// </summary>
        /// <param name="edmType">The <see cref="IEdmEnumTypeReference"/> of this object.</param>
        /// <param name="value">The value of the enumeration type.</param>
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "EnumDefinition checks the nullable.")]
        public EdmEnumObject(IEdmEnumTypeReference edmType, string value)
            : this(edmType.EnumDefinition(), value, edmType.IsNullable)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmEnumObject"/> class.
        /// </summary>
        /// <param name="edmType">The <see cref="IEdmEnumTypeReference"/> of this object.</param>
        /// <param name="value">The value of the enumeration type.</param>
        /// <param name="isNullable">true if this object can be nullable; otherwise, false.</param>
        public EdmEnumObject(IEdmEnumType edmType, string value, bool isNullable)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }
            _edmType = edmType;
            Value = value;
            IsNullable = isNullable;
        }

        /// <inheritdoc/>
        public IEdmTypeReference GetEdmType()
        {
            return new EdmEnumTypeReference(_edmType as IEdmEnumType, IsNullable);
        }
    }
}
