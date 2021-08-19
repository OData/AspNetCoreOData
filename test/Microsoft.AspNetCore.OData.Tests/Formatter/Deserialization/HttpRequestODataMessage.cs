//-----------------------------------------------------------------------------
// <copyright file="HttpRequestODataMessage.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Deserialization
{
    class HttpRequestODataMessage : IODataRequestMessageAsync
    {
        public HttpRequest _httpRequest;

        public Dictionary<string, string> _headers;

        //public HttpRequestODataMessage(HttpRequestMessage request)
        //{
        //    _request = request;
        //    _headers = Enumerable
        //        .Concat<KeyValuePair<string, IEnumerable<string>>>(request.Headers, request.Content.Headers)
        //        .Select(kvp => new KeyValuePair<string, string>(kvp.Key, string.Join(";", kvp.Value)))
        //        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        //}

        public HttpRequestODataMessage(HttpRequest request)
        {
            _httpRequest = request;
            _headers = request.Headers.
                Select(kvp => new KeyValuePair<string, string>(kvp.Key, string.Join(";", kvp.Value)))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public string GetHeader(string headerName)
        {
            return _headers[headerName];
        }

        public Stream GetStream()
        {
            // Can't make this async as the interface requires a return stream, not Task<Stream>
            return _httpRequest.Body;
        }

        public IEnumerable<KeyValuePair<string, string>> Headers
        {
            get { return _headers; }
        }

        public string Method
        {
            get
            {
                return _httpRequest.Method;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void SetHeader(string headerName, string headerValue)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> GetStreamAsync()
        {
            return Task.FromResult(_httpRequest.Body);
        }

        public Uri Url
        {
            get
            {
                return new Uri(UriHelper.GetEncodedUrl(_httpRequest));
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
