// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.OData.Extensions
{
    internal static class RequestPreferenceHelpers
    {
        public const string PreferHeaderName = "Prefer";
        public const string ReturnContentHeaderValue = "return=representation";
        public const string ReturnNoContentHeaderValue = "return=minimal";
        public const string ODataMaxPageSize = "odata.maxpagesize";
        public const string MaxPageSize = "maxpagesize";

        internal static bool RequestPrefersReturnContent(IHeaderDictionary headers)
        {
            StringValues preferences;
            if (headers.TryGetValue(PreferHeaderName, out preferences))
            {
                return (preferences.FirstOrDefault(s => s.IndexOf(ReturnContentHeaderValue, StringComparison.OrdinalIgnoreCase) >= 0) != null);
            }
            return false;
        }

        internal static bool RequestPrefersReturnNoContent(IHeaderDictionary headers)
        {
            StringValues preferences;
            if (headers.TryGetValue(PreferHeaderName, out preferences))
            {
                return (preferences.FirstOrDefault(s => s.IndexOf(ReturnNoContentHeaderValue, StringComparison.OrdinalIgnoreCase) >= 0) != null);
            }
            return false;
        }

        internal static bool RequestPrefersMaxPageSize(IHeaderDictionary headers, out int pageSize)
        {
            pageSize = -1;
            StringValues preferences;
            if (headers.TryGetValue(PreferHeaderName, out preferences))
            {
                pageSize = GetMaxPageSize(preferences, MaxPageSize);
                if (pageSize >= 0)
                {
                    return true;
                }
                //maxpagesize supersedes odata.maxpagesize
                pageSize = GetMaxPageSize(preferences, ODataMaxPageSize);
                if (pageSize >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static int GetMaxPageSize(IEnumerable<string> preferences, string preferenceHeaderName)
        {
            const int Failed = -1;
            string maxPageSize = preferences.FirstOrDefault(s => s.IndexOf(preferenceHeaderName, StringComparison.OrdinalIgnoreCase) >= 0);
            if (String.IsNullOrEmpty(maxPageSize))
            {
                return Failed;
            }
            else
            {
                int index = maxPageSize.IndexOf(preferenceHeaderName, StringComparison.OrdinalIgnoreCase) + preferenceHeaderName.Length;
                String value = String.Empty;
                if (maxPageSize[index++] == '=')
                {
                    while (index < maxPageSize.Length && Char.IsDigit(maxPageSize[index]))
                    {
                        value += maxPageSize[index++];
                    }
                }
                int pageSize = -1;
                if (Int32.TryParse(value, out pageSize))
                {
                    return pageSize;
                }
            }
            return Failed;
        }
        internal static string GetRequestPreferHeader(IHeaderDictionary headers)
        {
            StringValues values;
            if (headers.TryGetValue(PreferHeaderName, out values))
            {
                // If there are many "Prefer" headers, pick up the first one.
                return values.FirstOrDefault();
            }

            return null;
        }
    }
}
