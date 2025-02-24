using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DynamicModel;

public static class DynamicEdmModel
{
    private static IEdmModel _edmModel = null;
    
    public static  IEdmModel GetEdmModel()
    {
        if (_edmModel != null)
        {
            return _edmModel;
        }

        EdmModel model = new EdmModel();
        EdmEntityContainer container = new EdmEntityContainer("ns", "container");
        model.AddElement(container);

        EdmEntityType product = new EdmEntityType("ns", "Product");
        product.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
        EdmStructuralProperty key = product.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32);
        product.AddKeys(key);
        model.AddElement(product);
        EdmEntitySet products = container.AddEntitySet("Products", product);

        EdmEntityType detailInfo = new EdmEntityType("ns", "DetailInfo");
        detailInfo.AddKeys(detailInfo.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
        detailInfo.AddStructuralProperty("Title", EdmPrimitiveTypeKind.String);
        model.AddElement(detailInfo);
        EdmEntitySet detailInfos = container.AddEntitySet("DetailInfos", detailInfo);
        EdmNavigationProperty detailInfoNavProp = product.AddUnidirectionalNavigation(
            new EdmNavigationPropertyInfo
            {
                Name = "DetailInfo",
                TargetMultiplicity = EdmMultiplicity.One,
                Target = detailInfo
            });
        products.AddNavigationTarget(detailInfoNavProp, detailInfos);

        // Add a contained navigation property
        product.AddUnidirectionalNavigation(
            new EdmNavigationPropertyInfo
            {
                Name = "ContainedDetailInfo",
                TargetMultiplicity = EdmMultiplicity.One,
                Target = detailInfo,
                ContainsTarget = true
            });
        _edmModel = model;
        return _edmModel;
    }
}