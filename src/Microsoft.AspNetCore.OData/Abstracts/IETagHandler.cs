// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.OData.Abstracts
{
    /// <summary>
    /// Exposes the ability to convert a collection of concurrency property names and values into an <see cref="EntityTagHeaderValue"/>
    /// and parse an <see cref="EntityTagHeaderValue"/> into a list of concurrency property values.
    /// </summary>
    public interface IETagHandler
    {
        /// <summary>
        /// Creates an ETag from concurrency property names and values.
        /// </summary>
        /// <param name="properties">The input property names and values.</param>
        /// <param name="timeZoneInfo">The timezone info.</param>
        /// <returns>The generated ETag string.</returns>
        EntityTagHeaderValue CreateETag(IDictionary<string, object> properties, TimeZoneInfo timeZoneInfo = null);

        /// <summary>
        /// Parses an ETag header value into concurrency property names and values.
        /// </summary>
        /// <param name="etagHeaderValue">The ETag header value.</param>
        /// <returns>Concurrency property names and values.</returns>
        IDictionary<string, object> ParseETag(EntityTagHeaderValue etagHeaderValue);
    }
}
