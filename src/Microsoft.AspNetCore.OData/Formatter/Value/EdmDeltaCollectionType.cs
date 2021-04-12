// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Value
{
    /// <summary>
    /// Implementing IEdmCollectionType to identify collection of DeltaResourceSet.
    /// </summary>
    internal class EdmDeltaCollectionType : IEdmCollectionType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaCollectionType"/> class.
        /// </summary>
        /// <param name="typeReference">The element type reference.</param>
        internal EdmDeltaCollectionType(IEdmTypeReference typeReference)
        {
            ElementType = typeReference ?? throw Error.ArgumentNull("typeReference");
        }

        /// <inheritdoc />
        public EdmTypeKind TypeKind => EdmTypeKind.Collection;

        /// <inheritdoc />
        public IEdmTypeReference ElementType { get; }
    }
}