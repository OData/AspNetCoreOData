//-----------------------------------------------------------------------------
// <copyright file="TypedEdmStructuredObjectTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Value;

public class TypedEdmStructuredObjectTests
{
    // Manually-built open entity type: declares Id (key) and Name.
    // PasswordHash is present on the CLR class but omitted from the EDM type.
    private static readonly IEdmModel _model;
    private static readonly IEdmEntityTypeReference _typeRef;

    static TypedEdmStructuredObjectTests()
    {
        EdmModel model = new EdmModel();
        EdmEntityType entityType = new EdmEntityType("TestNS", "AccountEntity", null, false, isOpen: true);
        entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
        entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
        // PasswordHash is omitted from the EDM type
        model.AddElement(entityType);
        EdmEntityContainer container = new EdmEntityContainer("TestNS", "DefaultContainer");
        container.AddEntitySet("Accounts", entityType);
        model.AddElement(container);

        _model = model;
        _typeRef = new EdmEntityTypeReference(entityType, true);
    }

    [Fact]
    public void TryGetPropertyValue_ReturnsTrue_ForDeclaredEdmProperty()
    {
        // Arrange: Id is declared in the EDM type and must remain accessible
        var instance = new AccountEntity { Id = 42, Name = "Bob" };
        var obj = new TypedEdmEntityObject(instance, _typeRef, _model);

        // Act
        bool result = obj.TryGetPropertyValue("Id", out object value);

        // Assert: declared EDM property is accessible
        Assert.True(result);
        Assert.Equal(42, value);
    }

    [Fact]
    public void TryGetPropertyValue_ReturnsFalse_ForUndeclaredPropertyName()
    {
        // Arrange: property name not declared in the EDM type
        var instance = new AccountEntity { Id = 1 };
        var obj = new TypedEdmEntityObject(instance, _typeRef, _model);

        // Act — "GetType" is a real CLR member but is a method, not a property
        bool result = obj.TryGetPropertyValue("GetType", out object value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void GetOrCreatePropertyGetter_CachesGetter_ForDeclaredEdmProperty()
    {
        // Arrange
        var cacheField = typeof(TypedEdmStructuredObject).GetField(
            "_propertyGetterCache", BindingFlags.Static | BindingFlags.NonPublic);
        var cache = (ConcurrentDictionary<(string, Type), Func<object, object>>)cacheField.GetValue(null);

        (string, Type) declaredKey = ("Id", typeof(AccountEntity));
        cache.TryRemove(declaredKey, out _);

        // Act
        Func<object, object> getter = TypedEdmStructuredObject.GetOrCreatePropertyGetter(
            typeof(AccountEntity), "Id", _typeRef, _model);

        // Assert: a non-null getter is returned and stored in the cache for efficiency
        Assert.NotNull(getter);
        Assert.True(cache.ContainsKey(declaredKey));
    }

    // -------------------------------------------------------------------------
    // TypedEdmComplexObject — open complex type, same base-class behaviour
    // -------------------------------------------------------------------------

    [Fact]
    public void TypedEdmComplexObject_TryGetPropertyValue_ReturnsTrue_ForDeclaredEdmProperty()
    {
        // Arrange
        EdmModel model = new EdmModel();
        EdmComplexType complexType = new EdmComplexType("TestNS", "Address2", null, false, isOpen: true);
        complexType.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String);
        model.AddElement(complexType);
        var typeRef = new EdmComplexTypeReference(complexType, true);

        var instance = new AddressEntity { Street = "456 Elm St", Secret = "excluded-value" };
        var obj = new TypedEdmComplexObject(instance, typeRef, model);

        // Act
        bool result = obj.TryGetPropertyValue("Street", out object value);

        // Assert: the declared EDM property is accessible
        Assert.True(result);
        Assert.Equal("456 Elm St", value);
    }

    // -------------------------------------------------------------------------
    // TypedEdmUntypedObject — uses EdmUntypedStructuredTypeReference which has
    // no declared properties; TryGetPropertyValue must return false for all names
    // (primary property access path for untyped objects is GetProperties(), not
    // TryGetPropertyValue, so this is safe)
    // -------------------------------------------------------------------------

    [Fact]
    public void TypedEdmStructuredObject_TryGetPropertyValue_ReturnsFalse_WhenInstanceIsNull()
    {
        // Arrange: null instance — should return false without throwing
        var obj = new TypedEdmEntityObject(null, _typeRef, _model);

        // Act
        bool result = obj.TryGetPropertyValue("Id", out object value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }

    // CLR class backing the EDM entity type. PasswordHash is omitted from the EDM model.
    public class AccountEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IDictionary<string, object> DynamicProperties { get; set; }
        public string PasswordHash { get; set; }
    }

    // CLR class backing the EDM complex type. Secret is omitted from the EDM model.
    public class AddressEntity
    {
        public string Street { get; set; }
        public string Secret { get; set; }
    }
}
