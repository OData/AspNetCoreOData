//-----------------------------------------------------------------------------
// <copyright file="DeltaLinkOfTTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Deltas
{
    public class DeltaLinkOfTTests
    {
        [Fact]
        public void CtorDeltaLinkOfT_ThrowsArgumentNull_StructuralType()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new DeltaLink<object>(null), "structuralType");
        }

        [Fact]
        public void CtorDeltaLinkOfT_ThrowsInvalidOperation_NotAssignableFrom()
        {
            // Arrange & Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => new DeltaLink<B>(typeof(A)),
                "The actual entity type 'Microsoft.AspNetCore.OData.Tests.Deltas.DeltaLinkOfTTests+A' is not assignable to the expected type 'Microsoft.AspNetCore.OData.Tests.Deltas.DeltaLinkOfTTests+B'.");
        }

        [Fact]
        public void CtorDeltaLinkOfT_Sets_PropertyValue()
        {
            // Arrange & Act
            DeltaLink<A> link = new DeltaLink<A>(typeof(B))
            {
                Source = new Uri("http://source"),
                Target = new Uri("http://target"),
                Relationship = "any"
            };

            // Assert
            Assert.Equal("http://source", link.Source.OriginalString);
            Assert.Equal("http://target", link.Target.OriginalString);
            Assert.Equal("any", link.Relationship);
            Assert.Equal(DeltaItemKind.DeltaLink, link.Kind);
            Assert.Equal(typeof(B), link.StructuredType);
            Assert.Equal(typeof(A), link.ExpectedClrType);
        }

        private class A { }
        private class B : A { }
    }
}
