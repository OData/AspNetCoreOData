//-----------------------------------------------------------------------------
// <copyright file="SelectExpandQueryValidatorTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.ModelBuilder.Annotations;
using Microsoft.OData.ModelBuilder.Config;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Validator;

public class SelectExpandQueryValidatorTest
{
    private ODataQueryContext _queryContext;

    public const string MaxExpandDepthExceededErrorString =
        "The request includes a $expand path which is too deep. The maximum depth allowed is {0}. " +
        "To increase the limit, set the 'MaxExpansionDepth' property on EnableQueryAttribute or ODataValidationSettings, or set the 'MaxDepth' property in ExpandAttribute.";

    public SelectExpandQueryValidatorTest()
    {
        CustomersModelWithInheritance model = new CustomersModelWithInheritance();
        model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));
        _queryContext = new ODataQueryContext(model.Model, typeof(Customer), null);
        _queryContext.RequestContainer = new MockServiceProvider();
        _queryContext.DefaultQueryConfigurations.EnableExpand = true;
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_Throws_NullOption()
    {
        // Arrange & Act & Assert
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        ExceptionAssert.ThrowsArgumentNull(() => validator.Validate(null, new ODataValidationSettings()), "selectExpandQueryOption");
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_Throws_NullSettings()
    {
        // Arrange
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
        SelectExpandQueryOption option = new SelectExpandQueryOption("any", null, _queryContext);

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => validator.Validate(option, null), "validationSettings");
    }

    [Theory]
    [InlineData("Orders($expand=Customer)", 1)]
    [InlineData("Orders,Orders($expand=Customer)", 1)]
    [InlineData("Orders($expand=Customer($expand=Orders))", 2)]
    [InlineData("Orders($expand=Customer($expand=Orders($expand=Customer($expand=Orders($expand=Customer)))))", 5)]
    [InlineData("Orders($expand=NS.SpecialOrder/SpecialCustomer)", 1)]
    public void ValidateSelectExpandQueryValidator_DepthChecks(string expand, int maxExpansionDepth)
    {
        // Arrange
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(null, expand, _queryContext);
        selectExpandQueryOption.LevelsMaxLiteralExpansionDepth = 1;

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings { MaxExpansionDepth = maxExpansionDepth }),
            String.Format(CultureInfo.CurrentCulture, MaxExpandDepthExceededErrorString, maxExpansionDepth));

        ExceptionAssert.DoesNotThrow(
            () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings { MaxExpansionDepth = maxExpansionDepth + 1 }));
    }

    [Theory]
    [InlineData("Orders($expand=Customer)", 1)]
    [InlineData("Orders,Orders($expand=Customer)", 1)]
    [InlineData("Orders($expand=Customer($expand=Orders))", 2)]
    [InlineData("Orders($expand=Customer($expand=Orders($expand=Customer($expand=Orders($expand=Customer)))))", 5)]
    [InlineData("Orders($expand=NS.SpecialOrder/SpecialCustomer)", 1)]
    public void ValidateSelectExpandQueryValidator_DepthChecks_QuerySettings(string expand, int maxExpansionDepth)
    {
        // Arrange
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        CustomersModelWithInheritance model = new CustomersModelWithInheritance();
        model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));
        ODataQueryContext queryContext = new ODataQueryContext(model.Model, typeof(Customer));
        queryContext.DefaultQueryConfigurations.EnableExpand = true;
        queryContext.RequestContainer = new MockServiceProvider();
        SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(null, expand, queryContext);
        selectExpandQueryOption.LevelsMaxLiteralExpansionDepth = 1;
        IEdmStructuredType customerType =
            model.Model.SchemaElements.First(e => e.Name.Equals("Customer")) as IEdmStructuredType;
        ModelBoundQuerySettings querySettings = new ModelBoundQuerySettings();
        querySettings.ExpandConfigurations.Add("Orders", new ExpandConfiguration
        {
            ExpandType = SelectExpandType.Allowed,
            MaxDepth = maxExpansionDepth
        });
        model.Model.SetAnnotationValue(customerType, querySettings);

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings { MaxExpansionDepth = maxExpansionDepth + 1 }),
            String.Format(CultureInfo.CurrentCulture, MaxExpandDepthExceededErrorString, maxExpansionDepth));
    }

    [Theory]
    [InlineData("Parent($levels=5)", 4)]
    [InlineData("Parent($expand=Parent($levels=4))", 4)]
    [InlineData("Parent($expand=Parent($expand=Parent($levels=0)))", 1)]
    [InlineData("Parent($expand=Parent($levels=4);$levels=5)", 8)]
    [InlineData("Parent($levels=4),DerivedAncestors($levels=5)", 4)]
    [InlineData("DerivedAncestors($levels=5),Parent($levels=4)", 4)]
    public void ValidateSelectExpandQueryValidator_DepthChecks_DollarLevels(string expand, int maxExpansionDepth)
    {
        // Arrange
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<ODataLevelsTest.LevelsEntity>("Entities");
        IEdmModel model = builder.GetEdmModel();
        var context = new ODataQueryContext(model, typeof(ODataLevelsTest.LevelsEntity));
        context.DefaultQueryConfigurations.EnableExpand = true;
        context.RequestContainer = new MockServiceProvider();
        var selectExpandQueryOption = new SelectExpandQueryOption(null, expand, context);
        selectExpandQueryOption.LevelsMaxLiteralExpansionDepth = 1;

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => validator.Validate(
                selectExpandQueryOption,
                new ODataValidationSettings { MaxExpansionDepth = maxExpansionDepth }),
            string.Format(
                CultureInfo.CurrentCulture,
                MaxExpandDepthExceededErrorString,
                maxExpansionDepth));

        ExceptionAssert.DoesNotThrow(
            () => validator.Validate(
                selectExpandQueryOption,
                new ODataValidationSettings { MaxExpansionDepth = maxExpansionDepth + 1 }));
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_DoesNotThrow_IfExpansionDepthIsZero_DollarLevels()
    {
        // Arrange
        string expand = "Parent($expand=Parent($expand=Parent($levels=10)))";
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<ODataLevelsTest.LevelsEntity>("Entities");
        IEdmModel model = builder.GetEdmModel();
        var context = new ODataQueryContext(model, typeof(ODataLevelsTest.LevelsEntity));
        context.DefaultQueryConfigurations.EnableExpand = true;
        context.RequestContainer = new MockServiceProvider();
        var selectExpandQueryOption = new SelectExpandQueryOption(null, expand, context);

        // Act & Assert
        ExceptionAssert.DoesNotThrow(
            () => validator.Validate(
                selectExpandQueryOption,
                new ODataValidationSettings { MaxExpansionDepth = 0 }));
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_Throws_LevelsMaxLiteralExpansionDepthGreaterThanMaxExpansionDepth()
    {
        // Arrange
        string expand = "Parent($levels=2)";
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<ODataLevelsTest.LevelsEntity>("Entities");
        IEdmModel model = builder.GetEdmModel();
        var context = new ODataQueryContext(model, typeof(ODataLevelsTest.LevelsEntity));
        context.DefaultQueryConfigurations.EnableExpand = true;
        context.RequestContainer = new MockServiceProvider();
        var selectExpandQueryOption = new SelectExpandQueryOption(null, expand, context);
        selectExpandQueryOption.LevelsMaxLiteralExpansionDepth = 4;

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => validator.Validate(
                selectExpandQueryOption,
                new ODataValidationSettings { MaxExpansionDepth = 3 }),
            "'LevelsMaxLiteralExpansionDepth' should be less than or equal to 'MaxExpansionDepth'.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void ValidateSelectExpandQueryValidator_DoesNotThrow_DefaultLevelsMaxLiteralExpansionDepth(int maxExpansionDepth)
    {
        // Arrange
        string expand = "Parent($levels=1)";
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<ODataLevelsTest.LevelsEntity>("Entities");
        IEdmModel model = builder.GetEdmModel();
        var context = new ODataQueryContext(model, typeof(ODataLevelsTest.LevelsEntity));
        context.DefaultQueryConfigurations.EnableExpand = true;
        context.RequestContainer = new MockServiceProvider();
        var selectExpandQueryOption = new SelectExpandQueryOption(null, expand, context);

        // Act & Assert
        ExceptionAssert.DoesNotThrow(
            () => validator.Validate(
                selectExpandQueryOption,
                new ODataValidationSettings { MaxExpansionDepth = maxExpansionDepth }));
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_Throw_WithInvalidMaxExpansionDepth()
    {
        int maxExpansionDepth = -1;
        // Arrange
        string expand = "Parent($levels=1)";
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<ODataLevelsTest.LevelsEntity>("Entities");
        IEdmModel model = builder.GetEdmModel();
        var context = new ODataQueryContext(model, typeof(ODataLevelsTest.LevelsEntity));
        context.RequestContainer = new MockServiceProvider();
        var validator = context.GetSelectExpandQueryValidator();
        var selectExpandQueryOption = new SelectExpandQueryOption(null, expand, context);

        // Act & Assert
        ExceptionAssert.Throws<ArgumentOutOfRangeException>(
            () => validator.Validate(
                selectExpandQueryOption,
                new ODataValidationSettings { MaxExpansionDepth = maxExpansionDepth }),
            "Value must be greater than or equal to 0. (Parameter 'value')\r\nActual value was -1.");
    }

    [Theory]
    [InlineData(2, 3)]
    [InlineData(4, 4)]
    [InlineData(3, 0)]
    public void ValidateSelectExpandQueryValidator_DoesNotThrow_LevelsMaxLiteralExpansionDepthAndMaxExpansionDepth(
        int levelsMaxLiteralExpansionDepth,
        int maxExpansionDepth)
    {
        // Arrange
        string expand = "Parent($levels=2)";
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<ODataLevelsTest.LevelsEntity>("Entities");
        IEdmModel model = builder.GetEdmModel();
        var context = new ODataQueryContext(model, typeof(ODataLevelsTest.LevelsEntity));
        context.DefaultQueryConfigurations.EnableExpand = true;
        context.RequestContainer = new MockServiceProvider();
        var selectExpandQueryOption = new SelectExpandQueryOption(null, expand, context);
        selectExpandQueryOption.LevelsMaxLiteralExpansionDepth = levelsMaxLiteralExpansionDepth;

        // Act & Assert
        ExceptionAssert.DoesNotThrow(
            () => validator.Validate(
                selectExpandQueryOption,
                new ODataValidationSettings { MaxExpansionDepth = maxExpansionDepth }));
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_Throws_IfNotAllowTop()
    {
        // Arrange
        string expand = "Orders($top=4)";
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        _queryContext.DefaultQueryConfigurations.EnableExpand = true;
        _queryContext.DefaultQueryConfigurations.MaxTop = 2;
        SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(null, expand, _queryContext);

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings()),
            "The limit of '2' for Top query has been exceeded. The value from the incoming request is '4'.");
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_Throws_IfNotAllowCount()
    {
        // Arrange
        string expand = "Orders($count=true)";
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        _queryContext.DefaultQueryConfigurations.EnableExpand = true;
        _queryContext.DefaultQueryConfigurations.EnableCount = false;
        SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(null, expand, _queryContext);

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings()),
            "The property 'Orders' cannot be used for $count.");
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_Throws_IfNotAllowOrderby()
    {
        // Arrange
        string expand = "Orders($orderby=Amount)";
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        _queryContext.DefaultQueryConfigurations.EnableExpand = true;
        _queryContext.DefaultQueryConfigurations.EnableOrderBy = false;
        SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(null, expand, _queryContext);

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings()),
            "The property 'Amount' cannot be used in the $orderby query option.");
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_Throws_IfNotAllowFilter()
    {
        // Arrange
        string expand = "Orders($filter=Amount eq 42)";
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        _queryContext.DefaultQueryConfigurations.EnableExpand = true;
        _queryContext.DefaultQueryConfigurations.EnableFilter = false;
        SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(null, expand, _queryContext);

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings()),
            "The property 'Amount' cannot be used in the $filter query option.");
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_DoesNotThrow_IfExpansionDepthIsZero()
    {
        // Arrange
        string expand = "Orders($expand=Customer($expand=Orders($expand=Customer($expand=Orders($expand=Customer)))))";
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        _queryContext.DefaultQueryConfigurations.EnableExpand = true;
        SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(null, expand, _queryContext);

        // Act & Assert
        ExceptionAssert.DoesNotThrow(
            () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings { MaxExpansionDepth = 0 }));
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_DoesNotThrow_IfExpansionDepthIsZero_QuerySettings()
    {
        // Arrange
        string expand =
            "Orders($expand=Customer($expand=Orders($expand=Customer($expand=Orders($expand=Customer)))))";
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        CustomersModelWithInheritance model = new CustomersModelWithInheritance();
        model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));
        ODataQueryContext queryContext = new ODataQueryContext(model.Model, typeof(Customer));
        queryContext.DefaultQueryConfigurations.EnableExpand = true;
        queryContext.RequestContainer = new MockServiceProvider();
        SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(null, expand, queryContext);
        IEdmStructuredType customerType =
            model.Model.SchemaElements.First(e => e.Name.Equals("Customer")) as IEdmStructuredType;
        ModelBoundQuerySettings querySettings = new ModelBoundQuerySettings();
        querySettings.ExpandConfigurations.Add("Orders", new ExpandConfiguration
        {
            ExpandType = SelectExpandType.Allowed,
            MaxDepth = 0
        });
        model.Model.SetAnnotationValue(customerType, querySettings);

        // Act & Assert
        ExceptionAssert.DoesNotThrow(
            () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings { MaxExpansionDepth = 0 }));
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_ThrowException_IfNotNavigable()
    {
        CustomersModelWithInheritance model = new CustomersModelWithInheritance();
        model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));
        ODataQueryContext queryContext = new ODataQueryContext(model.Model, typeof(Customer));
        queryContext.RequestContainer = new MockServiceProvider();
        model.Model.SetAnnotationValue(
            model.Customer.FindProperty("Orders"),
            new QueryableRestrictionsAnnotation(new QueryableRestrictions { NotNavigable = true }));

        string select = "Orders";
        ISelectExpandQueryValidator validator = queryContext.GetSelectExpandQueryValidator();
        SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(select, null, queryContext);
        ExceptionAssert.Throws<ODataException>(
            () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings()),
            "The property 'Orders' cannot be used for navigation.");
    }

    [Theory]
    [InlineData("Customer", "Orders")]
    [InlineData("SpecialCustomer", "SpecialOrders")]
    public void ValidateSelectExpandQueryValidator_ThrowException_IfBaseOrDerivedClassPropertyNotNavigable(string className, string propertyName)
    {
        CustomersModelWithInheritance model = new CustomersModelWithInheritance();
        model.Model.SetAnnotationValue(model.SpecialCustomer, new ClrTypeAnnotation(typeof(Customer)));
        ODataQueryContext queryContext = new ODataQueryContext(model.Model, typeof(Customer));
        queryContext.RequestContainer = new MockServiceProvider();
        EdmEntityType classType = (className == "Customer") ? model.Customer : model.SpecialCustomer;
        model.Model.SetAnnotationValue(classType.FindProperty(propertyName), new QueryableRestrictionsAnnotation(new QueryableRestrictions { NotNavigable = true }));

        string select = "NS.SpecialCustomer/" + propertyName;
        ISelectExpandQueryValidator validator = queryContext.GetSelectExpandQueryValidator();
        SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(select, null, queryContext);
        ExceptionAssert.Throws<ODataException>(
            () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings()),
            String.Format(CultureInfo.InvariantCulture, "The property '{0}' cannot be used for navigation.", propertyName));
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_ThrowException_IfNotExpandable()
    {
        CustomersModelWithInheritance model = new CustomersModelWithInheritance();
        model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));
        ODataQueryContext queryContext = new ODataQueryContext(model.Model, typeof(Customer));
        queryContext.RequestContainer = new MockServiceProvider();
        model.Model.SetAnnotationValue(model.Customer.FindProperty("Orders"), new QueryableRestrictionsAnnotation(new QueryableRestrictions { NotExpandable = true }));

        string expand = "Orders";
        ISelectExpandQueryValidator validator = queryContext.GetSelectExpandQueryValidator();
        SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(null, expand, queryContext);
        ExceptionAssert.Throws<ODataException>(
            () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings()),
            "The property 'Orders' cannot be used in the $expand query option.");
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_ThrowException_IfNotExpandable_QuerySettings()
    {
        // Arrange
        CustomersModelWithInheritance model = new CustomersModelWithInheritance();
        model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));
        ODataQueryContext queryContext = new ODataQueryContext(model.Model, typeof(Customer));
        queryContext.RequestContainer = new MockServiceProvider();
        ISelectExpandQueryValidator validator = queryContext.GetSelectExpandQueryValidator();
        SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(null, "Orders", queryContext);
        IEdmStructuredType customerType =
            model.Model.SchemaElements.First(e => e.Name.Equals("Customer")) as IEdmStructuredType;
        ModelBoundQuerySettings querySettings = new ModelBoundQuerySettings();
        querySettings.ExpandConfigurations.Add("Orders", new ExpandConfiguration
        {
            ExpandType = SelectExpandType.Disabled,
            MaxDepth = 0
        });
        model.Model.SetAnnotationValue(customerType, querySettings);

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings()),
            "The property 'Orders' cannot be used in the $expand query option.");
    }

    [Theory]
    [InlineData("Customer", "Orders")]
    [InlineData("SpecialCustomer", "SpecialOrders")]
    public void ValidateSelectExpandQueryValidator_ThrowException_IfBaseOrDerivedClassPropertyNotExpandable(string className, string propertyName)
    {
        CustomersModelWithInheritance model = new CustomersModelWithInheritance();
        model.Model.SetAnnotationValue(model.SpecialCustomer, new ClrTypeAnnotation(typeof(Customer)));
        ODataQueryContext queryContext = new ODataQueryContext(model.Model, typeof(Customer));
        queryContext.RequestContainer = new MockServiceProvider();
        EdmEntityType classType = (className == "Customer") ? model.Customer : model.SpecialCustomer;
        model.Model.SetAnnotationValue(classType.FindProperty(propertyName), new QueryableRestrictionsAnnotation(new QueryableRestrictions { NotExpandable = true }));

        string expand = "NS.SpecialCustomer/" + propertyName;
        ISelectExpandQueryValidator validator = queryContext.GetSelectExpandQueryValidator();
        SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(null, expand, queryContext);
        ExceptionAssert.Throws<ODataException>(
            () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings()),
            string.Format(CultureInfo.InvariantCulture, "The property '{0}' cannot be used in the $expand query option.", propertyName));
    }

    [Fact]
    public void GetSelectExpandQueryValidator_Returns_Validator()
    {
        // Arrange & Act & Assert
        ODataQueryContext context = null;
        Assert.NotNull(context.GetSelectExpandQueryValidator());

        // Arrange & Act & Assert
        context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
        Assert.NotNull(context.GetSelectExpandQueryValidator());

        // Arrange & Act & Assert
        IServiceProvider services = new ServiceCollection()
            .AddSingleton<ISelectExpandQueryValidator, SelectExpandQueryValidator>()
            .AddSingleton<DefaultQueryConfigurations>().BuildServiceProvider();
        context.RequestContainer = services;
        Assert.NotNull(context.GetSelectExpandQueryValidator());
    }

    [Theory]
    [InlineData("P1,P2,P3", null, 2)]                // 3 selected properties, limit 2
    [InlineData(null, "Child,Other", 1)]             // 2 expanded properties, limit 1
    [InlineData("P1,P2", "Child", 2)]                // 2 selected + 1 expanded = 3, limit 2
    [InlineData(null, "Child($select=P1,P2)", 2)]    // 1 expanded + 2 nested selected = 3, limit 2
    [InlineData(null, "Child($expand=Other)", 1)]    // 1 expanded + 1 nested expanded = 2, limit 1
    public void ValidateSelectExpandQueryValidator_Throws_WhenItemCountExceedsMaxSelectExpandNodeCount(string select, string expand, int maxCount)
    {
        // Arrange
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(select, expand, GetItemCountQueryContext());
        ODataValidationSettings settings = new ODataValidationSettings { MaxSelectExpandNodeCount = maxCount, MaxExpansionDepth = 0 };

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => validator.Validate(selectExpandQueryOption, settings),
            String.Format(CultureInfo.CurrentCulture, SRResources.MaxSelectExpandNodeCountExceeded, maxCount, "MaxSelectExpandNodeCount"));
    }

    [Theory]
    [InlineData("P1,P2,P3", null, 3)]                // exactly at the limit
    [InlineData("P1,P2", null, 3)]                   // below the limit
    [InlineData("P1", null, 1)]                      // single selected item at the limit
    [InlineData(null, "Child", 1)]                   // single expanded item at the limit
    [InlineData(null, "Child($select=P1,P2)", 3)]    // 1 expanded + 2 nested selected = 3, at the limit
    public void ValidateSelectExpandQueryValidator_DoesNotThrow_WhenItemCountWithinMaxSelectExpandNodeCount(string select, string expand, int maxCount)
    {
        // Arrange
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(select, expand, GetItemCountQueryContext());
        ODataValidationSettings settings = new ODataValidationSettings { MaxSelectExpandNodeCount = maxCount, MaxExpansionDepth = 0 };

        // Act & Assert
        ExceptionAssert.DoesNotThrow(() => validator.Validate(selectExpandQueryOption, settings));
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_MaxSelectExpandNodeCount_EnforcesExactBoundary()
    {
        // Arrange
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        string select = "P1,P2,P3,P4"; // 4 selected items

        // Act & Assert: the same request passes when the limit equals the item count ...
        ExceptionAssert.DoesNotThrow(() => validator.Validate(
            new SelectExpandQueryOption(select, null, GetItemCountQueryContext()),
            new ODataValidationSettings { MaxSelectExpandNodeCount = 4, MaxExpansionDepth = 0 }));

        // ... and is rejected when the limit is one below the item count.
        ExceptionAssert.Throws<ODataException>(
            () => validator.Validate(
                new SelectExpandQueryOption(select, null, GetItemCountQueryContext()),
                new ODataValidationSettings { MaxSelectExpandNodeCount = 3, MaxExpansionDepth = 0 }),
            String.Format(CultureInfo.CurrentCulture, SRResources.MaxSelectExpandNodeCountExceeded, 3, "MaxSelectExpandNodeCount"));
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_EnforcesDefaultMaxSelectExpandNodeCount()
    {
        // Arrange
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();

        // Build a nested $expand tree where each of the N levels contributes one expanded navigation plus
        // four selected properties (5 items per level). N = 101 levels yields 505 items, one band above the
        // default limit of 500; N = 100 levels yields exactly 500. Moderate nesting is used so the item count
        // is reached without an unusually deep tree. MaxExpansionDepth is disabled so only the count applies.
        string overLimitExpand = BuildNestedExpand(101); // 101 * 5 = 505 items
        string atLimitExpand = BuildNestedExpand(100);   // 100 * 5 = 500 items

        // Act & Assert: the default settings (MaxSelectExpandNodeCount = 500) reject the over-limit request
        // without any explicit configuration.
        ExceptionAssert.Throws<ODataException>(
            () => validator.Validate(
                new SelectExpandQueryOption(null, overLimitExpand, GetItemCountQueryContext()),
                new ODataValidationSettings { MaxExpansionDepth = 0 }),
            String.Format(CultureInfo.CurrentCulture, SRResources.MaxSelectExpandNodeCountExceeded, 500, "MaxSelectExpandNodeCount"));

        // A tree with exactly 500 items is allowed under the default limit.
        ExceptionAssert.DoesNotThrow(() => validator.Validate(
            new SelectExpandQueryOption(null, atLimitExpand, GetItemCountQueryContext()),
            new ODataValidationSettings { MaxExpansionDepth = 0 }));
    }

    // Builds a $expand value nested 'depth' levels deep. Each level expands Child and selects P1-P4, so a tree
    // of 'depth' levels carries depth expanded navigations and 4 * depth selected properties (5 items per level).
    private static string BuildNestedExpand(int depth)
    {
        return "Child(" + string.Concat(Enumerable.Repeat("$select=P1,P2,P3,P4;$expand=Child(", depth - 1))
            + "$select=P1,P2,P3,P4" + new string(')', depth);
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_TreatsZeroMaxSelectExpandNodeCountAsDisabled()
    {
        // Arrange
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        string select = "P1,P2"; // 2 items

        // Act & Assert: a limit of 0 disables the check (it does not reject the request) ...
        ExceptionAssert.DoesNotThrow(() => validator.Validate(
            new SelectExpandQueryOption(select, null, GetItemCountQueryContext()),
            new ODataValidationSettings { MaxSelectExpandNodeCount = 0, MaxExpansionDepth = 0 }));

        // ... while a positive limit below the item count rejects it.
        ExceptionAssert.Throws<ODataException>(
            () => validator.Validate(
                new SelectExpandQueryOption(select, null, GetItemCountQueryContext()),
                new ODataValidationSettings { MaxSelectExpandNodeCount = 1, MaxExpansionDepth = 0 }),
            String.Format(CultureInfo.CurrentCulture, SRResources.MaxSelectExpandNodeCountExceeded, 1, "MaxSelectExpandNodeCount"));
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_UsesGlobalMaxSelectExpandNodeCount_WhenSettingUnset()
    {
        // Arrange
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        ODataQueryContext context = GetItemCountQueryContext();
        context.DefaultQueryConfigurations.MaxSelectExpandNodeCount = 2; // global limit

        // Act & Assert: with the per-endpoint setting unset, the global DefaultQueryConfigurations limit (2)
        // applies, so a request with 3 items is rejected.
        ExceptionAssert.Throws<ODataException>(
            () => validator.Validate(
                new SelectExpandQueryOption("P1,P2,P3", null, context),
                new ODataValidationSettings { MaxExpansionDepth = 0 }),
            String.Format(CultureInfo.CurrentCulture, SRResources.MaxSelectExpandNodeCountExceeded, 2, "MaxSelectExpandNodeCount"));
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_EndpointMaxSelectExpandNodeCount_OverridesGlobal()
    {
        // Arrange
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        ODataQueryContext context = GetItemCountQueryContext();
        context.DefaultQueryConfigurations.MaxSelectExpandNodeCount = 2; // stricter global limit

        // Act & Assert: the per-endpoint setting (10) overrides the global limit (2), so 3 items are allowed.
        ExceptionAssert.DoesNotThrow(() => validator.Validate(
            new SelectExpandQueryOption("P1,P2,P3", null, context),
            new ODataValidationSettings { MaxSelectExpandNodeCount = 10, MaxExpansionDepth = 0 }));
    }

    // $levels expands into one copy of the subtree per level at bind time (ProcessLevels). These tests verify the
    // validator counts that multiplicity, so a $levels expansion is capped exactly like the equivalent explicit nesting.

    [Fact]
    public void ValidateSelectExpandQueryValidator_DollarLevels_CountedConsistentlyWithExplicitNesting()
    {
        // Arrange: two requests that materialize the same tree (5 levels x 5 items = 25). Depth is raised above
        // $levels so only the projection-item count can reject them.
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        ODataValidationSettings settings = new ODataValidationSettings { MaxSelectExpandNodeCount = 5, MaxExpansionDepth = 100 };

        // Explicit: the 5-level tree carries 25 items and is rejected.
        ExceptionAssert.Throws<ODataException>(
            () => validator.Validate(
                new SelectExpandQueryOption(null, BuildNestedExpand(5), GetItemCountQueryContext()),
                settings),
            String.Format(CultureInfo.CurrentCulture, SRResources.MaxSelectExpandNodeCountExceeded, 5, "MaxSelectExpandNodeCount"));

        // $levels: same 25 items, now counted with the multiplicity, so rejected against the identical limit.
        ExceptionAssert.Throws<ODataException>(
            () => validator.Validate(
                new SelectExpandQueryOption(null, "Child($select=P1,P2,P3,P4;$levels=5)", GetItemCountQueryContext()),
                settings),
            String.Format(CultureInfo.CurrentCulture, SRResources.MaxSelectExpandNodeCountExceeded, 5, "MaxSelectExpandNodeCount"));
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_DollarLevels_EnforcesDefaultMaxSelectExpandNodeCount()
    {
        // Arrange: the $levels form materializes 505 items (101 levels x 5), exceeding the default 500 limit.
        // Depth is raised so the depth check does not mask the projection-count behavior under test.
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        string levelsExpand = "Child($select=P1,P2,P3,P4;$levels=101)";

        // Act & Assert: 505 materialized items exceed the default limit of 500.
        ExceptionAssert.Throws<ODataException>(
            () => validator.Validate(
                new SelectExpandQueryOption(null, levelsExpand, GetItemCountQueryContext()),
                new ODataValidationSettings { MaxExpansionDepth = 1000 }),
            String.Format(CultureInfo.CurrentCulture, SRResources.MaxSelectExpandNodeCountExceeded, 500, "MaxSelectExpandNodeCount"));
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_DollarLevelsMax_CountedAgainstEffectiveExpansionDepth()
    {
        // Arrange: $levels=max resolves to the effective MaxExpansionDepth (5 here), so the tree materializes
        // 5 levels x 5 items = 25 projection items, which exceeds the limit of 20.
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        ODataValidationSettings settings = new ODataValidationSettings { MaxSelectExpandNodeCount = 20, MaxExpansionDepth = 5 };

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => validator.Validate(
                new SelectExpandQueryOption(null, "Child($select=P1,P2,P3,P4;$levels=max)", GetItemCountQueryContext()),
                settings),
            String.Format(CultureInfo.CurrentCulture, SRResources.MaxSelectExpandNodeCountExceeded, 20, "MaxSelectExpandNodeCount"));
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_DollarLevels_WithinLimit_DoesNotThrow()
    {
        // Arrange: the level multiplicity is counted, but the materialized tree (5 levels x 5 items = 25 items)
        // exactly meets the limit of 25, so the request is accepted. Guards against over-rejection at the boundary.
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        ODataValidationSettings settings = new ODataValidationSettings { MaxSelectExpandNodeCount = 25, MaxExpansionDepth = 100 };

        // Act & Assert
        ExceptionAssert.DoesNotThrow(
            () => validator.Validate(
                new SelectExpandQueryOption(null, "Child($select=P1,P2,P3,P4;$levels=5)", GetItemCountQueryContext()),
                settings));
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_DollarLevels_BeyondExpansionDepth_ReportsDepthNotProjectionCount()
    {
        // Arrange: $levels=100 exceeds MaxExpansionDepth=2. Counted levels are capped at the depth (2 x 5 = 10,
        // under the limit of 50), so the depth validator - not the projection count - reports the failure.
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        ODataValidationSettings settings = new ODataValidationSettings { MaxSelectExpandNodeCount = 50, MaxExpansionDepth = 2 };

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => validator.Validate(
                new SelectExpandQueryOption(null, "Child($select=P1,P2,P3,P4;$levels=100)", GetItemCountQueryContext()),
                settings),
            String.Format(CultureInfo.CurrentCulture, MaxExpandDepthExceededErrorString, 2));
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_DollarLevels_DoesNotWeighSiblingItems()
    {
        // Arrange: Child($levels=5) materializes 5 items x 5 levels = 25; the top-level $select=P1 is a sibling
        // counted once. If the level weight leaked to siblings, P1 would count as 5 and the total (30) would exceed
        // the limit of 26.
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        ODataValidationSettings settings = new ODataValidationSettings { MaxSelectExpandNodeCount = 26, MaxExpansionDepth = 100 };

        // Act & Assert: the sibling stays at weight 1, so the total is 26 and the request is accepted.
        ExceptionAssert.DoesNotThrow(
            () => validator.Validate(
                new SelectExpandQueryOption("P1", "Child($select=P1,P2,P3,P4;$levels=5)", GetItemCountQueryContext()),
                settings));
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_DollarLevels_LargeValuesAreRejectedWithoutOverflow()
    {
        // Arrange: a very large $levels within an equally large MaxExpansionDepth. The scaled weight is clamped to
        // just past the limit, so counting stays overflow-safe and the request is rejected on the projection count.
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        ODataValidationSettings settings = new ODataValidationSettings { MaxSelectExpandNodeCount = 10, MaxExpansionDepth = int.MaxValue };

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => validator.Validate(
                new SelectExpandQueryOption(null, "Child($select=P1;$levels=2000000000)", GetItemCountQueryContext()),
                settings),
            String.Format(CultureInfo.CurrentCulture, SRResources.MaxSelectExpandNodeCountExceeded, 10, "MaxSelectExpandNodeCount"));
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_DollarLevels_ProjectionCountDisabled_DoesNotThrow()
    {
        // Arrange: with the projection-item check disabled (non-positive limit), a $levels expansion is not counted
        // and must not trip the projection limit, even though it would exceed a positive limit. MaxExpansionDepth
        // still admits the depth.
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        ODataValidationSettings settings = new ODataValidationSettings { MaxSelectExpandNodeCount = 0, MaxExpansionDepth = 100 };

        // Act & Assert
        ExceptionAssert.DoesNotThrow(
            () => validator.Validate(
                new SelectExpandQueryOption(null, "Child($select=P1,P2,P3,P4;$levels=50)", GetItemCountQueryContext()),
                settings));
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_DollarLevelsOne_IsNotScaled()
    {
        // Arrange: $levels=1 expands a single level, so it is counted like an ordinary $expand (weight 1). The tree
        // carries 5 items (1 expand + 4 selects); a limit of 5 is met exactly and the request is accepted.
        SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
        ODataValidationSettings settings = new ODataValidationSettings { MaxSelectExpandNodeCount = 5, MaxExpansionDepth = 100 };

        // Act & Assert
        ExceptionAssert.DoesNotThrow(
            () => validator.Validate(
                new SelectExpandQueryOption(null, "Child($select=P1,P2,P3,P4;$levels=1)", GetItemCountQueryContext()),
                settings));
    }

    [Fact]
    public void ValidateSelectExpandQueryValidator_DollarLevels_ProducesNestedExpandsCountedByMaxSelectExpandNodeCount()
    {
        // Arrange: $levels is rewritten into explicit nested $expand copies at bind time (ProcessLevels), and
        // MaxSelectExpandNodeCount is measured against that produced projection. "Child($select=P1,P2,P3;$levels=3)"
        // expands into three copies:
        //   Child($select=P1,P2,P3;$expand=Child($select=P1,P2,P3;$expand=Child($select=P1,P2,P3)))
        // so the request materializes 3 x (1 expand + 3 selects) = 12 projection nodes.
        const string levelsExpand = "Child($select=P1,P2,P3;$levels=3)";
        SelectExpandQueryOption selectExpand = new SelectExpandQueryOption(null, levelsExpand, GetItemCountQueryContext())
        {
            LevelsMaxLiteralExpansionDepth = 3
        };

        // Act: expand $levels exactly as the binder does before building the projection.
        SelectExpandClause producedClause = selectExpand.ProcessLevels();

        // Assert: the produced clause is three Child levels deep, each repeating $select=P1,P2,P3, with $levels flattened away.
        SelectExpandClause clause = producedClause;
        for (int level = 1; level <= 3; level++)
        {
            ExpandedNavigationSelectItem child = Assert.Single(clause.SelectedItems.OfType<ExpandedNavigationSelectItem>());
            Assert.Equal("Child", ((NavigationPropertySegment)child.PathToNavigationProperty.FirstSegment).NavigationProperty.Name);
            Assert.Null(child.LevelsOption); // $levels has been rewritten into explicit nesting

            clause = child.SelectAndExpand;
            foreach (string property in new[] { "P1", "P2", "P3" })
            {
                Assert.Contains(
                    clause.SelectedItems.OfType<PathSelectItem>(),
                    item => item.SelectedPath.FirstSegment is PropertySegment segment && segment.Property.Name == property);
            }
        }

        // The innermost copy has no further Child expand.
        Assert.Empty(clause.SelectedItems.OfType<ExpandedNavigationSelectItem>());

        // The produced tree carries exactly 12 projection nodes (3 expands + 9 selected properties) ...
        Assert.Equal(12, CountProjectionNodes(producedClause));

        // ... and MaxSelectExpandNodeCount is enforced against that produced count: 12 is accepted, 11 is rejected.
        ExceptionAssert.DoesNotThrow(
            () => new SelectExpandQueryValidator().Validate(
                new SelectExpandQueryOption(null, levelsExpand, GetItemCountQueryContext()),
                new ODataValidationSettings { MaxSelectExpandNodeCount = 12, MaxExpansionDepth = 100 }));

        ExceptionAssert.Throws<ODataException>(
            () => new SelectExpandQueryValidator().Validate(
                new SelectExpandQueryOption(null, levelsExpand, GetItemCountQueryContext()),
                new ODataValidationSettings { MaxSelectExpandNodeCount = 11, MaxExpansionDepth = 100 }),
            String.Format(CultureInfo.CurrentCulture, SRResources.MaxSelectExpandNodeCountExceeded, 11, "MaxSelectExpandNodeCount"));
    }

    // Counts projection nodes in a produced (levels-expanded) clause the same way the validator does: each $expand
    // navigation and each selected structural property counts once. Navigation properties that the parser adds to
    // $select as a side effect of nesting are ignored so the count reflects the requested projection.
    private static int CountProjectionNodes(SelectExpandClause clause)
    {
        if (clause == null)
        {
            return 0;
        }

        int count = 0;
        foreach (SelectItem item in clause.SelectedItems)
        {
            if (item is ExpandedNavigationSelectItem expand)
            {
                count += 1 + CountProjectionNodes(expand.SelectAndExpand);
            }
            else if (item is PathSelectItem path && path.SelectedPath.FirstSegment is PropertySegment)
            {
                count++;
            }
        }

        return count;
    }

    private static ODataQueryContext GetItemCountQueryContext()
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<SelectExpandNodeCountEntity>("Entities");
        IEdmModel model = builder.GetEdmModel();
        ODataQueryContext context = new ODataQueryContext(model, typeof(SelectExpandNodeCountEntity));
        context.RequestContainer = new MockServiceProvider();
        context.DefaultQueryConfigurations.EnableExpand = true;
        context.DefaultQueryConfigurations.EnableSelect = true;
        return context;
    }

    public class SelectExpandNodeCountEntity
    {
        public int Id { get; set; }
        public string P1 { get; set; }
        public string P2 { get; set; }
        public string P3 { get; set; }
        public string P4 { get; set; }
        public SelectExpandNodeCountEntity Child { get; set; }
        public SelectExpandNodeCountEntity Other { get; set; }
    }
}
