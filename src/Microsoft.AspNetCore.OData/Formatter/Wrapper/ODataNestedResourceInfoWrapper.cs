// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Wrapper
{
    /// <summary>
    /// Encapsulates an <see cref="ODataNestedResourceInfo"/> and the list of nested items.
    /// </summary>
    public sealed class ODataNestedResourceInfoWrapper : ODataItemWrapper
    {
        private IList<ODataEntityReferenceLinkWrapper> _nestedLinks;

        /// <summary>
        /// Initializes a new instance of <see cref="ODataNestedResourceInfoWrapper"/>.
        /// </summary>
        /// <param name="nestedInfo">The wrapped nested resource info item.</param>
        public ODataNestedResourceInfoWrapper(ODataNestedResourceInfo nestedInfo)
        {
            NestedResourceInfo = nestedInfo;
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataNestedResourceInfo"/>.
        /// </summary>
        public ODataNestedResourceInfo NestedResourceInfo { get; }

        /// <summary>
        /// Gets/sets the wrapped the <see cref="ODataResourceBaseWrapper"/> child.
        /// </summary>
        public ODataResourceBaseWrapper NestedResource { get; set; }

        /// <summary>
        /// Gets/sets the really wrapped the <see cref="ODataResourceSetBaseWrapper"/> child.
        /// </summary>
        public ODataResourceSetBaseWrapper NestedResourceSet { get; set; }

        /// <summary>
        /// Gets/set the nested entity reference(s) that are part of this nested resource info.
        /// A nested resource info for a singleton nested property can only contain one ODataEntityReferenceLink.
        /// A nested resource info for a collection nested property can contain any number of ODataEntityReferenceLink
        /// </summary>
        public IList<ODataEntityReferenceLinkWrapper> NestedLinks => _nestedLinks;

        internal void AppendReferenceLink(ODataEntityReferenceLinkWrapper referenceLink)
        {
            if (_nestedLinks == null)
            {
                _nestedLinks = new List<ODataEntityReferenceLinkWrapper>();
            }

            _nestedLinks.Add(referenceLink);
        }
    }
}
