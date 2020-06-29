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
    /// Represents a template that could match an <see cref="IEdmSingleton"/>.
    /// </summary>
    public class SingletonSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonSegmentTemplate" /> class.
        /// </summary>
        /// <param name="singleton">The Edm singleton</param>
        public SingletonSegmentTemplate(IEdmSingleton singleton)
        {
            Singleton = singleton ?? throw new ArgumentNullException(nameof(singleton));
        }

        /// <inheritdoc />
        public override string Template => Singleton.Name;

        /// <summary>
        /// Gets the wrapped Edm singleton.
        /// </summary>
        public IEdmSingleton Singleton { get; }

        /// <inheritdoc />
        public override ODataPathSegment GenerateODataSegment(IEdmModel model,
            IEdmNavigationSource previous, RouteValueDictionary routeValue, QueryString queryString)
        {
            return new SingletonSegment(Singleton);
        }
    }
}
