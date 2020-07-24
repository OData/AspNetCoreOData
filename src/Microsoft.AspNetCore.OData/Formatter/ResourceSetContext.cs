// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter
{
    /// <summary>
    /// Contains context information about the resource set currently being serialized.
    /// </summary>
    public class ResourceSetContext
    {
        /// <summary>
        /// Gets the <see cref="IEdmEntitySetBase"/> this instance belongs to.
        /// </summary>
        public IEdmEntitySetBase EntitySetBase { get; set; }

        /// <summary>
        /// Gets the value of this feed instance.
        /// </summary>
        public object ResourceSetInstance { get; set; }

        /// <summary>
        /// Gets or sets the HTTP request that caused this instance to be generated.
        /// </summary>
        public HttpRequest Request { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="IEdmModel"/> to which this instance belongs.
        /// </summary>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public IEdmModel EdmModel
        {
            get { return Request.GetModel(); }
        }

        /// <summary>
        /// Create a <see cref="ResourceSetContext"/> from an <see cref="ODataSerializerContext"/> and <see cref="IEnumerable"/>.
        /// </summary>
        /// <param name="resourceSetInstance">The instance representing the resourceSet being written.</param>
        /// <param name="writeContext">The serializer context.</param>
        /// <returns>A new <see cref="ResourceSetContext"/>.</returns>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        internal static ResourceSetContext Create(ODataSerializerContext writeContext, IEnumerable resourceSetInstance)
        {
            ResourceSetContext resourceSetContext = new ResourceSetContext
            {
                Request = writeContext.Request,
                EntitySetBase = writeContext.NavigationSource as IEdmEntitySetBase,
              //  Url = writeContext.Url,
                ResourceSetInstance = resourceSetInstance
            };

            return resourceSetContext;
        }
    }
}
