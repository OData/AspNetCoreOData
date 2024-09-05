//-----------------------------------------------------------------------------
// <copyright file="DeltaDeletedLinkOfTTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Deltas;

public class DeltaDeletedLinkOfTTests
{
    [Fact]
    public void CtorDeltaDeletedLinkOfT_ThrowsArgumentNull_StructuralType()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => new DeltaDeletedLink<object>(null), "structuralType");
    }

    [Fact]
    public void CtorDeltaDeletedLinkOfT_ThrowsInvalidOperation_NotAssignableFrom()
    {
        // Arrange & Act & Assert
        ExceptionAssert.Throws<InvalidOperationException>(
            () => new DeltaDeletedLink<B>(typeof(A)),
            "The actual entity type 'Microsoft.AspNetCore.OData.Tests.Deltas.DeltaDeletedLinkOfTTests+A' is not assignable to the expected type 'Microsoft.AspNetCore.OData.Tests.Deltas.DeltaDeletedLinkOfTTests+B'.");
    }

    [Fact]
    public void CtorDeltaDeletedLinkOfT_Sets_PropertyValue()
    {
        // Arrange & Act
        DeltaDeletedLink<A> deletedLink = new DeltaDeletedLink<A>(typeof(B))
        {
            Source = new Uri("http://source"),
            Target = new Uri("http://target"),
            Relationship = "any"
        };

        // Assert
        Assert.Equal("http://source", deletedLink.Source.OriginalString);
        Assert.Equal("http://target", deletedLink.Target.OriginalString);
        Assert.Equal("any", deletedLink.Relationship);
        Assert.Equal(DeltaItemKind.DeltaDeletedLink, deletedLink.Kind);
        Assert.Equal(typeof(B), deletedLink.StructuredType);
        Assert.Equal(typeof(A), deletedLink.ExpectedClrType);
    }

    private class A { }
    private class B : A { }
}
