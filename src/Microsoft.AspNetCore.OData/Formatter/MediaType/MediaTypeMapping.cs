// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.OData.Formatter.MediaType
{
    /// <summary>
    /// A class to support matching media types.
    /// </summary>
    public abstract class MediaTypeMapping
    {
        /// <summary>
        /// Initializes a new instance of a <see cref="MediaTypeMapping"/> with
        /// the given mediaType value.
        /// </summary>
        /// <param name="mediaType">The mediaType that is associated with the request.</param>
        protected MediaTypeMapping(string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue value))
            {
                throw new ArgumentNullException(nameof(mediaType));
            }

            this.MediaType = value;
        }

        /// <summary>
        ///  Gets the media type that is associated with request.
        /// </summary>
        public MediaTypeHeaderValue MediaType { get; protected set; }

        /// <summary>
        /// Returns a value indicating whether this instance can provide a
        /// <see cref="MediaTypeHeaderValue"/> for the given <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> to check.</param>
        /// <returns>If this <paramref name="request"/>'s route data contains it returns <c>1.0</c> otherwise <c>0.0</c>.</returns>
        public abstract double TryMatchMediaType(HttpRequest request);
    }
}
