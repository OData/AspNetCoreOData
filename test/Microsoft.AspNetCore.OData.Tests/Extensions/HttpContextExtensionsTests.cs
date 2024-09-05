//-----------------------------------------------------------------------------
// <copyright file="HttpContextExtensionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Extensions;

public class HttpContextExtensionsTests
{
    [Fact]
    public void ODataFeature_ThrowsArgumentNull_HttpContext()
    {
        // Arrange & Act & Assert
        HttpContext httpContext = null;
        ExceptionAssert.ThrowsArgumentNull(() => httpContext.ODataFeature(), "httpContext");
    }

    [Fact]
    public void ODataBatchFeature_ThrowsArgumentNull_HttpContext()
    {
        // Arrange & Act & Assert
        HttpContext httpContext = null;
        ExceptionAssert.ThrowsArgumentNull(() => httpContext.ODataBatchFeature(), "httpContext");
    }
}
