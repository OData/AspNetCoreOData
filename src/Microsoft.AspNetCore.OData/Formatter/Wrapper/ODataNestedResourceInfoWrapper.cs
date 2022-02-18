//-----------------------------------------------------------------------------
// <copyright file="ODataNestedResourceInfoWrapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Wrapper
{
    /// <summary>
    /// Encapsulates an <see cref="ODataNestedResourceInfo"/> and the list of nested items.
    /// </summary>
    public sealed class ODataNestedResourceInfoWrapper : ODataItemWrapper
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataNestedResourceInfoWrapper"/>.
        /// </summary>
        /// <param name="nestedInfo">The wrapped nested resource info item.</param>
        public ODataNestedResourceInfoWrapper(ODataNestedResourceInfo nestedInfo)
        {
            NestedResourceInfo = nestedInfo;
            NestedItems = new List<ODataItemWrapper>();
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataNestedResourceInfo"/>.
        /// </summary>
        public ODataNestedResourceInfo NestedResourceInfo { get; }

        /// <summary>
        /// Gets the nested items that are part of this nested resource info.
        /// </summary>
        public IList<ODataItemWrapper> NestedItems { get; }
    }
}
