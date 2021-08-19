//-----------------------------------------------------------------------------
// <copyright file="ContainerBuilderExtensionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Abstracts
{
    public class ContainerBuilderExtensionsTests
    {
        [Fact]
        public void AddDefaultWebApiServices_ThrowsArgumentNull_Builder()
        {
            // Arrange & Act & Assert
            IContainerBuilder builder = null;
            ExceptionAssert.ThrowsArgumentNull(() => builder.AddDefaultWebApiServices(), "builder");
        }
    }
}
