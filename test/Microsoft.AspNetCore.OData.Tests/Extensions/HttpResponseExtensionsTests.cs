//-----------------------------------------------------------------------------
// <copyright file="HttpResponseExtensionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Extensions;

public class HttpResponseExtensionsTests
{
    [Fact]
    public void IsSuccessStatusCode_ReturnsCorrectly()
    {
        // null
        HttpResponse response = null;
        Assert.False(response.IsSuccessStatusCode());

        // 500
        response = new DefaultHttpContext().Response;
        response.StatusCode = 500;
        Assert.False(response.IsSuccessStatusCode());

        // 100
        response.StatusCode = 100;
        Assert.False(response.IsSuccessStatusCode());

        // Success
        response.StatusCode = 202;
        Assert.True(response.IsSuccessStatusCode());
    }
}
