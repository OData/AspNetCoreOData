//-----------------------------------------------------------------------------
// <copyright file="ODataResourceWrapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Wrapper
{
    /// <summary>
    /// Encapsulates an <see cref="ODataResource"/>.
    /// </summary>
    public sealed class ODataResourceWrapper : ODataItemWrapper
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataResourceWrapper"/>.
        /// </summary>
        /// <param name="resource">The wrapped resource item, it could be null.</param>
        public ODataResourceWrapper(ODataItem resource)
        {
            if (resource == null || resource is ODataResourceBase)
            {
                Resource = (ODataResourceBase)resource;

                IsDeletedResource = Resource != null && Resource is ODataDeletedResource;

                NestedResourceInfos = new List<ODataNestedResourceInfoWrapper>();

                Item = this;
            }
            else if (resource is ODataPrimitiveValue primitiveValue)
            {
                Item = new ODataPrimitiveWrapper(primitiveValue);
            }
            else if (resource is ODataResourceSet resourceSet)
            {
                Item = new ODataResourceSetWrapper(resourceSet);
            }
            else
            {
                throw new ODataException("Not allowed!");
            }
        }

        /// <summary>
        /// Gets the real item.
        /// </summary>
        public ODataItemWrapper Item { get; }

        /// <summary>
        /// Gets the wrapped <see cref="ODataResource"/>.
        /// </summary>
        public ODataResourceBase Resource { get; }

        /// <summary>
        /// Gets a boolean indicating whether the resource is deleted resource.
        /// </summary>
        public bool IsDeletedResource { get; }

        /// <summary>
        /// Gets the inner nested resource infos.
        /// </summary>
        public IList<ODataNestedResourceInfoWrapper> NestedResourceInfos { get; }
    }
}
