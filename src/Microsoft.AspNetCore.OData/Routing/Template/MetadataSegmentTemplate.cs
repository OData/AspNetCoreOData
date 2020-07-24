// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that could match $metadata.
    /// </summary>
    public class MetadataSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Gets the static instace of $metadata
        /// </summary>
        public static MetadataSegmentTemplate Instance { get; } = new MetadataSegmentTemplate();

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataSegmentTemplate" /> class.
        /// </summary>
        private MetadataSegmentTemplate()
        {
        }

        /// <inheritdoc />
        public override string Literal => "$metadata";

        /// <inheritdoc />
        public override IEdmType EdmType => throw new NotSupportedException();

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.Metadata;

        /// <inheritdoc />
        public override bool IsSingle => true;

        /// <inheritdoc />
        public override ODataPathSegment GenerateODataSegment(IEdmModel model,
            IEdmNavigationSource previous, RouteValueDictionary routeValue, QueryString queryString)
        {
            return MetadataSegment.Instance;
        }
    }
}
