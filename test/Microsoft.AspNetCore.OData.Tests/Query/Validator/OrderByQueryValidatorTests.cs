//-----------------------------------------------------------------------------
// <copyright file="OrderByQueryValidatorTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Validator;

public class OrderByQueryValidatorTests
{
    private OrderByQueryValidator _validator;
    private ODataQueryContext _context;

    public OrderByQueryValidatorTests()
    {
        _context = ValidationTestHelper.CreateCustomerContext();
        _validator = new OrderByQueryValidator();
    }

    [Fact]
    public void ValidateOrderByQueryValidator_ThrowsOnNullOption()
    {
        // Arrange & Act & Assert
        ExceptionAssert.Throws<ArgumentNullException>(() => _validator.Validate(null, new ODataValidationSettings()));
    }

    [Fact]
    public void ValidateOrderByQueryValidator_ThrowsOnNullSettings()
    {
        // Arrange & Act & Assert
        ExceptionAssert.Throws<ArgumentNullException>(() => _validator.Validate(new OrderByQueryOption("Name eq 'abc'", _context), null));
    }

    [Theory]
    [InlineData("NotSortableProperty")]
    [InlineData("UnsortableProperty")]
    public void ValidateOrderByQueryValidator_ThrowsNotSortableException_ForNotSortableProperty_OnEmptyAllowedPropertiesList(string property)
    {
        // Arrange : empty allowed orderby list
        ODataValidationSettings settings = new ODataValidationSettings();

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() =>
            _validator.Validate(new OrderByQueryOption(String.Format("{0} asc", property), _context), settings),
            string.Format("The property '{0}' cannot be used in the $orderby query option.", property));
    }

    [Theory]
    [InlineData("NotSortableProperty")]
    [InlineData("UnsortableProperty")]
    public void ValidateOrderByQueryValidator_DoesntThrowNotSortableException_ForNotSortableProperty_OnNonEmptyAllowedPropertiesList(string property)
    {
        // Arrange : nonempty allowed orderby list
        ODataValidationSettings settings = new ODataValidationSettings();
        settings.AllowedOrderByProperties.Add(property);

        // Act & Assert
        _validator.Validate(new OrderByQueryOption(string.Format("{0} asc", property), _context), settings);
    }

    [Fact]
    public void ValidateOrderByQueryValidator_NoException_ForAllowedAndSortableUnlimitedProperty_OnEmptyAllowedPropertiesList()
    {
        // Arrange: empty allowed orderby list
        ODataValidationSettings settings = new ODataValidationSettings();

        // Act & Assert
        ExceptionAssert.DoesNotThrow(() => _validator.Validate(new OrderByQueryOption("Name asc", _context), settings));
    }

    [Fact]
    public void ValidateOrderByQueryValidator_NoException_ForAllowedAndSortableUnlimitedProperty_OnNonEmptyAllowedPropertiesList()
    {
        // Arrange: nonempty allowed orbderby list
        ODataValidationSettings settings = new ODataValidationSettings();
        settings.AllowedOrderByProperties.Add("Name");

        // Act & Assert
        ExceptionAssert.DoesNotThrow(() => _validator.Validate(new OrderByQueryOption("Name asc", _context), settings));
    }

    [Theory]
    [InlineData("NotSortableProperty")]
    [InlineData("UnsortableProperty")]
    public void ValidateOrderByQueryValidator_ThrowsNotAllowedException_ForNotAllowedAndSortableLimitedProperty(string property)
    {
        // Arrange
        ODataValidationSettings settings = new ODataValidationSettings();
        settings.AllowedOrderByProperties.Add("Name");

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(new OrderByQueryOption(string.Format("{0} asc", property), _context), settings),
            string.Format("Order by '{0}' is not allowed. To allow it, set the 'AllowedOrderByProperties' property on EnableQueryAttribute or QueryValidationSettings.", property));
    }

    [Fact]
    public void ValidateOrderByQueryValidator_ThrowsNotAllowedException_ForNotAllowedAndSortableUnlimitedProperty()
    {
        // Arrange
        ODataValidationSettings settings = new ODataValidationSettings();
        settings.AllowedOrderByProperties.Add("Address");

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() => _validator.Validate(new OrderByQueryOption("Name asc", _context), settings),
            "Order by 'Name' is not allowed. To allow it, set the 'AllowedOrderByProperties' property on EnableQueryAttribute or QueryValidationSettings.");
    }

    [Fact]
    public void ValidateOrderByQueryValidator_WillNotAllowName()
    {
        // Arrange
        OrderByQueryOption option = new OrderByQueryOption("Name", _context);
        ODataValidationSettings settings = new ODataValidationSettings();
        settings.AllowedOrderByProperties.Add("Id");

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, settings),
            "Order by 'Name' is not allowed. To allow it, set the 'AllowedOrderByProperties' property on EnableQueryAttribute or QueryValidationSettings.");
    }

    [Fact]
    public void ValidateOrderByQueryValidator_WillNotAllowMultipleProperties()
    {
        // Arrange
        OrderByQueryOption option = new OrderByQueryOption("Name desc, Id asc", _context);
        ODataValidationSettings settings = new ODataValidationSettings();
        ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, settings));

        settings.AllowedOrderByProperties.Add("Address");
        settings.AllowedOrderByProperties.Add("Name");

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, settings),
            "Order by 'Id' is not allowed. To allow it, set the 'AllowedOrderByProperties' property on EnableQueryAttribute or QueryValidationSettings.");
    }

    [Fact]
    public void ValidateOrderByQueryValidator_WillAllowId()
    {
        // Arrange
        OrderByQueryOption option = new OrderByQueryOption("Id", _context);
        ODataValidationSettings settings = new ODataValidationSettings();
        settings.AllowedOrderByProperties.Add("Id");

        // Act & Assert
        ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, settings));
    }

    [Fact]
    public void ValidateOrderByQueryValidator_AllowsOrderByIt()
    {
        // Arrange
        OrderByQueryOption option = new OrderByQueryOption("$it", new ODataQueryContext(EdmCoreModel.Instance, typeof(int)));
        ODataValidationSettings settings = new ODataValidationSettings();

        // Act & Assert
        ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, settings));
    }

    [Fact]
    public void ValidateOrderByQueryValidator_AllowsOrderByIt_IfExplicitlySpecified()
    {
        // Arrange
        OrderByQueryOption option = new OrderByQueryOption("$it", new ODataQueryContext(EdmCoreModel.Instance, typeof(int)));
        ODataValidationSettings settings = new ODataValidationSettings { AllowedOrderByProperties = { "$it" } };

        // Act & Assert
        ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, settings));
    }

    [Fact]
    public void ValidateOrderByQueryValidator_DisallowsOrderByIt_IfTurnedOff()
    {
        // Arrange
        _context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
        OrderByQueryOption option = new OrderByQueryOption("$it", _context);
        ODataValidationSettings settings = new ODataValidationSettings();
        settings.AllowedOrderByProperties.Add("dummy");

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(option, settings),
            "Order by '$it' is not allowed. To allow it, set the 'AllowedOrderByProperties' property on EnableQueryAttribute or QueryValidationSettings.");
    }

    [Fact]
    public void ValidateOrderByQueryValidator__ThrowsCountExceeded()
    {
        // Arrange
        OrderByQueryOption option = new OrderByQueryOption("Name desc, Id asc", _context);
        ODataValidationSettings settings = new ODataValidationSettings { MaxOrderByNodeCount = 1 };

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(option, settings),
            "The number of clauses in $orderby query option exceeded the maximum number allowed. The maximum number of $orderby clauses allowed is 1.");
    }

    [Theory]
    // Works with complex properties
    [InlineData("ComplexProperty/Value", "LimitedEntity",
        "The property 'ComplexProperty' cannot be used in the $orderby query option.")]
    // Works with simple properties
    [InlineData("RelatedEntity/RelatedComplexProperty/NotSortableValue", "LimitedEntity",
        "The property 'NotSortableValue' cannot be used in the $orderby query option.")]
    [InlineData("RelatedEntity/RelatedComplexProperty/UnsortableValue", "LimitedEntity",
        "The property 'UnsortableValue' cannot be used in the $orderby query option.")]
    // Works with navigation properties
    [InlineData("RelatedEntity/BackReference/Id", "LimitedEntity",
        "The property 'BackReference' cannot be used in the $orderby query option.")]
    // Works with inheritance
    [InlineData("RelatedEntity/NS.LimitedSpecializedEntity/SpecializedComplexProperty/Value", "LimitedEntity",
        "The property 'SpecializedComplexProperty' cannot be used in the $orderby query option.")]
    // Works with multiple clauses
    [InlineData("Id, ComplexProperty/NotSortableValue", "LimitedEntity",
        "The property 'NotSortableValue' cannot be used in the $orderby query option.")]
    [InlineData("Id, ComplexProperty/UnsortableValue", "LimitedEntity",
        "The property 'UnsortableValue' cannot be used in the $orderby query option.")]
    public void ValidateOrderByQueryValidator_ThrowsIfTryingToValidateALimitedProperty(string query, string edmTypeName, string message)
    {
        // Arrange
        IEdmModel model = GetEdmModel();
        IEdmEntityType edmType = model.SchemaElements.OfType<IEdmEntityType>().Single(t => t.Name == edmTypeName);
        ODataQueryContext context = new ODataQueryContext(model, edmType);
        context.DefaultQueryConfigurations.EnableOrderBy = true;
        OrderByQueryOption option = new OrderByQueryOption(query, context);
        ODataValidationSettings settings = new ODataValidationSettings();

        // Act & Assert
        OrderByQueryValidator validator = new OrderByQueryValidator();
        ExceptionAssert.Throws<ODataException>(() => validator.Validate(option, settings), message);
    }

    [Fact]
    public void ValidateOrderByQueryValidator_DoesntThrowIfTheLeafOfThePathIsWithinTheAllowedProperties()
    {
        // Arrange
        IEdmModel model = GetEdmModel();
        IEdmEntityType edmType = model.SchemaElements.OfType<IEdmEntityType>().Single(t => t.Name == "LimitedEntity");
        ODataQueryContext context = new ODataQueryContext(model, edmType);
        OrderByQueryOption option = new OrderByQueryOption("ComplexProperty/Value", context);
        ODataValidationSettings settings = new ODataValidationSettings();
        settings.AllowedOrderByProperties.Add("ComplexProperty");
        settings.AllowedOrderByProperties.Add("Value");

        // Act & Assert
        IOrderByQueryValidator validator = context.GetOrderByQueryValidator();
        ExceptionAssert.DoesNotThrow(() => validator.Validate(option, settings));
    }

    [Fact]
    public void ValidateOrderByQueryValidator_ThrowsIfTheLeafOfThePathIsntWithinTheAllowedProperties()
    {
        // Arrange
        IEdmModel model = GetEdmModel();
        IEdmEntityType edmType = model.SchemaElements.OfType<IEdmEntityType>().Single(t => t.Name == "LimitedEntity");
        ODataQueryContext context = new ODataQueryContext(model, edmType);
        OrderByQueryOption option = new OrderByQueryOption("ComplexProperty/Value", context);
        ODataValidationSettings settings = new ODataValidationSettings();
        settings.AllowedOrderByProperties.Add("NotSortableProperty");

        // Act & Assert
        IOrderByQueryValidator validator = context.GetOrderByQueryValidator();
        ExceptionAssert.Throws<ODataException>(() =>
            validator.Validate(option, settings),
            "Order by 'Value' is not allowed. To allow it, set the 'AllowedOrderByProperties' property on EnableQueryAttribute or QueryValidationSettings.");
    }

    [Fact]
    public void ValidateOrderByQueryValidator_NoException_ForParameterAlias()
    {
        // Arrange
        IEdmModel model = GetEdmModel();
        IEdmEntityType edmType = model.SchemaElements.OfType<IEdmEntityType>().Single(t => t.Name == "LimitedEntity");
        IEdmEntitySet entitySet = model.FindDeclaredEntitySet("LimitedEntities");
        Assert.NotNull(entitySet);
        ODataQueryContext context = new ODataQueryContext(model, edmType);
        context.DefaultQueryConfigurations.EnableOrderBy = true;

        OrderByQueryOption option = new OrderByQueryOption(
            "@p,@q desc",
            context,
            new ODataQueryOptionParser(
                model,
                edmType,
                entitySet,
                new Dictionary<string, string> { { "$orderby", "@p,@q desc" }, { "@p", "Id" }, { "@q", "RelatedEntity/Id" } }));

        ODataValidationSettings settings = new ODataValidationSettings();

        // Act & Assert
        ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, settings));
    }

    private static IEdmModel GetEdmModel()
    {
        ODataModelBuilder builder = new ODataModelBuilder();

        // Configure LimitedEntity
        EntitySetConfiguration<LimitedEntity> limitedEntities = builder.EntitySet<LimitedEntity>("LimitedEntities");
        limitedEntities.EntityType.HasKey(p => p.Id);
        limitedEntities.EntityType.ComplexProperty(c => c.ComplexProperty).IsNotSortable();
        limitedEntities.EntityType.HasOptional(l => l.RelatedEntity);
        limitedEntities.EntityType.CollectionProperty(cp => cp.Integers);

        // Configure LimitedRelatedEntity
        EntitySetConfiguration<LimitedRelatedEntity> limitedRelatedEntities =
            builder.EntitySet<LimitedRelatedEntity>("LimitedRelatedEntities");
        limitedRelatedEntities.EntityType.HasKey(p => p.Id);
        limitedRelatedEntities.EntityType.HasOptional(p => p.BackReference).IsNotSortable();
        limitedRelatedEntities.EntityType.ComplexProperty(p => p.RelatedComplexProperty).IsNotSortable();

        // Configure SpecializedEntity
        EntityTypeConfiguration<LimitedSpecializedEntity> specializedEntity =
            builder.EntityType<LimitedSpecializedEntity>().DerivesFrom<LimitedRelatedEntity>();
        specializedEntity.Namespace = "NS";
        specializedEntity.ComplexProperty(p => p.SpecializedComplexProperty).IsNotSortable();

        // Configure Complextype
        ComplexTypeConfiguration<LimitedComplexType> complexType = builder.ComplexType<LimitedComplexType>();
        complexType.Property(p => p.NotSortableValue).IsNotSortable();
        complexType.Property(p => p.UnsortableValue).IsUnsortable();
        complexType.Property(p => p.Value);

        return builder.GetEdmModel();
    }

    private class LimitedEntity
    {
        public int Id { get; set; }
        public LimitedComplexType ComplexProperty { get; set; }
        public LimitedRelatedEntity RelatedEntity { get; set; }
        public ICollection<int> Integers { get; set; }
    }

    private class LimitedComplexType
    {
        public int Value { get; set; }
        public int NotSortableValue { get; set; }
        public int UnsortableValue { get; set; }
    }

    private class LimitedRelatedEntity
    {
        public int Id { get; set; }
        public LimitedEntity BackReference { get; set; }
        public LimitedComplexType RelatedComplexProperty { get; set; }
    }

    private class LimitedSpecializedEntity : LimitedRelatedEntity
    {
        public string Name { get; set; }
        public LimitedComplexType SpecializedComplexProperty { get; set; }
    }
}
