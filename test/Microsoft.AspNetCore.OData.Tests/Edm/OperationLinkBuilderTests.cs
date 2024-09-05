//-----------------------------------------------------------------------------
// <copyright file="OperationLinkBuilderTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Edm;

public class OperationLinkBuilderTests
{
    [Fact]
    public void CtorOperationLinkBuilder_ThrowsArgumentNull_LinkFactory()
    {
        // Arrange & Act & Assert
        Func<ResourceContext, Uri> linkFactory = null;
        ExceptionAssert.ThrowsArgumentNull(() => new OperationLinkBuilder(linkFactory, false), "linkFactory");

        // Arrange & Act & Assert
        Func<ResourceSetContext, Uri> linkSetFactory = null;
        ExceptionAssert.ThrowsArgumentNull(() => new OperationLinkBuilder(linkSetFactory, false), "linkFactory");
    }

    [Fact]
    public void CtorOperationLinkBuilder_SetsProperties()
    {
        // Arrange & Act & Assert
        Func<ResourceContext, Uri> linkFactory = r => null;
        OperationLinkBuilder linkBuilder = new OperationLinkBuilder(linkFactory, false);

        // Act & Assert
        Assert.Same(linkFactory, linkBuilder.LinkFactory);
        Assert.False(linkBuilder.FollowsConventions);
        Assert.Null(linkBuilder.FeedLinkFactory);

        // Arrange
        Func<ResourceSetContext, Uri> linkSetFactory = r => null;
        OperationLinkBuilder linkSetBuilder = new OperationLinkBuilder(linkSetFactory, true);

        // Act & Assert
        Assert.Null(linkSetBuilder.LinkFactory);
        Assert.True(linkSetBuilder.FollowsConventions);
        Assert.Same(linkSetFactory, linkSetBuilder.FeedLinkFactory);
    }

    [Fact]
    public void BuildLinkOperationLinkBuilder_Works_ResourceContext()
    {
        // Arrange
        Uri uri = new Uri("http://any");
        Func<ResourceContext, Uri> linkFactory = r => uri;
        OperationLinkBuilder linkBuilder = new OperationLinkBuilder(linkFactory, false);

        // Act & Assert
        ResourceContext context = new ResourceContext();
        Assert.Same(uri, linkBuilder.BuildLink(context));

        // Act & Assert
        ResourceSetContext setContext = new ResourceSetContext();
        Assert.Null(linkBuilder.BuildLink(setContext));
    }

    [Fact]
    public void BuildLinkOperationLinkBuilder_Works_ResourceSetContext()
    {
        // Arrange
        Uri uri = new Uri("http://any");
        Func<ResourceSetContext, Uri> linkFactory = r => uri;
        OperationLinkBuilder linkBuilder = new OperationLinkBuilder(linkFactory, false);

        // Act & Assert
        ResourceContext context = new ResourceContext();
        Assert.Null(linkBuilder.BuildLink(context));

        // Act & Assert
        ResourceSetContext setContext = new ResourceSetContext();
        Assert.Same(uri, linkBuilder.BuildLink(setContext));
    }
}
