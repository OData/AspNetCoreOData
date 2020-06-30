// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that could match an <see cref="IEdmEntitySet"/>.
    /// </summary>
    public class EntitySetSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySetSegmentTemplate" /> class.
        /// </summary>
        /// <param name="entitySet">The Edm entity set.</param>
        public EntitySetSegmentTemplate(IEdmEntitySet entitySet)
        {
            EntitySet = entitySet ?? throw new ArgumentNullException(nameof(entitySet));
        }

        /// <inheritdoc />
        public override string Template => EntitySet.Name;

        /// <summary>
        /// Gets the wrapped entity set.
        /// </summary>
        public IEdmEntitySet EntitySet { get; }

        /// <inheritdoc />
        public override bool IsSingle => false;

        /// <inheritdoc />
        public override ODataPathSegment GenerateODataSegment(IEdmModel model,
            IEdmNavigationSource previous, RouteValueDictionary routeValue, QueryString queryString)
        {
            return new EntitySetSegment(EntitySet);
        }
    }
}
