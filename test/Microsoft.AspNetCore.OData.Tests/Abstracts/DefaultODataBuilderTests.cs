// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Abstracts
{
    public class DefaultODataBuilderTests
    {
        [Fact]
        public void CtorThrowsArgumentNullServices()
        {
            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => new DefaultODataBuilder(null), "services");
        }
    }
}
