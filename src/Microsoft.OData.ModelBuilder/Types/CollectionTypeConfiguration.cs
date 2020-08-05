// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.OData.Edm;

namespace Microsoft.OData.ModelBuilder
{
    /// <summary>
    /// Represents a Collection of some named type.
    /// <example>
    /// Collection(Namespace.Customer) or Collection(Namespace.Address).
    /// </example>
    /// </summary>
    public class CollectionTypeConfiguration : IEdmTypeConfiguration
    {
        /// <summary>
        /// Constructs a collection that contains elements of the specified ElementType
        /// and that is represented in CLR using the specified clrType.
        /// </summary>
        /// <param name="elementType">The EdmTypeConfiguration of the elements in the collection</param>
        /// <param name="clrType">The type of this collection when manifested in CLR.</param>
        public CollectionTypeConfiguration(IEdmTypeConfiguration elementType, Type clrType)
        {
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
            ClrType = clrType ?? throw new ArgumentNullException(nameof(clrType));
        }

        /// <summary>
        /// Gets the <see cref="IEdmTypeConfiguration" /> of elements in this collection.
        /// </summary>
        public IEdmTypeConfiguration ElementType { get; }

        /// <summary>
        /// Gets the CLR type associated with this collection type.
        /// </summary>
        public Type ClrType { get; }

        /// <summary>
        /// Gets the fullname (including namespace) of this collection type.
        /// </summary>
        public string FullName
        {
            get
            {
                // There is no need to include the Namespace when it comes from the Edm Namespace.
                return Name;
            }
        }

        /// <summary>
        /// Gets the namespace of this collection type.
        /// </summary>
        public string Namespace
        {
            get { return "Edm"; }
        }

        /// <summary>
        /// Gets the name of this collection type.
        /// </summary>
        public string Name => String.Format(CultureInfo.InvariantCulture, "Collection({0})", ElementType.FullName);

        /// <summary>
        /// Gets the kind of the <see cref="IEdmType" />. In this case, it is <see cref="EdmTypeKind.Collection" />.
        /// </summary>
        public EdmTypeKind Kind => EdmTypeKind.Collection;

        /// <summary>
        /// Gets the <see cref="ODataModelBuilder"/> used to create this configuration.
        /// </summary>
        public ODataModelBuilder ModelBuilder => ElementType.ModelBuilder;
    }
}
