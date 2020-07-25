// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.Routing
{
    internal static class QueryOptionUtils
    {
        public static IDictionary<string, string> GetQueryAlias(QueryString query)
        {
            if (!query.HasValue)
            {
                return null;
            }

            IDictionary<string, string> queryOptions = new Dictionary<string, string>();
            string queryString = query.Value;
            int length = queryString.Length;
            for (int i = 0; i < length; i++)
            {
                int startIndex = i;
                int equalSignIndex = -1;
                while (i < length)
                {
                    char ch = queryString[i];
                    if (ch == '=')
                    {
                        if (equalSignIndex < 0)
                        {
                            equalSignIndex = i;
                        }
                    }
                    else if (ch == '&')
                    {
                        break;
                    }

                    i++;
                }

                string queryOptionsName = null;
                string queryOptionValue = null;
                if (equalSignIndex >= 0)
                {
                    queryOptionsName = queryString.Substring(startIndex, equalSignIndex - startIndex);
                    queryOptionValue = queryString.Substring(equalSignIndex + 1, (i - equalSignIndex) - 1);
                }
                else
                {
                    queryOptionValue = queryString.Substring(startIndex, i - startIndex);
                }

                // COMPAT 31: Query options parsing
                // The System.Web version of the code uses HttpUtility.UrlDecode here, which calls into System.Web's own implementation
                // of the decoder. It's unclear if it's OK to use Uri.UnescapeDataString instead.
                //queryOptionsName = queryOptionsName == null ? null : Uri.UnescapeDataString(queryOptionsName).Trim();
                //queryOptionValue = queryOptionValue == null ? null : Uri.UnescapeDataString(queryOptionValue).Trim();

                queryOptions[queryOptionsName] = queryOptionValue;

                if ((i == (length - 1)) && (queryString[i] == '&'))
                {
                    queryOptions[null] = string.Empty;
                }
            }

            return queryOptions;
        }
    }
}
