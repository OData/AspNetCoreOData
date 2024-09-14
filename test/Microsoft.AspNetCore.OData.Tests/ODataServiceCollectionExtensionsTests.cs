//-----------------------------------------------------------------------------
// <copyright file="ODataServiceCollectionExtensionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests;

public class ODataServiceCollectionExtensionsTests
{
    [Fact]
    public void AddODataCore_ThrowsArgumentNull_Services()
    {
        // Arrange
        IServiceCollection services = null;

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => services.AddODataCore(), "services");
    }
}
