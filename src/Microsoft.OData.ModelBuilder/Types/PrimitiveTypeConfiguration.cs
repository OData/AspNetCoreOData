// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.OData.Edm;

namespace Microsoft.OData.ModelBuilder
{
    /// <summary>
    /// Represents a PrimitiveType
    /// </summary>
    public class PrimitiveTypeConfiguration : IEdmTypeConfiguration
    {
        /// <summary>
        /// This constructor is only for unit testing purposes.
        /// To get a PrimitiveTypeConfiguration use ODataModelBuilder.GetTypeConfigurationOrNull(Type)
        /// </summary>
        internal PrimitiveTypeConfiguration(ODataModelBuilder builder, IEdmPrimitiveType edmType, Type clrType)
        {
            ModelBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
            ClrType = clrType ?? throw new ArgumentNullException(nameof(clrType));
            EdmPrimitiveType = edmType ?? throw new ArgumentNullException(nameof(edmType));
        }

        /// <summary>
        /// Gets the backing CLR type of this EDM type.
        /// </summary>
        public Type ClrType { get; }

        /// <summary>
        /// Gets the full name of this EDM type.
        /// </summary>
        public string FullName => EdmPrimitiveType.FullName();

        /// <summary>
        ///  Gets the namespace of this EDM type.
        /// </summary>
        public string Namespace => EdmPrimitiveType.Namespace;

        /// <summary>
        /// Gets the name of this EDM type.
        /// </summary>
        public string Name => EdmPrimitiveType.Name;

        /// <summary>
        /// Gets the <see cref="EdmTypeKind"/> of this EDM type.
        /// </summary>
        public EdmTypeKind Kind => EdmTypeKind.Primitive;

        /// <summary>
        /// Gets the <see cref="ODataModelBuilder"/> used to create this configuration.
        /// </summary>
        public ODataModelBuilder ModelBuilder { get; }

        /// <summary>
        /// Returns the IEdmPrimitiveType associated with this PrimitiveTypeConfiguration
        /// </summary>
        public IEdmPrimitiveType EdmPrimitiveType { get; }
    }
}
