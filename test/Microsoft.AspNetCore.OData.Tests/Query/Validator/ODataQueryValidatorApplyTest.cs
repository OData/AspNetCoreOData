//-----------------------------------------------------------------------------
// <copyright file="ODataQueryValidatorApplyTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.ModelBuilder.Config;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Validator;

/// <summary>
/// Tests asserting that per-clause property restrictions are enforced consistently for the
/// <c>$apply</c> and top-level <c>$compute</c> query options, matching the enforcement that
/// already exists for <c>$filter</c> (not-filterable properties) and <c>$select</c>
/// (not-selectable properties).
/// A property a service marks as not filterable or configures as not selectable is expected to
/// be rejected with an <see cref="ODataException"/> when it is referenced through
/// <c>$apply=filter(...)</c>, <c>$apply=groupby((...))</c>, <c>$apply=aggregate(... with ...)</c>,
/// <c>$apply=compute(...)</c> or top-level <c>$compute=...</c>, exactly as it is when referenced
/// through the equivalent <c>$filter</c> or <c>$select</c>.
/// </summary>
public class ODataQueryValidatorApplyTest
{
    // ---------------------------------------------------------------------
    // Not-filterable properties referenced through $apply / $compute.
    // These describe the expected (consistent) behavior; they are rejected
    // through $filter today and are expected to be rejected the same way here.
    // ---------------------------------------------------------------------

