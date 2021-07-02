// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataDynamicModel.Extensions;

namespace ODataDynamicModel.Controllers
{
    public class HandleAllController : ODataController
    {
        private IDataSourceProvider _provider;
        public HandleAllController(IDataSourceProvider provider)
        {
            _provider = provider;
        }

        // Get entityset
        // odata/{datasource}/{entityset}
        public EdmEntityObjectCollection Get(string datasource)
        {
            // Get entity set's EDM type: A collection type.
            ODataPath path = Request.ODataFeature().Path;
            IEdmCollectionType collectionType = (IEdmCollectionType)path.Last().EdmType;
            IEdmEntityTypeReference entityType = collectionType.ElementType.AsEntity();

            // Create an untyped collection with the EDM collection type.
            EdmEntityObjectCollection collection =
                new EdmEntityObjectCollection(new EdmCollectionTypeReference(collectionType));

            // Add untyped objects to collection.
            IDataSource ds = _provider.DataSources[datasource];
            ds.Get(entityType, collection);

            return collection;
        }

        // Get entityset(key) odata/{datasource}/{entityset}({key})
        public IEdmEntityObject Get(string datasource, string key)
        {
            // Get entity type from path.
            ODataPath path = Request.ODataFeature().Path;
            IEdmEntityType entityType = (IEdmEntityType)path.Last().EdmType;

            // Create an untyped entity object with the entity type.
            EdmEntityObject entity = new EdmEntityObject(entityType);

            IDataSource ds = _provider.DataSources[datasource];
            ds.Get(key, entity);

            return entity;
        }

        // odata/{datasource}/{entityset}({key})/Name
        public IActionResult GetName(string datasource, string key)
        {
            // Get entity type from path.
            ODataPath path = Request.ODataFeature().Path;

            PropertySegment property = path.Last() as PropertySegment;
            IEdmEntityType entityType = property.Property.DeclaringType as IEdmEntityType;

            // Create an untyped entity object with the entity type.
            EdmEntityObject entity = new EdmEntityObject(entityType);

            IDataSource ds = _provider.DataSources[datasource];
            ds.Get(key, entity);

            object value = ds.GetProperty("Name", entity);

            if (value == null)
            {
                return NotFound();
            }

            string strValue = value as string;
            return Ok(strValue);
        }

        // odata/{datasource}/{entityset}({key})/{navigation}
        public IActionResult GetNavigation(string datasource, string key, string navigation)
        {
            ODataPath path = Request.ODataFeature().Path;

            NavigationPropertySegment property = path.Last() as NavigationPropertySegment;
            if (property == null)
            {
                return BadRequest("Not the correct navigation property access request!");
            }

            IEdmEntityType entityType = property.NavigationProperty.DeclaringType as IEdmEntityType;

            EdmEntityObject entity = new EdmEntityObject(entityType);
            IDataSource ds = _provider.DataSources[datasource];

            ds.Get(key, entity);

            object value = ds.GetProperty(navigation, entity);

            if (value == null)
            {
                return NotFound();
            }

            IEdmEntityObject nav = value as IEdmEntityObject;
            if (nav == null)
            {
                return NotFound();
            }

            return Ok(nav);
        }
    }
}