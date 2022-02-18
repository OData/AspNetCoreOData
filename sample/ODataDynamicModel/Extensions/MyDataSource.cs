//-----------------------------------------------------------------------------
// <copyright file="MyDataSource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.OData.Edm;

namespace ODataDynamicModel.Extensions
{
    internal class MyDataSource : IDataSource
    {
        private static IEdmModel _edmModel;

        public virtual IEdmModel GetEdmModel()
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
            EdmEntitySet detailInfos = container.AddEntitySet("DetailInfos", product);

            EdmNavigationProperty detailInfoNavProp = product.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    Name = "DetailInfo",
                    TargetMultiplicity = EdmMultiplicity.One,
                    Target = detailInfo
                });
            products.AddNavigationTarget(detailInfoNavProp, detailInfos);
            _edmModel = model;
            return _edmModel;
        }

        public void Get(IEdmEntityTypeReference entityType, EdmEntityObjectCollection collection)
        {
            EdmEntityObject entity = new EdmEntityObject(entityType);
            entity.TrySetPropertyValue("Name", "abc");
            entity.TrySetPropertyValue("ID", 1);
            entity.TrySetPropertyValue("DetailInfo", CreateDetailInfo(88, "abc_detailinfo", entity.ActualEdmType));

            collection.Add(entity);
            entity = new EdmEntityObject(entityType);
            entity.TrySetPropertyValue("Name", "def");
            entity.TrySetPropertyValue("ID", 2);
            entity.TrySetPropertyValue("DetailInfo", CreateDetailInfo(99, "def_detailinfo", entity.ActualEdmType));

            collection.Add(entity);
        }

        public void Get(string key, EdmEntityObject entity)
        {
            entity.TrySetPropertyValue("Name", "abc");
            entity.TrySetPropertyValue("ID", int.Parse(key));
            entity.TrySetPropertyValue("DetailInfo", CreateDetailInfo(88, "abc_detailinfo", entity.ActualEdmType));
        }

        public object GetProperty(string property, EdmEntityObject entity)
        {
            object value;
            entity.TryGetPropertyValue(property, out value);
            return value;
        }

        private IEdmEntityObject CreateDetailInfo(int id, string title, IEdmStructuredType edmType)
        {
            IEdmNavigationProperty navigationProperty = edmType.DeclaredProperties.OfType<EdmNavigationProperty>().FirstOrDefault(e => e.Name == "DetailInfo");
            if (navigationProperty == null)
            {
                return null;
            }

            EdmEntityObject entity = new EdmEntityObject(navigationProperty.ToEntityType());
            entity.TrySetPropertyValue("ID", id);
            entity.TrySetPropertyValue("Title", title);
            return entity;
        }
    }
}
