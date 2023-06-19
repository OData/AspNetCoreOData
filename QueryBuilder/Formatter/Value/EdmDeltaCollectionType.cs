using Microsoft.OData.Edm;
using QueryBuilder;

namespace QueryBuilder.Formatter.Value
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
            ElementType = typeReference ?? throw Error.ArgumentNull(nameof(typeReference));
        }

        /// <inheritdoc />
        public EdmTypeKind TypeKind => EdmTypeKind.Collection;

        /// <inheritdoc />
        public IEdmTypeReference ElementType { get; }
    }
}
