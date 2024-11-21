//-----------------------------------------------------------------------------
// <copyright file="EdmModelBuilder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.ComponentModel;

namespace microsoft.graph;

public class GraphModel
{
    public static EdmModel Model = GetEdmModel();
    public static EdmEntityType userType;
    public static EdmEntityType groupType;
    public static EdmEntityType recoveryChangeObjectType;
    public static EdmModel GetEdmModel()
    {
        var model = new EdmModel();
        string graphNamespace = "microsoft.graph";
        var entityContainer = model.AddEntityContainer(graphNamespace, "graphService");

        // define recovery preview job
        var recoveryPreviewJobType = model.AddEntityType(graphNamespace,"recoveryPreviewJob");
        recoveryPreviewJobType.AddKeys(
            recoveryPreviewJobType.AddStructuralProperty("id", EdmPrimitiveTypeKind.String)
        );
        entityContainer.AddEntitySet("recoveryPreviewJobs", recoveryPreviewJobType);

        // define recovery change object
        recoveryChangeObjectType = model.AddEntityType(graphNamespace, "recoveryChangeObject");
        recoveryChangeObjectType.AddKeys(
            recoveryChangeObjectType.AddStructuralProperty("id", EdmPrimitiveTypeKind.String)
        );
        recoveryChangeObjectType.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
        {
            Name = "currentState",
            Target = EdmCoreModel.Instance.GetEntityType(),
            TargetMultiplicity = EdmMultiplicity.One
        });
        recoveryChangeObjectType.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
        {
            Name = "deltaFromCurrent",
            Target = EdmCoreModel.Instance.GetEntityType(),
            TargetMultiplicity = EdmMultiplicity.One
        });

        // schema for graph types (loaded from graph csdl)
        var directoryObjectType = model.AddEntityType(graphNamespace, "directoryObject");
        directoryObjectType.AddKeys(
            directoryObjectType.AddStructuralProperty("id", EdmPrimitiveTypeKind.String)
        );
        directoryObjectType.AddStructuralProperty("displayName",EdmPrimitiveTypeKind.String);
        userType = model.AddEntityType(graphNamespace, "user", directoryObjectType);
        userType.AddStructuralProperty("emailName", EdmPrimitiveTypeKind.String);
        groupType = model.AddEntityType(graphNamespace, "group", directoryObjectType);
        groupType.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
        {
            Name = "members",
            Target = directoryObjectType,
            TargetMultiplicity= EdmMultiplicity.Many
        });

        // add getChanges function
        var recoveryChangeObjectCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(recoveryChangeObjectType,false)));
        var getChangesFunction = new EdmFunction(graphNamespace, "getChanges", recoveryChangeObjectCollectionType, true, null, true);
        getChangesFunction.AddParameter("recoveryPreviewJob", new EdmEntityTypeReference(recoveryPreviewJobType, false));

        model.AddElement(getChangesFunction);

        // TODO: Temportary hack to provide navigation source when writing; get rid of this 
        entityContainer.AddEntitySet("recoveryChangeObjects", recoveryChangeObjectType);
        model.SetAnnotationValue<ReturnedEntitySetAnnotation>(getChangesFunction, new ReturnedEntitySetAnnotation("recoveryChangeObjects"));

        //jobs.EntityType.Function("getChanges").ReturnsCollectionFromEntitySet<recoveryChangeObject>("recoveryChangeObjects");
        //jobs.EntityType.Function("getChanges").ReturnsCollectionViaEntitySetPath<recoveryChangeObject>("getChanges");
        //jobs.EntityType.Function("getChanges").ReturnsCollection<recoveryChangeObject>();

        return model;
    }
}
