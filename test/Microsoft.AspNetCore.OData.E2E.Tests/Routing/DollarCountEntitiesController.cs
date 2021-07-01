// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.UriParser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Routing
{

    public class DollarCountEntitiesController : ODataController
    {

        public IList<DollarCountEntity> Entities;

        public DollarCountEntitiesController()
        {
            Entities = new List<DollarCountEntity>();
            for (int i = 1; i <= 10; i++)
            {
                if (i % 2 == 0)
                {
                    var newEntity = new DollarCountEntity
                    {
                        Id = i,
                        StringCollectionProp = Enumerable.Range(1, 2).Select(index => index.ToString()).ToArray(),
                        EnumCollectionProp = new[] { DollarColor.Red, DollarColor.Blue | DollarColor.Green, DollarColor.Green },
                        TimeSpanCollectionProp = Enumerable.Range(1, 4).Select(_ => TimeSpan.Zero).ToArray(),
                        ComplexCollectionProp =
                            Enumerable.Range(1, 5).Select(_ => new DollarCountComplex()).ToArray(),
                        EntityCollectionProp = Entities.ToArray(),
                        DollarCountNotAllowedCollectionProp = new[] { 1, 2, 3, 4 }
                    };
                    Entities.Add(newEntity);
                }
                else
                {
                    var newEntity = new DerivedDollarCountEntity
                    {
                        Id = i,
                        StringCollectionProp = Enumerable.Range(1, 2).Select(index => index.ToString()).ToArray(),
                        EnumCollectionProp = new[] { DollarColor.Red, DollarColor.Blue | DollarColor.Green, DollarColor.Green },
                        TimeSpanCollectionProp = Enumerable.Range(1, 4).Select(_ => TimeSpan.Zero).ToArray(),
                        ComplexCollectionProp =
                            Enumerable.Range(1, 5).Select(_ => new DollarCountComplex()).ToArray(),
                        EntityCollectionProp = Entities.ToArray(),
                        DollarCountNotAllowedCollectionProp = new[] { 1, 2, 3, 4 },
                        DerivedProp = "DerivedProp"
                    };
                    Entities.Add(newEntity);
                }
            }
        }

        [EnableQuery(PageSize = 3)]
        public IActionResult Get()
        {
            return Ok(Entities);
        }

        [EnableQuery]
        public IActionResult GetDollarCountEntitiesFromDerivedDollarCountEntity()
        {
            return Ok(Entities.OfType<DerivedDollarCountEntity>());
        }

        [EnableQuery]
        public IActionResult Get(int key)
        {
            return Ok(Entities.Single(e => e.Id == key));
        }

        [HttpGet]
        public IActionResult GetStringCollectionProp(int key, ODataQueryOptions<string> options)
        {
            IQueryable<string> result = Entities.Single(e => e.Id == key).StringCollectionProp.AsQueryable();

            if (options.Filter != null)
            {
                result = options.Filter.ApplyTo(result, new ODataQuerySettings()).Cast<string>();
            }

            ODataPath odataPath = Request.ODataFeature().Path;
            if (odataPath.OfType<CountSegment>().Any())
            {
                return Ok(result.Count());
            }

            return Ok(result);
        }

        [HttpGet("DollarCountEntities({key})/EnumCollectionProp/$count")]
        public IActionResult GetCountForEnumCollectionProp(int key, ODataQueryOptions<DollarColor> options)
        {
            IQueryable<DollarColor> result = Entities.Single(e => e.Id == key).EnumCollectionProp.AsQueryable();

            if (options.Filter != null)
            {
                result = options.Filter.ApplyTo(result, new ODataQuerySettings()).Cast<DollarColor>();
            }

            return Ok(result.Count());
        }

        [EnableQuery]
        public IActionResult GetTimeSpanCollectionProp(int key)
        {
            return Ok(Entities.Single(e => e.Id == key).TimeSpanCollectionProp);
        }

        [EnableQuery]
        public IActionResult GetComplexCollectionProp(int key)
        {
            return Ok(Entities.Single(e => e.Id == key).ComplexCollectionProp);
        }

        [EnableQuery]
        public IActionResult GetEntityCollectionProp(int key)
        {
            return Ok(Entities.Single(e => e.Id == key).EntityCollectionProp);
        }

        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.All ^ AllowedQueryOptions.Count)]
        public IActionResult GetDollarCountNotAllowedCollectionProp(int key)
        {
            return Ok(Entities.Single(e => e.Id == key).EntityCollectionProp);
        }

        [HttpGet("UnboundFunctionReturnsPrimitveCollection()/$count")]
        public IActionResult UnboundFunctionReturnsPrimitveCollectionWithDollarCount()
        {
            return Ok(6);
        }

        [HttpGet("UnboundFunctionReturnsEnumCollection()/$count")]
        public IActionResult UnboundFunctionReturnsEnumCollectionWithDollarCount()
        {
            return Ok(7);
        }

        [HttpGet("UnboundFunctionReturnsDateTimeOffsetCollection()/$count")]
        public IActionResult UnboundFunctionReturnsDateTimeOffsetCollectionWithDollarCount()
        {
            return Ok(8);
        }

        [HttpGet("UnboundFunctionReturnsDateCollection()/$count")]
        public IActionResult UnboundFunctionReturnsDateCollectionWithDollarCount()
        {
            return Ok(18);
        }

        [HttpGet("UnboundFunctionReturnsComplexCollection()/$count")]
        public IActionResult UnboundFunctionReturnsComplexCollectionWithDollarCount()
        {
            return Ok(9);
        }

        [HttpGet("UnboundFunctionReturnsEntityCollection()/$count")]
        public IActionResult UnboundFunctionReturnsEntityCollectionWithDollarCount()
        {
            return Ok(10);
        }

        [HttpGet("UnboundFunctionReturnsEntityCollection()/Microsoft.AspNetCore.OData.E2E.Tests.Routing.DerivedDollarCountEntity/$count")]
        public IActionResult UnboundFunctionReturnsDerivedEntityCollectionWithDollarCount()
        {
            return Ok(11);
        }

        [EnableQuery]
        [HttpGet("DollarCountEntities/Default.BoundFunctionReturnsPrimitveCollection()/$count")]
        public IActionResult BoundFunctionReturnsPrimitveCollectionOnCollectionOfDollarCountEntity()
        {
            return Ok(Enumerable.Range(1, 12).Select(_ => DateTimeOffset.Now));
        }
        
        [EnableQuery]
        [HttpGet("DollarCountEntities/Default.BoundFunctionReturnsEnumCollection()/$count")]
        public IActionResult BoundFunctionReturnsEnumCollectionOnCollectionOfDollarCountEntity()
        {
            return Ok(Enumerable.Range(1, 13).Select(_ => DollarColor.Green));
        }

        [EnableQuery]
        [HttpGet("DollarCountEntities/Default.BoundFunctionReturnsDateTimeOffsetCollection()/$count")]
        public IActionResult BoundFunctionReturnsDateTimeOffsetCollectionOnCollectionOfDollarCountEntity()
        {
            return Ok(Enumerable.Range(1, 14).Select(_ => DateTimeOffset.Now));
        }

        [EnableQuery]
        [HttpGet("DollarCountEntities/Default.BoundFunctionReturnsComplexCollection()/$count")]
        public IActionResult BoundFunctionReturnsComplexCollectionOnCollectionOfDollarCountEntity()
        {
            return Ok(Enumerable.Range(1, 15).Select(i => new DollarCountComplex { StringProp = i.ToString() }));
        }

        [EnableQuery]
        [HttpGet("DollarCountEntities/Default.BoundFunctionReturnsEntityCollection()/$count")]
        public IActionResult BoundFunctionReturnsEntityCollectionOnCollectionOfDollarCountEntity()
        {
            return Ok(Entities);
        }

        [EnableQuery]
        [HttpGet("DollarCountEntities/Default.BoundFunctionReturnsEntityCollection()/Microsoft.AspNetCore.OData.E2E.Tests.Routing.DerivedDollarCountEntity/$count")]
        public IActionResult BoundFunctionReturnsDerivedEntityCollectionOnCollectionOfDollarCountEntity()
        {
            return Ok(Entities.OfType<DerivedDollarCountEntity>());
        }
    }
}