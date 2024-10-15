//-----------------------------------------------------------------------------
// <copyright file="ODataResourceWrapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Wrapper
{
    /// <summary>
    /// Encapsulates an <see cref="ODataResource"/> and <see cref="ODataResourceValue"/>.
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
            ResourceValue = null;
            IsResourceValue = false;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ODataResourceWrapper"/>.
        /// </summary>
        /// <param name="resourceValue">The wrapped resource value, it could NOT be null.</param>
        public ODataResourceWrapper(ODataResourceValue resourceValue)
        {
            ResourceValue = resourceValue ?? throw Error.ArgumentNull(nameof(resourceValue));
            IsResourceValue = true;
            Resource = null;
            IsDeletedResource = false;
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataResource"/>.
        /// </summary>
        public ODataResourceBase Resource { get; }

        /// <summary>
        /// Gets the wrapped <see cref="ODataResourceValue"/>.
        /// Since the ODataResource can't allow the 'ODataResourceValue' as property value,
        /// We have to create a ODataResourceValue to hold the properties.
        /// </summary>
        public ODataResourceValue ResourceValue { get; }

        /// <summary>
        /// Gets a boolean indicating whether the resource is resource value.
        /// </summary>
        public bool IsResourceValue { get; }

        /// <summary>
        /// Gets a boolean indicating whether the resource is deleted resource.
        /// </summary>
        public bool IsDeletedResource { get; }

        /// <summary>
        /// Gets the inner nested resource infos.
        /// </summary>
        public IList<ODataNestedResourceInfoWrapper> NestedResourceInfos { get; } = new List<ODataNestedResourceInfoWrapper>();

        /// <summary>
        /// Gets the nested property infos.
        /// The nested property info is a property without value but could have instance annotations.
        /// </summary>
        public IList<ODataPropertyInfo> NestedPropertyInfos { get; } = new List<ODataPropertyInfo>();
    }
}
