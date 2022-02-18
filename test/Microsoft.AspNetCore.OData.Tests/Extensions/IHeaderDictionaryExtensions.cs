//-----------------------------------------------------------------------------
// <copyright file="IHeaderDictionaryExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.OData.Tests.Extensions
{
    /// <summary>
    /// Extensions for IHeaderDictionaryExtensions.
    /// </summary>
    public static class IHeaderDictionaryExtensions
    {
        /// <summary>
        /// Add to IfMatch values;
        /// </summary>
        /// <returns>The IfMatch values.</returns>
        public static void AddIfMatch(this IHeaderDictionary headers, EntityTagHeaderValue value)
        {
            StringValues newValue = StringValues.Concat(headers["If-Match"], new StringValues(value.ToString()));
            headers["If-Match"] = newValue;
        }

        /// <summary>
        /// Add to IfNoneMatch values.
        /// </summary>
        /// <returns>The IfNoneMatch values.</returns>
        public static void AddIfNoneMatch(this IHeaderDictionary headers, EntityTagHeaderValue value)
        {
            StringValues newValue = StringValues.Concat(headers["If-None-Match"], new StringValues(value.ToString()));
            headers["If-None-Match"] = newValue;
        }
    }
}
