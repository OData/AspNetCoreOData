// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Wrapper
{
    /// <summary>
    /// Encapsulates an <see cref="ODataDeltaResourceSet"/>.
    /// </summary>
    /// <remarks>
    /// The Delta resource set could have normal resource or the deleted resource.
    /// The Delta resource set could have delta link.
    /// </remarks>
    public sealed class ODataDeltaResourceSetWrapper : ODataResourceSetBaseWrapper
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataDeltaResourceSetWrapper"/>.
        /// </summary>
        /// <param name="deltaResourceSet">The wrapped delta resource set item.</param>
        public ODataDeltaResourceSetWrapper(ODataDeltaResourceSet deltaResourceSet)
        {
            ResourceBases = new List<ODataResourceBaseWrapper>();
            DeltaLinks = new List<ODataDeltaLinkBaseWrapper>();
            DeltaResourceSet = deltaResourceSet;
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataDeltaResourceSet"/>.
        /// </summary>
        public ODataDeltaResourceSet DeltaResourceSet { get; }

        /// <summary>
        /// Gets the nested resources (resource or deleted resource) of this delta resource set.
        /// </summary>
        public IList<ODataResourceBaseWrapper> ResourceBases { get; }

        /// <summary>
        /// Gets the nested delta links of this delta resource set.
        /// </summary>
        public IList<ODataDeltaLinkBaseWrapper> DeltaLinks { get; }
    }
}
