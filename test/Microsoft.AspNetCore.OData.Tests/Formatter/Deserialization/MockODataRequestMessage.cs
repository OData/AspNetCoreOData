//-----------------------------------------------------------------------------
// <copyright file="MockODataRequestMessage.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Deserialization;

internal class MockODataRequestMessage : IODataRequestMessageAsync
{
    Dictionary<string, string> _headers;
    MemoryStream _body;

    public MockODataRequestMessage()
    {
        _headers = new Dictionary<string, string>();
        _body = new MemoryStream();
    }

    public MockODataRequestMessage(MockODataRequestMessage requestMessage)
    {
        _headers = new Dictionary<string, string>(requestMessage._headers);
        _body = new MemoryStream(requestMessage._body.ToArray());
    }

    public string GetHeader(string headerName)
    {
        string value;
        _headers.TryGetValue(headerName, out value);
        return value;
    }

    public Stream GetStream()
    {
        return _body;
    }

    public IEnumerable<KeyValuePair<string, string>> Headers
    {
        get { return _headers; }
    }

    public string Method
    {
        get
        {
            return "Get";
        }
        set
        {
            throw new NotImplementedException();
        }
    }

    public void SetHeader(string headerName, string headerValue)
    {
        _headers[headerName] = headerValue;
    }

    public Task<Stream> GetStreamAsync()
    {
        return Task.FromResult<Stream>(_body);
    }

    public Uri Url
    {
        get
        {
            throw new NotImplementedException();
        }
        set
        {
            throw new NotImplementedException();
        }
    }
}
