// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Tests.Commons
{
    public class InMemoryMessage : IODataRequestMessage, IODataResponseMessage, IContainerProvider, IDisposable
    {
        private readonly Dictionary<string, string> headers;

        public InMemoryMessage()
        {
            headers = new Dictionary<string, string>();
        }

        public IEnumerable<KeyValuePair<string, string>> Headers
        {
            get { return this.headers; }
        }

        public int StatusCode { get; set; }

        public Uri Url { get; set; }

        public string Method { get; set; }

        public Stream Stream { get; set; }

        public IServiceProvider Container { get; set; }

        public string GetHeader(string headerName)
        {
            string headerValue;
            return this.headers.TryGetValue(headerName, out headerValue) ? headerValue : null;
        }

        public void SetHeader(string headerName, string headerValue)
        {
            headers[headerName] = headerValue;
        }

        public Stream GetStream()
        {
            return this.Stream;
        }

        public Action DisposeAction { get; set; }

        void IDisposable.Dispose()
        {
            if (this.DisposeAction != null)
            {
                this.DisposeAction();
            }
        }
    }
}
