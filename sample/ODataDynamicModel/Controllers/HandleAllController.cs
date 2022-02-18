//-----------------------------------------------------------------------------
// <copyright file="HandleAllController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
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
            IEdmEntityTypeReference edmEntityTypeReference = collectionType.ElementType.AsEntity();
            var edmEntityType = edmEntityTypeReference.EntityDefinition();

            //Set the SelectExpandClause on OdataFeature to include navigation property set in the $expand
            SetSelectExpandClauseOnODataFeature(path, edmEntityType);

            // Create an untyped collection with the EDM collection type.
            EdmEntityObjectCollection collection =
                new EdmEntityObjectCollection(new EdmCollectionTypeReference(collectionType));

            // Add untyped objects to collection.
            IDataSource ds = _provider.DataSources[datasource];
            ds.Get(edmEntityTypeReference, collection);

            return collection;
        }

        // Get entityset(key) odata/{datasource}/{entityset}({key})
        public IEdmEntityObject Get(string datasource, string key)
        {
            // Get entity type from path.
            ODataPath path = Request.ODataFeature().Path;
            IEdmEntityType entityType = (IEdmEntityType)path.Last().EdmType;

            //Set the SelectExpandClause on OdataFeature to include navigation property set in the $expand
            SetSelectExpandClauseOnODataFeature(path, entityType);

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

            //Set the SelectExpandClause on OdataFeature to include navigation property set in the $expand
            SetSelectExpandClauseOnODataFeature(path, entityType);

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
            //Set the SelectExpandClause on OdataFeature to include navigation property set in the $expand
            SetSelectExpandClauseOnODataFeature(path, entityType);

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

        /// <summary>
        /// Set the <see cref="SelectExpandClause"/> on ODataFeature.
        /// Without this, the response does not contains navigation property included in $expand
        /// </summary>
        /// <param name="odataPath">OData Path from the Request</param>
        /// <param name="edmEntityType">Entity type on which the query is being performed</param>
        /// <returns></returns>
        private void SetSelectExpandClauseOnODataFeature(ODataPath odataPath, IEdmType edmEntityType)
        {
            IDictionary<string, string> options = new Dictionary<string, string>();
            foreach (var k in Request.Query.Keys)
            {
                options.Add(k, Request.Query[k]);
            }

            //At this point, we should have valid entity segment and entity type.
            //If there is invalid entity in the query, then OData routing should return 404 error before executing this api
            var segment = odataPath.FirstSegment as EntitySetSegment;
            IEdmNavigationSource source = segment?.EntitySet;
            ODataQueryOptionParser parser = new(Request.GetModel(), edmEntityType, source, options);
            //Set the SelectExpand Clause on the ODataFeature otherwise  Odata formatter won't show the expand and select properties in the response.
            Request.ODataFeature().SelectExpandClause = parser.ParseSelectAndExpand();
        }
    }
}
