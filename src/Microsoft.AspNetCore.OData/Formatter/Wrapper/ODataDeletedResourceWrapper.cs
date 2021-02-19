// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Wrapper
{
    /// <summary>
    /// Encapsulates an <see cref="ODataDeletedResource"/>.
    /// </summary>
    public sealed class ODataDeletedResourceWrapper : ODataResourceBaseWrapper
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataDeletedResourceWrapper"/>.
        /// </summary>
        /// <param name="deletedResource">The wrapped deleted resource item.</param>
        public ODataDeletedResourceWrapper(ODataDeletedResource deletedResource)
        {
            DeletedResource = deletedResource;
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataDeletedResource"/>.
        /// </summary>
        public ODataDeletedResource DeletedResource { get; }
    }
}
