//-----------------------------------------------------------------------------
// <copyright file="CastControllers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Cast;

public class ProductsController : ODataController
{
    private ProductsContext _context;

    public ProductsController(ProductsContext context)
    {
        context.Database.EnsureCreated();
        _context = context;

        if (!_context.Products.Any())
        {
            foreach (Product product in DataSource.InMemoryProducts)
            {
                _context.Products.Add(product);
            }

            _context.SaveChanges();
        }
    }

    [EnableQuery]
    public IActionResult Get()
    {
        if (GetRoutePrefix() == "EF")
        {
            return Ok(_context.Products);
        }
        else
        {
            return Ok(DataSource.InMemoryProducts);
        }
    }

    [EnableQuery]
    public IActionResult GetDimensionInCentimeter(int key)
    {
        if (GetRoutePrefix() == "EF")
        {
            Product product = _context.Products.Single(p => p.ID == key);
            return Ok(product.DimensionInCentimeter);
        }
        else
        {
            Product product = DataSource.InMemoryProducts.Single(p => p.ID == key);
            return Ok(product.DimensionInCentimeter);
        }
    }

    protected string GetRoutePrefix()
    {
        IODataFeature feature = Request.ODataFeature();
        return feature.RoutePrefix;
    }
}
