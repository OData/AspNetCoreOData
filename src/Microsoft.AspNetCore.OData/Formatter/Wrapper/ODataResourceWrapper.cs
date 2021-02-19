// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Wrapper
{
    /// <summary>
    /// Encapsulates an <see cref="ODataResource"/>.
    /// </summary>
    public sealed class ODataResourceWrapper : ODataResourceBaseWrapper
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataResourceWrapper"/>.
        /// </summary>
        /// <param name="resource">The wrapped resource item, it could be null.</param>
        public ODataResourceWrapper(ODataResource resource)
        {
            Resource = resource;
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataResource"/>.
        /// </summary>
        public ODataResource Resource { get; }
    }
}
