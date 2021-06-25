// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
