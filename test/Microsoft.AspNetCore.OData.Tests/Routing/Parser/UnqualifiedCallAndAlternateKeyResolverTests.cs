//-----------------------------------------------------------------------------
// <copyright file="UnqualifiedCallAndAlternateKeyResolverTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Parser;

public class UnqualifiedCallAndAlternateKeyResolverTests
{
    private static IEdmModel _model = GetEdmModel();

    [Fact]
    public void Ctor_ThrowsArgumentNull_Model()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => new UnqualifiedCallAndAlternateKeyResolver(null), "model");
    }

    [Fact]
    public void ResolveUnboundOperations_CanUnboundOperations()
    {
        // Arrange
        UnqualifiedCallAndAlternateKeyResolver resolver = new UnqualifiedCallAndAlternateKeyResolver(_model);

        // Act
        IEnumerable<IEdmOperation> operations = resolver.ResolveUnboundOperations(_model, "UnboundFunc");

        // Assert
        IEdmOperation operation = Assert.Single(operations);
        EdmFunction function = Assert.IsType<EdmFunction>(operation);
        Assert.False(function.IsBound);
        Assert.Equal("UnboundFunc", function.Name);
    }

    [Fact]
    public void ResolveBoundOperations_CanBoundOperations()
    {
        // Arrange
        UnqualifiedCallAndAlternateKeyResolver resolver = new UnqualifiedCallAndAlternateKeyResolver(_model);
        IEdmEntityType type = _model.SchemaElements.OfType<IEdmEntityType>().First();

        // Act
        IEnumerable<IEdmOperation> operations = resolver.ResolveBoundOperations(_model, "BoundFunc", type);

        // Assert
        IEdmOperation operation = Assert.Single(operations);
        EdmFunction function = Assert.IsType<EdmFunction>(operation);
        Assert.True(function.IsBound);
        Assert.Equal("BoundFunc", function.Name);
    }

    private static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<Customer>("Customers");
        builder.Function("UnboundFunc").Returns<int>();
        builder.EntityType<Customer>().Function("BoundFunc").Returns<int>();
        return builder.GetEdmModel();
    }

    private class Customer
    {
        public int Id { get; set; }
    }
}
