// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.Formatter
{
    /// <summary>
    /// An instance of <see cref="ResourceContext{TStructuredType}"/> gets passed to the self link
    /// and navigation link builders and can be used by the link builders to generate links.
    /// </summary>
    /// <typeparam name="TStructuredType">The structural type</typeparam>
    internal class ResourceContext<TStructuredType> : ResourceContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceContext{TStructuredType}"/> class.
        /// </summary>
        public ResourceContext()
            : base()
        {
        }
    }
}
