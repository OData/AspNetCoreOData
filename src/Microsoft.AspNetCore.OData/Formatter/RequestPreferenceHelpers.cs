// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.OData.Formatter
{
    internal static class RequestPreferenceHelpers
    {
        public const string PreferHeaderName = "Prefer";
        public const string ReturnContentHeaderValue = "return=representation";
        public const string ReturnNoContentHeaderValue = "return=minimal";
        public const string ODataMaxPageSize = "odata.maxpagesize";
        public const string MaxPageSize = "maxpagesize";

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