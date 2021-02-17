// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Wrapper
{
    /// <summary>
    /// Base class for all classes that wrap an <see cref="ODataItem"/>.
    /// </summary>
    public abstract class ODataItemWrapper
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataItemWrapper"/>.
        /// </summary>
        /// <param name="item">The wrapped item.</param>
        protected ODataItemWrapper(ODataItem item)
        {
            Item = item;
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataItem"/>.
        /// </summary>
        public ODataItem Item { get; }
    }
}
