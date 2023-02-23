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
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Validator
{
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
    }
}
