//-----------------------------------------------------------------------------
// <copyright file="ResponsePreferenceHelpers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.OData.Formatter
{
    internal static class ResponsePreferenceHelpers
    {
        public const string PreferAppliedHeaderName = "Preference-Applied";

        public static void SetResponsePreferAppliedHeader(HttpResponse response)
        {
            if (response == null)
            {
                throw Error.ArgumentNull(nameof(response));
            }

            HttpRequest request = response.HttpContext.Request;

            OmitValuesKind omitValuesKind = request.GetOmitValuesKind();
            if (omitValuesKind == OmitValuesKind.Unknown)
            {
                return;
            }

            string prefer_applied = null;
            if (response.Headers.TryGetValue(PreferAppliedHeaderName, out StringValues values))
            {
                // If there are many "Preference-Applied" headers, pick up the first one.
                prefer_applied = values.FirstOrDefault();
            }

            string omitValuesHead = omitValuesKind == OmitValuesKind.Nulls ?
                "omit-values=nulls" :
                "omit-values=defaults";

            if (prefer_applied == null)
            {
                response.Headers[PreferAppliedHeaderName] = omitValuesHead;
            }
            else
            {
                response.Headers[PreferAppliedHeaderName] = $"{prefer_applied},{omitValuesHead}";
            }
        }
    }
}
