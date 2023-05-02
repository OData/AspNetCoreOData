//-----------------------------------------------------------------------------
// <copyright file="ODataResourceSetWrapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
            Items = new List<ODataItemWrapper>();
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

        /// <summary>
        /// Gets the nested items of this ResourceSet.
        /// Since we have 'Resources' to contain ODataResource items in the collection (we have to keep it avoid breaking changes).
        /// This list is used to hold other items, for example a primitive or a collection, etc.
        /// In the next major release, we should combine 'Resources' and 'Items' together.
        /// </summary>
        public IList<ODataItemWrapper> Items { get; }
    }
}
