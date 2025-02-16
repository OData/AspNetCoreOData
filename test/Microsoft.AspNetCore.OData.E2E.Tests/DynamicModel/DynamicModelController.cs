using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DynamicModel;

public class DynamicController : ODataController
{
    private DynamicDataSource _dataSource;

    public DynamicController(DynamicDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    [HttpGet("odata/Products")]
    public EdmEntityObjectCollection Get()
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
        _dataSource.Get(edmEntityTypeReference, collection);

        return collection;
    }

    [HttpGet("odata/Products({key})")]
    public IEdmEntityObject Get(string key)
    {
        // Get entity type from path.
        ODataPath path = Request.ODataFeature().Path;
        IEdmEntityType entityType = (IEdmEntityType)path.Last().EdmType;

        //Set the SelectExpandClause on OdataFeature to include navigation property set in the $expand
        SetSelectExpandClauseOnODataFeature(path, entityType);

        // Create an untyped entity object with the entity type.
        EdmEntityObject entity = new EdmEntityObject(entityType);

        _dataSource.Get(key, entity);

        return entity;
    }

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
        parser.Resolver.EnableCaseInsensitive = true;

        //Set the SelectExpand Clause on the ODataFeature otherwise  Odata formatter won't show the expand and select properties in the response.
        Request.ODataFeature().SelectExpandClause = parser.ParseSelectAndExpand();
    }
}