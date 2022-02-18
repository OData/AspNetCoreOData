//-----------------------------------------------------------------------------
// <copyright file="HttpRequestTestExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Commons
{
    public static class HttpRequestTestExtensions
    {
        /// <summary>
        /// Helper method to get the key value from a uri.
        /// Usually used by $link action to extract the key value from the url in body.
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <param name="request">The request instance in current context</param>
        /// <param name="uri">OData uri that contains the key value</param>
        /// <returns>The key value</returns>
        public static TKey GetKeyValue<TKey>(this HttpRequest request, Uri uri)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            //get the odata path Ex: ~/entityset/key/$links/navigation
            var odataPath = request.CreateODataPath(uri);
            var keySegment = odataPath.OfType<KeySegment>().FirstOrDefault();
            if (keySegment == null)
            {
                throw new InvalidOperationException("The link does not contain a key.");
            }

            return (TKey)keySegment.Keys.First().Value;
        }

        /// <summary>
        /// Helper method to get the odata path for an arbitrary odata uri.
        /// </summary>
        /// <param name="request">The request instance in current context</param>
        /// <param name="uri">OData uri</param>
        /// <returns>The parsed odata path</returns>
        public static ODataPath CreateODataPath(this HttpRequest request, Uri uri)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            IEdmModel model = request.GetModel();
            IServiceProvider sp = request.GetRouteServices();
            string serviceRoot = request.CreateODataLink();
            ODataUriParser uriParser = new ODataUriParser(model, new Uri(serviceRoot), uri, sp);
            return uriParser.ParsePath();
        }
    }
}
