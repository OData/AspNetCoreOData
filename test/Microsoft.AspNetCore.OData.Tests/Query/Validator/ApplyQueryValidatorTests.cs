//-----------------------------------------------------------------------------
// <copyright file="ApplyQueryValidatorTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.ModelBuilder.Config;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Validator;

/// <summary>
/// Unit tests for <see cref="ApplyQueryValidator"/>.
/// The <c>filter</c> transformation is validated with the same rules as <c>$filter</c> (full parity,
/// tied to the model's <see cref="DefaultQueryConfigurations.EnableFilter"/> switch). The
/// <c>groupby</c>, <c>aggregate</c> and <c>compute</c> transformations are selective: they reject only
/// properties that the model explicitly restricts (a not-filterable attribute or a not-selectable
/// model-bound configuration), so a property that is not explicitly restricted keeps validating
/// regardless of the global enable switches.
/// </summary>
public class ApplyQueryValidatorTests
{
    private readonly ApplyQueryValidator _validator = new ApplyQueryValidator();

    private const string NotFilterableMessage =
        "The property 'NotFilterableProperty' cannot be used in the $filter query option.";

    #region Argument validation

    [Fact]
    public void Validate_ThrowsArgumentNull_ForNullApplyQueryOption()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(
            () => _validator.Validate(null, new ODataValidationSettings()), "applyQueryOption");
    }

    [Fact]
    public void Validate_ThrowsArgumentNull_ForNullValidationSettings()
    {
        // Arrange
        var apply = new ApplyQueryOption("groupby((Name))", CreateContext());

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => _validator.Validate(apply, null), "validationSettings");
    }

    #endregion

    #region TryValidate reports errors instead of throwing

    [Fact]
    public void TryValidate_ReturnsValidationError_ForNullApplyQueryOption()
    {
        // Arrange & Act
        var result = _validator.TryValidate(null, new ODataValidationSettings(), out IEnumerable<string> validationErrors);

        // Assert
        Assert.False(result);
        Assert.Single(validationErrors);
        Assert.Equal("Value cannot be null. (Parameter 'applyQueryOption')", validationErrors.First());
    }

    [Fact]
    public void TryValidate_ReturnsValidationError_ForNullValidationSettings()
    {
        // Arrange
        var option = new ApplyQueryOption("groupby((Name))", CreateContext());

        // Act
        var result = _validator.TryValidate(option, null, out IEnumerable<string> validationErrors);

        // Assert
        Assert.False(result);
        Assert.Single(validationErrors);
        Assert.Equal("Value cannot be null. (Parameter 'validationSettings')", validationErrors.First());
    }

    [Fact]
    public void TryValidate_ReturnsFalseAndError_WithoutThrowing_ForRestrictedProperty()
    {
        // Arrange - a not-filterable property referenced through compute is rejected without throwing.
        var option = new ApplyQueryOption("compute(NotFilterableProperty eq 'x' as F)", CreateContext());

        // Act
        var result = _validator.TryValidate(option, new ODataValidationSettings(), out IEnumerable<string> validationErrors);

        // Assert
        Assert.False(result);
        Assert.Single(validationErrors);
        Assert.Equal(NotFilterableMessage, validationErrors.First());
    }

    [Fact]
    public void TryValidate_ReturnsTrueAndNoErrors_ForUnrestrictedApply()
    {
        // Arrange
        var option = new ApplyQueryOption("groupby((Name),aggregate(AmountSpent with sum as Total))", CreateContext());

        // Act
        var result = _validator.TryValidate(option, new ODataValidationSettings(), out IEnumerable<string> validationErrors);

        // Assert
        Assert.True(result);
        Assert.Empty(validationErrors);
    }

    #endregion

    #region groupby / aggregate / compute reject explicitly restricted (not-filterable) properties

    [Theory]
    [InlineData("groupby((NotFilterableProperty))")]
    [InlineData("groupby((NonFilterableProperty))")]
    [InlineData("groupby((Id,NotFilterableProperty))")]
    [InlineData("groupby((Address/NotFilterableProperty))")]
    public void Validate_Throws_ForRestrictedPropertyInGroupBy(string apply)
    {
        // Arrange
        var option = new ApplyQueryOption(apply, CreateContext());

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, new ODataValidationSettings()));
    }

    [Theory]
    [InlineData("aggregate(NotFilterableProperty with countdistinct as R)")]
    [InlineData("aggregate(NonFilterableProperty with countdistinct as R)")]
    [InlineData("groupby((Id),aggregate(NotFilterableProperty with countdistinct as R))")]
    public void Validate_Throws_ForRestrictedPropertyInAggregate(string apply)
    {
        // Arrange
        var option = new ApplyQueryOption(apply, CreateContext());

        // Act & Assert - both [NotFilterable] and [NonFilterable] properties are rejected.
        ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, new ODataValidationSettings()));
    }

    [Theory]
    [InlineData("compute(NotFilterableProperty eq 'x' as F)")]
    [InlineData("compute(length(NotFilterableProperty) as L)")]
    [InlineData("compute(Name eq 'a' or NotFilterableProperty eq 'b' as F)")]
    [InlineData("compute(Address/NotFilterableProperty eq 'x' as F)")]
    public void Validate_Throws_ForRestrictedPropertyInCompute(string apply)
    {
        // Arrange
        var option = new ApplyQueryOption(apply, CreateContext());

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(option, new ODataValidationSettings()), NotFilterableMessage);
    }

    [Fact]
    public void Validate_Throws_ForRestrictedPropertyInEntitySetAggregate()
    {
        // Arrange - aggregate over a navigation collection where the inner property is restricted.
        var option = new ApplyQueryOption("aggregate(Contacts(NotFilterableProperty with countdistinct as R))", CreateContext());

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(option, new ODataValidationSettings()), NotFilterableMessage);
    }

    [Fact]
    public void Validate_Throws_ForRestrictedPropertyInsideAnyLambdaBody()
    {
        // Arrange - the restricted property is referenced only inside the any(...) lambda body.
        var option = new ApplyQueryOption("compute(Contacts/any(c: c/NotFilterableProperty ne null) as HasRestricted)", CreateContext());

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(option, new ODataValidationSettings()), NotFilterableMessage);
    }

    [Fact]
    public void Validate_Throws_ForRestrictedPropertyInsideAllLambdaBody()
    {
        // Arrange - the restricted property is referenced only inside the all(...) lambda body; this
        // guards the all(...) body being walked the same way as the any(...) body.
        var option = new ApplyQueryOption("compute(Contacts/all(c: c/NotFilterableProperty ne null) as AllRestricted)", CreateContext());

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(option, new ODataValidationSettings()), NotFilterableMessage);
    }

    [Fact]
    public void Validate_Throws_ForRestrictedPropertyThroughSingleNavigation()
    {
        // Arrange - restricted property reached through a single-valued navigation.
        var option = new ApplyQueryOption("compute(RelationshipManager/NotFilterableProperty eq 'x' as F)", CreateContext());

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(option, new ODataValidationSettings()), NotFilterableMessage);
    }

    #endregion

    #region groupby / aggregate / compute are a no-op for unrestricted properties (default, unconfigured context)

    // The context leaves EnableFilter/EnableSelect at their framework defaults (false). These cases
    // prove the groupby/aggregate/compute walk is selective: an unrestricted property keeps validating
    // without error even though nothing enabled filter or select globally.
    [Theory]
    [InlineData("groupby((Name))")]
    [InlineData("groupby((Id))")]
    [InlineData("groupby((Name,Id))")]
    [InlineData("groupby((Address/City))")]
    [InlineData("groupby((Name),aggregate(AmountSpent with sum as Total))")]
    [InlineData("aggregate(AmountSpent with sum as Total)")]
    [InlineData("aggregate($count as Count)")]
    [InlineData("aggregate(Contacts(Name with countdistinct as R))")]
    [InlineData("compute(AmountSpent mul 2 as Double)")]
    [InlineData("compute(length(Name) as L)")]
    [InlineData("compute(Contacts/any(c: c/Name ne null) as HasNamed)")]
    [InlineData("compute(Contacts/all(c: c/Name ne null) as AllNamed)")]
    [InlineData("compute(RelationshipManager/Name eq 'x' as F)")]
    public void Validate_DoesNotThrow_ForUnrestrictedPropertyInGroupByAggregateCompute(string apply)
    {
        // Arrange
        var option = new ApplyQueryOption(apply, CreateContext());

        // Act & Assert
        ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, new ODataValidationSettings()));
    }

    #endregion

    #region filter transformation keeps full $filter parity

    [Fact]
    public void Validate_Throws_ForRestrictedPropertyInFilterTransformation()
    {
        // Arrange - filter is validated as $filter, so the restricted property is rejected.
        var option = new ApplyQueryOption("filter(NotFilterableProperty eq 'x')", CreateFilterEnabledContext());

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(option, new ODataValidationSettings()), NotFilterableMessage);
    }

    [Fact]
    public void Validate_DoesNotThrow_ForUnrestrictedPropertyInFilterTransformation_WhenFilterEnabled()
    {
        // Arrange - when the model enables filter, an unrestricted property validates through filter().
        var option = new ApplyQueryOption("filter(Name eq 'x')", CreateFilterEnabledContext());

        // Act & Assert
        ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, new ODataValidationSettings()));
    }

    [Fact]
    public void Validate_Throws_ForFilterTransformation_MirrorsDollarFilterWhenFilterNotEnabled()
    {
        // Arrange - the same unrestricted property inside filter() is rejected when the model does not
        // enable filter, exactly as $filter=Name would be. This documents that filter() follows $filter
        // parity while groupby((Name)) on the identical context is a no-op (see the no-op theory above).
        var context = CreateContext();
        var filterOption = new ApplyQueryOption("filter(Name eq 'x')", context);
        var groupByOption = new ApplyQueryOption("groupby((Name))", context);

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(filterOption, new ODataValidationSettings()),
            "The property 'Name' cannot be used in the $filter query option.");
        ExceptionAssert.DoesNotThrow(() => _validator.Validate(groupByOption, new ODataValidationSettings()));
    }

    #endregion

    #region Chained transformations

    [Fact]
    public void Validate_Throws_WhenRestrictedPropertyAppearsInLaterTransformation()
    {
        // Arrange - the first transformation is allowed; the restricted property is in the second.
        var option = new ApplyQueryOption("filter(Name eq 'x')/groupby((NotFilterableProperty))", CreateFilterEnabledContext());

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(option, new ODataValidationSettings()), NotFilterableMessage);
    }

    #endregion

    #region groupby / aggregate / compute enforce operator, function and node-count limits

    // These cases prove that the operator / function allow-lists and the node-count limit from
    // ODataValidationSettings are enforced for the groupby/aggregate/compute transformations, not just
    // for filter(). The referenced properties (Name/AmountSpent) are unrestricted, so only the
    // operator/function/complexity limit can be the cause of the rejection.

    [Fact]
    public void Validate_Throws_ForDisallowedFunctionInCompute()
    {
        // Arrange - length(...) is used but AllowedFunctions excludes Length.
        var option = new ApplyQueryOption("compute(length(Name) as L)", CreateContext());
        var settings = new ODataValidationSettings { AllowedFunctions = AllowedFunctions.AllFunctions & ~AllowedFunctions.Length };

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(option, settings),
            "Function 'length' is not allowed. To allow it, set the 'AllowedFunctions' property on EnableQueryAttribute or QueryValidationSettings.");
    }

    [Fact]
    public void Validate_Throws_ForDisallowedArithmeticOperatorInCompute()
    {
        // Arrange - 'mul' is used but AllowedArithmeticOperators excludes Multiply.
        var option = new ApplyQueryOption("compute(AmountSpent mul 2 as Double)", CreateContext());
        var settings = new ODataValidationSettings { AllowedArithmeticOperators = AllowedArithmeticOperators.All & ~AllowedArithmeticOperators.Multiply };

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(option, settings),
            "Arithmetic operator 'Multiply' is not allowed. To allow it, set the 'AllowedArithmeticOperators' property on EnableQueryAttribute or QueryValidationSettings.");
    }

    [Fact]
    public void Validate_Throws_ForDisallowedLogicalOperatorInCompute()
    {
        // Arrange - 'eq' is used but AllowedLogicalOperators excludes Equal.
        var option = new ApplyQueryOption("compute(Name eq 'x' as Flag)", CreateContext());
        var settings = new ODataValidationSettings { AllowedLogicalOperators = AllowedLogicalOperators.All & ~AllowedLogicalOperators.Equal };

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(option, settings),
            "Logical operator 'Equal' is not allowed. To allow it, set the 'AllowedLogicalOperators' property on EnableQueryAttribute or QueryValidationSettings.");
    }

    [Fact]
    public void Validate_Throws_ForDisallowedArithmeticOperatorInAggregate()
    {
        // Arrange - the aggregate expression itself contains 'mul', which is disallowed.
        var option = new ApplyQueryOption("aggregate(AmountSpent mul 2 with sum as Total)", CreateContext());
        var settings = new ODataValidationSettings { AllowedArithmeticOperators = AllowedArithmeticOperators.All & ~AllowedArithmeticOperators.Multiply };

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(option, settings),
            "Arithmetic operator 'Multiply' is not allowed. To allow it, set the 'AllowedArithmeticOperators' property on EnableQueryAttribute or QueryValidationSettings.");
    }

    [Fact]
    public void Validate_Throws_WhenNodeCountExceededInCompute()
    {
        // Arrange - a small MaxNodeCount is exceeded by the compute expression tree.
        var option = new ApplyQueryOption("compute(AmountSpent mul 2 as Double)", CreateContext());
        var settings = new ODataValidationSettings { MaxNodeCount = 1 };

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(option, settings),
            "The node count limit of '1' has been exceeded. To increase the limit, set the 'MaxNodeCount' property on EnableQueryAttribute or ODataValidationSettings.");
    }

    [Theory]
    [InlineData("compute(length(Name) as L)")]
    [InlineData("compute(AmountSpent mul 2 as Double)")]
    [InlineData("compute(Name eq 'x' as Flag)")]
    [InlineData("aggregate(AmountSpent mul 2 with sum as Total)")]
    public void Validate_DoesNotThrow_ForOperatorFunctionOrNodeCount_WhenLimitsPermit(string apply)
    {
        // Arrange - default settings allow every function/operator and set MaxNodeCount to 100, so the
        // same expressions that the restrictive settings above reject validate cleanly here. This guards
        // against the new enforcement producing false rejections under permissive limits.
        var option = new ApplyQueryOption(apply, CreateContext());

        // Act & Assert
        ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, new ODataValidationSettings()));
    }

    #endregion

    #region Not-selectable (model-bound) properties in groupby

    [Fact]
    public void Validate_Throws_ForNotSelectablePropertyInGroupBy()
    {
        // Arrange
        var option = new ApplyQueryOption("groupby((RestrictedName))", CreateNotSelectableContext());

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(option, new ODataValidationSettings()),
            "The property 'RestrictedName' cannot be used in the $select query option.");
    }

    [Fact]
    public void Validate_DoesNotThrow_ForSelectablePropertyInGroupBy()
    {
        // Arrange
        var option = new ApplyQueryOption("groupby((AllowedName))", CreateNotSelectableContext());

        // Act & Assert
        ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, new ODataValidationSettings()));
    }

    #endregion

    #region Virtual Validate can be overridden by a derived validator

    [Fact]
    public void Validate_IsInvoked_OnDerivedValidator_AndBaseEnforcementStillRuns()
    {
        // Arrange
        var validator = new TrackingApplyQueryValidator();
        var option = new ApplyQueryOption("groupby((NotFilterableProperty))", CreateContext());

        // Act & Assert - the override runs and the base enforcement is still applied.
        ExceptionAssert.Throws<ODataException>(() => validator.Validate(option, new ODataValidationSettings()));
        Assert.Equal(1, validator.ValidateCallCount);
    }

    [Fact]
    public void Validate_CanBeFullyReplaced_ByDerivedValidator()
    {
        // Arrange - a derived validator that does not call base replaces the enforcement entirely.
        var validator = new NoOpApplyQueryValidator();
        var option = new ApplyQueryOption("groupby((NotFilterableProperty))", CreateContext());

        // Act & Assert
        ExceptionAssert.DoesNotThrow(() => validator.Validate(option, new ODataValidationSettings()));
    }

    #endregion

    #region Helpers

    // Context over QueryCompositionCustomer, which carries the [NotFilterable]/[NonFilterable]
    // properties and unrestricted Name/Id/AmountSpent. EnableFilter/EnableSelect are left at their
    // framework defaults (false), so the no-op cases prove the walk does not depend on those switches.
    private static ODataQueryContext CreateContext()
    {
        return ValidationTestHelper.CreateCustomerContext();
    }

    // Same model, but with filtering enabled so the filter() parity cases exercise an enabled model.
    private static ODataQueryContext CreateFilterEnabledContext()
    {
        var context = ValidationTestHelper.CreateCustomerContext();
        context.DefaultQueryConfigurations.EnableFilter = true;
        context.DefaultQueryConfigurations.EnableSelect = true;
        return context;
    }

    // Context where RestrictedName is configured as not selectable via model-bound query settings.
    // EnableSelect is left false to show the model-bound configuration is honored on its own.
    private static ODataQueryContext CreateNotSelectableContext()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<ApplyRestrictionModel>("ApplyRestrictionModels");
        var model = builder.GetEdmModel();

        var entityType = model.SchemaElements.OfType<IEdmEntityType>().Single(t => t.Name == nameof(ApplyRestrictionModel));
        var settings = new ModelBoundQuerySettings();
        settings.SelectConfigurations.Add(nameof(ApplyRestrictionModel.RestrictedName), SelectExpandType.Disabled);
        model.SetAnnotationValue(entityType, settings);

        return new ODataQueryContext(model, typeof(ApplyRestrictionModel), null);
    }

    private class ApplyRestrictionModel
    {
        public int Id { get; set; }
        public string AllowedName { get; set; }
        public string RestrictedName { get; set; }
    }

    private sealed class TrackingApplyQueryValidator : ApplyQueryValidator
    {
        public int ValidateCallCount { get; private set; }

        public override void Validate(ApplyQueryOption applyQueryOption, ODataValidationSettings validationSettings)
        {
            ValidateCallCount++;
            base.Validate(applyQueryOption, validationSettings);
        }
    }

    private sealed class NoOpApplyQueryValidator : ApplyQueryValidator
    {
        public override void Validate(ApplyQueryOption applyQueryOption, ODataValidationSettings validationSettings)
        {
            // Intentionally does not call base: a derived validator can fully replace the behavior.
        }
    }

    #endregion
}
