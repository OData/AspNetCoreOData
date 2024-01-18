//-----------------------------------------------------------------------------
// <copyright file="ODataPathNavigationSourceHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// A handler used to calculate some values based on the odata path.
    /// </summary>
    public class ODataPathNavigationSourceHandler : PathSegmentHandler
    {
        /// <summary>
        /// Gets the path navigation source.
        /// </summary>
        public IEdmNavigationSource NavigationSource { get; private set; }

        /// <summary>
        /// Handle an <see cref="EntitySetSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(EntitySetSegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = segment.EntitySet;
        }

        /// <summary>
        /// Handle a <see cref="KeySegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(KeySegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = segment.NavigationSource;
        }

        /// <summary>
        /// Handle a <see cref="NavigationPropertyLinkSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle</param>
        public override void Handle(NavigationPropertyLinkSegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = segment.NavigationSource;
        }

        /// <summary>
        /// Handle a <see cref="NavigationPropertySegment"/>.
        /// </summary>
        /// <param name="segment">the segment to Handle</param>
        public override void Handle(NavigationPropertySegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = segment.NavigationSource;
        }

        /// <summary>
        /// Handle a <see cref="DynamicPathSegment"/>.
        /// </summary>
        /// <param name="segment">the segment to Handle</param>
        public override void Handle(DynamicPathSegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = null;
        }

        /// <summary>
        /// Handle an <see cref="OperationImportSegment"/>.
        /// </summary>
        /// <param name="segment">the segment to Handle</param>
        public override void Handle(OperationImportSegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = segment.EntitySet;
        }

        /// <summary>
        /// Handle an <see cref="OperationSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(OperationSegment segment)
        {
            Contract.Assert(segment != null);
            NavigationSource = segment.EntitySet;
        }

        /// <summary>
        /// Handle a <see cref="PathTemplateSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(PathTemplateSegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = null;
        }

        /// <summary>
        /// Handle a <see cref="PropertySegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(PropertySegment segment)
        {
            Contract.Assert(segment != null);

            // Not set navigation source to null as the relevant navigation source for the path will be the previous navigation source.
        }

        /// <summary>
        /// Handle a <see cref="SingletonSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(SingletonSegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = segment.Singleton;
        }

        /// <summary>
        /// Handle a <see cref="TypeSegment"/>, we use "cast" for type segment.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(TypeSegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = segment.NavigationSource;
        }

        /// <summary>
        /// Handle a <see cref="ValueSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(ValueSegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = null;
        }

        /// <summary>
        /// Handle a <see cref="CountSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(CountSegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = null;
        }

        /// <summary>
        /// Handle a <see cref="BatchSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(BatchSegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = null;
        }

        /// <summary>
        /// Handle a <see cref="MetadataSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(MetadataSegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = null;
        }

        /// <summary>
        /// Handle a general path segment.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(ODataPathSegment segment)
        {
            // ODL doesn't provide the handle function for general path segment
            Contract.Assert(segment != null);

            NavigationSource = null;
        }
    }
}
