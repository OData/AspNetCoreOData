//-----------------------------------------------------------------------------
// <copyright file="GetNextPageHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.OData.Query.Query;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.OData.Extensions
{
    /// <summary>
    /// Helper to generate next page links.
    /// </summary>
    internal static class GetNextPageHelper
    {
        [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "<Pending>")]
        internal static Uri GetNextPageLink(Uri requestUri, IEnumerable<KeyValuePair<string, string>> queryParameters, int pageSize, object instance = null, Func<object, string> objectToSkipTokenValue = null)
        {
            Contract.Assert(requestUri != null);
            Contract.Assert(queryParameters != null);

            StringBuilder queryBuilder = new StringBuilder();

            int nextPageSkip = pageSize;

            String skipTokenValue = objectToSkipTokenValue == null ? null : objectToSkipTokenValue(instance);
            //If no value for skiptoken can be extracted; revert to using skip 
            bool useDefaultSkip = String.IsNullOrWhiteSpace(skipTokenValue);

            foreach (KeyValuePair<string, string> kvp in queryParameters)
            {
                string key = kvp.Key.ToLowerInvariant();
                string value = kvp.Value;

                switch (key)
                {
                    case ODataQueryOptionsConstants.Top:
                        int top;
                        if (Int32.TryParse(value, out top))
                        {
                            // We decrease top by the pageSize because that's the number of results we're returning in the current page.
                            // If the $top query option's value is less than or equal to the page size, there is no next page.
                            if (top > pageSize)
                            {
                                value = (top - pageSize).ToString(CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                return null;
                            }
                        }
                        break;
                    case ODataQueryOptionsConstants.Skip:
                        if (useDefaultSkip)
                        {
                            //Need to increment skip only if we are not using skiptoken 
                            int skip;
                            if (Int32.TryParse(value, out skip))
                            {
                                // We increase skip by the pageSize because that's the number of results we're returning in the current page
                                nextPageSkip += skip;
                            }
                        }
                        continue;
                    case ODataQueryOptionsConstants.SkipToken:
                        continue;
                    default:
                        key = kvp.Key; // Leave parameters that are not OData query options in initial form
                        break;
                }

                if (key.Length > 0 && key[0] == '$')
                {
                    // $ is a legal first character in query keys
                    key = '$' + Uri.EscapeDataString(key.Substring(1));
                }
                else
                {
                    key = Uri.EscapeDataString(key);
                }

                value = Uri.EscapeDataString(value);

                queryBuilder.Append(key);
                queryBuilder.Append('=');
                queryBuilder.Append(value);
                queryBuilder.Append('&');
            }

            if (useDefaultSkip)
            {
                queryBuilder.AppendFormat(CultureInfo.CurrentCulture, ODataQueryOptionsConstants.Skip + "={0}", nextPageSkip);
            }
            else
            {
                queryBuilder.AppendFormat(CultureInfo.CurrentCulture, ODataQueryOptionsConstants.SkipToken + "={0}", skipTokenValue);
            }

            UriBuilder uriBuilder = new UriBuilder(requestUri)
            {
                Query = queryBuilder.ToString()
            };

            return uriBuilder.Uri;
        }

        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        internal static Uri GetNextPageLink(Uri requestUri, int pageSize, object instance = null, Func<object, String> objectToSkipTokenValue = null)
        {
            Contract.Assert(requestUri != null);

            Dictionary<string, StringValues> queryValues = QueryHelpers.ParseQuery(requestUri.Query);
            IEnumerable<KeyValuePair<string, string>> queryParameters = queryValues.SelectMany(
                kvp => kvp.Value, (kvp, value) => new KeyValuePair<string, string>(kvp.Key, value));

            return GetNextPageLink(requestUri, queryParameters, pageSize, instance, objectToSkipTokenValue);
        }
    }
}
