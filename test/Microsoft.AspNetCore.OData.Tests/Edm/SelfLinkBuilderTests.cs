//-----------------------------------------------------------------------------
// <copyright file="SelfLinkBuilderTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Edm
{
    public class SelfLinkBuilderTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_LinkFactory()
        {
            // Arrange & Act & Assert
            Func<ResourceContext, Uri> linkFactory = null;
            ExceptionAssert.ThrowsArgumentNull(() => new SelfLinkBuilder<Uri>(linkFactory, false), "linkFactory");
        }
    }
}
