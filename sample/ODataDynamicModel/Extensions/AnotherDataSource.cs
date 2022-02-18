//-----------------------------------------------------------------------------
// <copyright file="AnotherDataSource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.OData.Edm;

namespace ODataDynamicModel.Extensions
{
    internal class AnotherDataSource : IDataSource
    {
        private IEdmEntityType _school;

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

            EdmEntityType student = new EdmEntityType("ns", "Student");
            student.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            EdmStructuralProperty key = student.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32);
            student.AddKeys(key);
            model.AddElement(student);
            EdmEntitySet students = container.AddEntitySet("Students", student);

            EdmEntityType school = new EdmEntityType("ns", "School");
            school.AddKeys(school.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            school.AddStructuralProperty("CreatedDay", EdmPrimitiveTypeKind.DateTimeOffset);
            model.AddElement(school);
            EdmEntitySet schools = container.AddEntitySet("Schools", student);

            EdmNavigationProperty schoolNavProp = student.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    Name = "School",
                    TargetMultiplicity = EdmMultiplicity.One,
                    Target = school
                });
            students.AddNavigationTarget(schoolNavProp, schools);

            _school = school;
            _edmModel = model;
            return _edmModel;
        }

        public void Get(IEdmEntityTypeReference entityType, EdmEntityObjectCollection collection)
        {
            EdmEntityObject entity = new EdmEntityObject(entityType);
            entity.TrySetPropertyValue("Name", "Foo");
            entity.TrySetPropertyValue("ID", 100);
            entity.TrySetPropertyValue("School", Createchool(99, new DateTimeOffset(2016, 1, 19, 1, 2, 3, TimeSpan.Zero), entity.ActualEdmType));
            collection.Add(entity);

            entity = new EdmEntityObject(entityType);
            entity.TrySetPropertyValue("Name", "Bar");
            entity.TrySetPropertyValue("ID", 101);
            entity.TrySetPropertyValue("School", Createchool(99, new DateTimeOffset(1978, 11, 15, 1, 2, 3, TimeSpan.Zero), entity.ActualEdmType));

            collection.Add(entity);
        }

        public void Get(string key, EdmEntityObject entity)
        {
            entity.TrySetPropertyValue("Name", "Foo");
            entity.TrySetPropertyValue("ID", int.Parse(key));
            entity.TrySetPropertyValue("School", Createchool(99, new DateTimeOffset(2016, 1, 19, 1, 2, 3, TimeSpan.Zero), entity.ActualEdmType));
        }

        public object GetProperty(string property, EdmEntityObject entity)
        {
            object value;
            entity.TryGetPropertyValue(property, out value);
            return value;
        }

        private IEdmEntityObject Createchool(int id, DateTimeOffset dto, IEdmStructuredType edmType)
        {
            IEdmNavigationProperty navigationProperty = edmType.DeclaredProperties.OfType<EdmNavigationProperty>().FirstOrDefault(e => e.Name == "School");
            if (navigationProperty == null)
            {
                return null;
            }

            EdmEntityObject entity = new EdmEntityObject(navigationProperty.ToEntityType());
            entity.TrySetPropertyValue("ID", id);
            entity.TrySetPropertyValue("CreatedDay", dto);
            return entity;
        }
    }
}
