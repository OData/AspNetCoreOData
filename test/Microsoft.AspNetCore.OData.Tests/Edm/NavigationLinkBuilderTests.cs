//-----------------------------------------------------------------------------
// <copyright file="NavigationLinkBuilderTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Edm
{
    public class NavigationLinkBuilderTests
    {
        [Fact]
        public void CtorNavigationLinkBuilder_ThrowsArgumentNull_LinkFactory()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new NavigationLinkBuilder(null, true), "navigationLinkFactory");
        }

        [Fact]
        public void CtorNavigationLinkBuilder_SetsProperties()
        {
            // Arrange & Act & Assert
            Func<ResourceContext, IEdmNavigationProperty, Uri> navigationLinkFactory = (r, p) => null;
            NavigationLinkBuilder builder = new NavigationLinkBuilder(navigationLinkFactory, false);

            // Assert
            Assert.Same(navigationLinkFactory, builder.Factory);
            Assert.False(builder.FollowsConventions);
        }
    }
}
