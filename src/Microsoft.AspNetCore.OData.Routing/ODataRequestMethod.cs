// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// An enumeration for request methods.
    /// </summary>
    internal enum ODataRequestMethod
    {
        /// <summary>
        /// An unknown method.
        /// </summary>
        None = -1,

        /// <summary>
        /// "Get"
        /// </summary>
        Get = 0,

        /// <summary>
        /// "Post"
        /// </summary>
        Post,

        /// <summary>
        /// "Put"
        /// </summary>
        Put,

        /// <summary>
        /// "Patch"
        /// </summary>
        Patch,

        /// <summary>
        /// "Delete"
        /// </summary>
        Delete
    }
}