    [Theory]
    [InlineData("$apply=filter(NotFilterableProperty eq 'restricted')")]
    [InlineData("$apply=filter(NonFilterableProperty eq 'restricted')")]
    public void Validate_Throws_ForNotFilterablePropertyInApplyFilter(string queryString)
    {
        // Arrange
        var options = CreateCustomerQueryOptions(queryString);

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() => options.Validate(new ODataValidationSettings()));
    }

    [Theory]
    [InlineData("$apply=filter(NotFilterableNavigationProperty/Name eq 'restricted')")]
    [InlineData("$apply=filter(NonFilterableNavigationProperty/Name eq 'restricted')")]
    public void Validate_Throws_ForNotFilterableNavigationPropertyInApplyFilter(string queryString)
    {
        // Arrange
        var options = CreateCustomerQueryOptions(queryString);

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() => options.Validate(new ODataValidationSettings()));
    }

    [Fact]
    public void Validate_Throws_ForNotFilterablePropertyInApplyAggregate()
    {
        // Arrange
        var options = CreateCustomerQueryOptions("$apply=aggregate(NotFilterableProperty with countdistinct as RestrictedCount)");

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() => options.Validate(new ODataValidationSettings()));
    }

    [Fact]
    public void Validate_Throws_ForNotFilterablePropertyInNestedGroupByAggregate()
    {
        // Arrange
        var options = CreateCustomerQueryOptions("$apply=groupby((Id),aggregate(NotFilterableProperty with countdistinct as RestrictedCount))");

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() => options.Validate(new ODataValidationSettings()));
    }

    [Fact]
    public void Validate_Throws_ForNotFilterablePropertyInApplyCompute()
    {
        // Arrange
        var options = CreateCustomerQueryOptions("$apply=compute(NotFilterableProperty eq 'restricted' as RestrictedFlag)");

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() => options.Validate(new ODataValidationSettings()));
    }

    [Fact]
    public void Validate_Throws_ForNotFilterablePropertyInTopLevelCompute()
    {
        // Arrange
        var options = CreateCustomerQueryOptions("$compute=NotFilterableProperty eq 'restricted' as RestrictedFlag");

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() => options.Validate(new ODataValidationSettings()));
    }

    // ---------------------------------------------------------------------
    // Allowed properties referenced through $apply / $compute must keep
    // validating without error. This proves the restriction is selective
    // (per-property) rather than rejecting every $apply / $compute.
    // ---------------------------------------------------------------------

    [Theory]
    [InlineData("$apply=filter(Name eq 'allowed')")]
    [InlineData("$apply=groupby((Name))")]
    [InlineData("$apply=aggregate(AmountSpent with sum as TotalAmount)")]
    [InlineData("$apply=compute(AmountSpent mul 2 as DoubleAmount)")]
    [InlineData("$compute=AmountSpent mul 2 as DoubleAmount")]
    public void Validate_DoesNotThrow_ForAllowedPropertyInApplyOrCompute(string queryString)
    {
        // Arrange
        var options = CreateCustomerQueryOptions(queryString);

        // Act & Assert
        ExceptionAssert.DoesNotThrow(() => options.Validate(new ODataValidationSettings()));
    }

    [Fact]
    public void Validate_DoesNotThrow_WhenNoApplyOrComputeIsPresent()
    {
        // Arrange - a request that carries neither $apply nor $compute.
        var options = CreateCustomerQueryOptions("$filter=Name eq 'allowed'");

        // Act & Assert
        ExceptionAssert.DoesNotThrow(() => options.Validate(new ODataValidationSettings()));
    }

    // ---------------------------------------------------------------------
    // Regression anchor: the not-filterable property is already rejected
    // through $filter today. The $apply / $compute tests above express the
    // same expectation for the aggregation pipeline.
    // ---------------------------------------------------------------------

    [Fact]
    public void Validate_Throws_ForNotFilterablePropertyInFilter()
    {
        // Arrange
        var options = CreateCustomerQueryOptions("$filter=NotFilterableProperty eq 'restricted'");

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() => options.Validate(new ODataValidationSettings()));
    }

    // ---------------------------------------------------------------------
    // Not-selectable properties referenced through $apply=groupby(...).
    // A property configured as not selectable is rejected through $select
    // today and is expected to be rejected the same way through groupby.
    // ---------------------------------------------------------------------

    [Fact]
    public void Validate_Throws_ForNotSelectablePropertyInApplyGroupBy()
    {
        // Arrange
        var options = CreateNotSelectableQueryOptions("$apply=groupby((RestrictedName))");

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() => options.Validate(new ODataValidationSettings()));
    }

    [Fact]
    public void Validate_Throws_ForNotSelectablePropertyInLaterTransformation()
    {
        // Arrange - the second transformation references the not-selectable property.
        var options = CreateNotSelectableQueryOptions("$apply=filter(AllowedName eq 'allowed')/groupby((RestrictedName))");

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() => options.Validate(new ODataValidationSettings()));
    }

    [Fact]
    public void Validate_DoesNotThrow_ForSelectablePropertyInApplyGroupBy()
    {
        // Arrange
        var options = CreateNotSelectableQueryOptions("$apply=groupby((AllowedName))");

        // Act & Assert
        ExceptionAssert.DoesNotThrow(() => options.Validate(new ODataValidationSettings()));
    }

    // ---------------------------------------------------------------------
    // Regression anchor: the not-selectable property is already rejected
    // through $select today, and the selectable property is accepted. This
    // confirms the model configuration used by the groupby tests above.
    // ---------------------------------------------------------------------

    [Fact]
    public void Validate_Throws_ForNotSelectablePropertyInSelect()
    {
        // Arrange
        var options = CreateNotSelectableQueryOptions("$select=RestrictedName");

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() => options.Validate(new ODataValidationSettings()));
    }

    [Fact]
    public void Validate_DoesNotThrow_ForSelectablePropertyInSelect()
    {
        // Arrange
        var options = CreateNotSelectableQueryOptions("$select=AllowedName");

        // Act & Assert
        ExceptionAssert.DoesNotThrow(() => options.Validate(new ODataValidationSettings()));
    }

    // ---------------------------------------------------------------------
    // Helpers.
    // ---------------------------------------------------------------------

    // Builds options over QueryCompositionCustomer, which carries the
    // [NotFilterable]/[NonFilterable] properties and allowed Name/AmountSpent.
    private static ODataQueryOptions CreateCustomerQueryOptions(string queryString)
    {
        // CreateCustomerContext(false) intentionally leaves RequestContainer unset; the
        // ODataQueryOptions constructor requires a null RequestContainer and fills it from the request.
        var context = ValidationTestHelper.CreateCustomerContext(false);
        context.DefaultQueryConfigurations.EnableFilter = true;
        context.DefaultQueryConfigurations.EnableSelect = true;
        context.DefaultQueryConfigurations.MaxTop = null;

        return CreateQueryOptions(context, queryString);
    }

    // Builds options over a model where RestrictedName is configured as not selectable.
    private static ODataQueryOptions CreateNotSelectableQueryOptions(string queryString)
    {
        return CreateQueryOptions(CreateNotSelectableContext(), queryString);
    }

    private static ODataQueryOptions CreateQueryOptions(ODataQueryContext context, string queryString)
    {
        var request = RequestFactory.Create("Get", "http://localhost/?" + queryString, setupAction: null);
        return new ODataQueryOptions(context, request);
    }

    // Configures RestrictedName as not selectable using model-bound query settings on the
    // entity type. Top-level $select / groupby restriction is read from the entity type's
    // SelectConfigurations (see EdmHelpers.IsNotSelectable), so the setting is attached there.
    private static ODataQueryContext CreateNotSelectableContext()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<ApplyValidationModel>("ApplyValidationModels");
        var model = builder.GetEdmModel();

        var entityType = model.SchemaElements.OfType<IEdmEntityType>().Single(t => t.Name == nameof(ApplyValidationModel));
        var settings = new ModelBoundQuerySettings();
        settings.SelectConfigurations.Add(nameof(ApplyValidationModel.RestrictedName), SelectExpandType.Disabled);
        model.SetAnnotationValue(entityType, settings);

        var context = new ODataQueryContext(model, typeof(ApplyValidationModel), null);
        context.DefaultQueryConfigurations.EnableSelect = true;
        context.DefaultQueryConfigurations.EnableFilter = true;
        context.DefaultQueryConfigurations.MaxTop = null;
        return context;
    }

    private class ApplyValidationModel
    {
        public int Id { get; set; }
        public string AllowedName { get; set; }
        public string RestrictedName { get; set; }
    }
}
