// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Wrapper
{
    /// <summary>
    /// Encapsulates an <see cref="ODataResourceSet"/>.
    /// </summary>
    public sealed class ODataResourceSetWrapper : ODataResourceSetBaseWrapper
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataResourceSetWrapper"/>.
        /// </summary>
        /// <param name="resourceSet">The wrapped resource set item.</param>
        public ODataResourceSetWrapper(ODataResourceSet resourceSet)
        {
            Resources = new List<ODataResourceWrapper>();
            ResourceSet = resourceSet;
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataResourceSet"/>.
        /// </summary>
        public ODataResourceSet ResourceSet { get; }

        /// <summary>
        /// Gets the nested resources of this ResourceSet.
        /// Resource set only contains resources.
        /// </summary>
        public IList<ODataResourceWrapper> Resources { get; }
    }
}
