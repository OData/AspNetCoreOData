//-----------------------------------------------------------------------------
// <copyright file="ApplyQueryValidationEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ApplyQueryValidation;

/// <summary>
/// Builds the EDM model for the apply/compute query validation tests and marks
/// <see cref="ApplyValidationItem.NotSelectableName"/> as not selectable through the entity type's
/// model-bound query settings, mirroring how existing tests configure non-selectable properties.
/// </summary>
public static class ApplyQueryValidationEdmModel
{
    public static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<ApplyValidationItem>("ApplyValidationItems");
        builder.EntitySet<ApplyValidationItem>("RestrictedLimitItems");
        builder.EntitySet<ApplyValidationItem>("DepthValidationItems").HasManyBinding(item => item.Related, "DepthValidationItems");
        builder.EntitySet<ApplyValidationItem>("DepthTwoValidationItems").HasManyBinding(item => item.Related, "DepthTwoValidationItems");
        builder.EntitySet<ApplyValidationItem>("DepthValidationCollectionShapes").HasManyBinding(item => item.Related, "DepthValidationCollectionShapes");
        var model = builder.GetEdmModel();

        var entityType = model.SchemaElements.OfType<IEdmEntityType>().Single(t => t.Name == nameof(ApplyValidationItem));
        var settings = new Microsoft.OData.ModelBuilder.Config.ModelBoundQuerySettings();
        settings.SelectConfigurations.Add(nameof(ApplyValidationItem.NotSelectableName), SelectExpandType.Disabled);
        model.SetAnnotationValue(entityType, settings);

        return model;
    }
}
