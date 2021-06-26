// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
        public ODataResourceWrapper(ODataResourceBase resource)
        {
            Resource = resource;

            IsDeletedResource = resource != null && resource is ODataDeletedResource;

            NestedResourceInfos = new List<ODataNestedResourceInfoWrapper>();
        }

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
