using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.OData.Edm;
using QueryBuilder.Abstracts;
using QueryBuilder.Formatter.Value;
using QueryBuilder;

namespace QueryBuilder.Formatter.Value
{
    /// <summary>
    /// Represents an <see cref="IEdmObject"/> that is a collection of <see cref="IEdmEnumObject"/>s.
    /// </summary>
    // TODO: Fix attribute compilation below:
    //[NonValidatingParameterBinding]
    public class EdmEnumObjectCollection : Collection<IEdmEnumObject>, IEdmObject
    {
        private IEdmCollectionTypeReference _edmType;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmEnumObjectCollection"/> class.
        /// </summary>
        /// <param name="edmType">The edm type of the collection.</param>
        public EdmEnumObjectCollection(IEdmCollectionTypeReference edmType)
            : this(edmType, Enumerable.Empty<IEdmEnumObject>().ToList())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmEnumObjectCollection"/> class.
        /// </summary>
        /// <param name="edmType">The edm type of the collection.</param>
        /// <param name="list">The list that is wrapped by the new collection.</param>
        public EdmEnumObjectCollection(IEdmCollectionTypeReference edmType, IList<IEdmEnumObject> list)
            : base(list)
        {
            Initialize(edmType);
        }

        /// <inheritdoc/>
        public IEdmTypeReference GetEdmType()
        {
            return _edmType;
        }

        private void Initialize(IEdmCollectionTypeReference edmType)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }
            if (!edmType.ElementType().IsEnum())
            {
                throw Error.Argument("edmType",
                    SRResources.UnexpectedElementType, edmType.ElementType().ToTraceString(), edmType.ToTraceString(), typeof(IEdmEnumType).Name);
            }

            _edmType = edmType;
        }
    }
}

