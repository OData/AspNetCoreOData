// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.OData.Tests.Extensions
{
    /// <summary>
    /// A class to create HttpRequest.
    /// </summary>
    public static class ResponseFactory
    {
        public static string ReadBody(this HttpResponse response)
        {
            if (response.Body == null)
            {
                return "";
            }

            response.Body.Position = 0;
            string requestBody = "";
            using (StreamReader reader = new StreamReader(response.Body, Encoding.UTF8, true, 1024, true))
            {
                requestBody = reader.ReadToEnd();
            }

            return requestBody;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static HttpResponse Create()
        {
            HttpContext context = new DefaultHttpContext();
            return context.Response;
        }
    }
}
