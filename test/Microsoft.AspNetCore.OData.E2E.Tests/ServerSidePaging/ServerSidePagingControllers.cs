//-----------------------------------------------------------------------------
// <copyright file="ServerSidePagingControllers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ServerSidePaging;

public class ServerSidePagingCustomersController : ODataController
{
    private readonly IList<ServerSidePagingCustomer> _serverSidePagingCustomers;

    public ServerSidePagingCustomersController()
    {
        _serverSidePagingCustomers = Enumerable.Range(1, 7)
            .Select(i => new ServerSidePagingCustomer
            {
                Id = i,
                Name = "Customer Name " + i
            }).ToList();

        for (int i = 0; i < _serverSidePagingCustomers.Count; i++)
        {
            // Customer 1 => 6 Orders, Customer 2 => 5 Orders, Customer 3 => 4 Orders, ...
            // NextPageLink will be expected on the Customers collection as well as
            // the Orders child collection on Customer 1
            _serverSidePagingCustomers[i].ServerSidePagingOrders = Enumerable.Range(1, 6 - i)
                .Select(j => new ServerSidePagingOrder
                {
                    Id = j,
                    Amount = (i + j) * 10,
                    ServerSidePagingCustomer = _serverSidePagingCustomers[i]
                }).ToList();
        }
    }

    [EnableQuery(PageSize = 5)]
    public IActionResult Get()
    {
        return Ok(_serverSidePagingCustomers);
    }
}

public class ServerSidePagingEmployeesController : ODataController
{
    private static List<ServerSidePagingEmployee> employees = new List<ServerSidePagingEmployee>(
        Enumerable.Range(1, 13).Select(idx => new ServerSidePagingEmployee
        {
            Id = idx,
            HireDate = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(2022, 11, 07).AddMonths(idx), TimeZoneInfo.Local)
        }));

    [HttpGet]
    [EnableQuery(PageSize = 3)]
    public IActionResult GetEmployeesHiredInPeriod([FromRoute] DateTime fromDate, [FromRoute] DateTime toDate)
    {
        var hiredInPeriod = employees.Where(d => d.HireDate >= fromDate && d.HireDate <= toDate);

        return Ok(hiredInPeriod);
    }
}

public class SkipTokenPagingS1CustomersController : ODataController
{
    private static readonly List<SkipTokenPagingCustomer> customers = new List<SkipTokenPagingCustomer>
    {
        new SkipTokenPagingCustomer { Id = 1, CreditLimit = null },
        new SkipTokenPagingCustomer { Id = 2, CreditLimit = 2 },
        new SkipTokenPagingCustomer { Id = 3, CreditLimit = null },
        new SkipTokenPagingCustomer { Id = 4, CreditLimit = 30 },
        new SkipTokenPagingCustomer { Id = 5, CreditLimit = null },
        new SkipTokenPagingCustomer { Id = 6, CreditLimit = 35 },
        new SkipTokenPagingCustomer { Id = 7, CreditLimit = 5 },
        new SkipTokenPagingCustomer { Id = 8, CreditLimit = 50 },
        new SkipTokenPagingCustomer { Id = 9, CreditLimit = 25 },
    };

    [EnableQuery(PageSize = 2)]
    public ActionResult<IEnumerable<SkipTokenPagingCustomer>> Get()
    {
        return customers;
    }
}

public class SkipTokenPagingS2CustomersController : ODataController
{
    private readonly List<SkipTokenPagingCustomer> customers = new List<SkipTokenPagingCustomer>
    {
        new SkipTokenPagingCustomer { Id = 1, Grade = "A", CreditLimit = null },
        new SkipTokenPagingCustomer { Id = 2, Grade = "B", CreditLimit = null },
        new SkipTokenPagingCustomer { Id = 3, Grade = "A", CreditLimit = 10 },
        new SkipTokenPagingCustomer { Id = 4, Grade = "C", CreditLimit = null },
        new SkipTokenPagingCustomer { Id = 5, Grade = "A", CreditLimit = 30 },
        new SkipTokenPagingCustomer { Id = 6, Grade = "C", CreditLimit = null },
        new SkipTokenPagingCustomer { Id = 7, Grade = "B", CreditLimit = 5 },
        new SkipTokenPagingCustomer { Id = 8, Grade = "C", CreditLimit = 25 },
        new SkipTokenPagingCustomer { Id = 9, Grade = "B", CreditLimit = 50 },
        new SkipTokenPagingCustomer { Id = 10, Grade = "D", CreditLimit = 50 },
        new SkipTokenPagingCustomer { Id = 11, Grade = "F", CreditLimit = 35 },
        new SkipTokenPagingCustomer { Id = 12, Grade = "F", CreditLimit = 30 },
        new SkipTokenPagingCustomer { Id = 13, Grade = "F", CreditLimit = 55 }
    };

    [EnableQuery(PageSize = 4)]
    public ActionResult<IEnumerable<SkipTokenPagingCustomer>> Get()
    {
        return customers;
    }
}

public class SkipTokenPagingS3CustomersController : ODataController
{
    private static readonly List<SkipTokenPagingCustomer> customers = new List<SkipTokenPagingCustomer>
    {
        new SkipTokenPagingCustomer { Id = 1, CustomerSince = null },
        new SkipTokenPagingCustomer { Id = 2, CustomerSince = new DateTime(2023, 1, 2) },
        new SkipTokenPagingCustomer { Id = 3, CustomerSince = null },
        new SkipTokenPagingCustomer { Id = 4, CustomerSince = new DateTime(2023, 1, 30) },
        new SkipTokenPagingCustomer { Id = 5, CustomerSince = null },
        new SkipTokenPagingCustomer { Id = 6, CustomerSince = new DateTime(2023, 2, 4) },
        new SkipTokenPagingCustomer { Id = 7, CustomerSince = new DateTime(2023, 1, 5) },
        new SkipTokenPagingCustomer { Id = 8, CustomerSince = new DateTime(2023, 2, 19) },
        new SkipTokenPagingCustomer { Id = 9, CustomerSince = new DateTime(2023, 1, 25) },
    };

    [EnableQuery(PageSize = 2)]
    public ActionResult<IEnumerable<SkipTokenPagingCustomer>> Get()
    {
        return customers;
    }
}

public class SkipTokenPagingEdgeCase1CustomersController : ODataController
{
    private static readonly List<SkipTokenPagingEdgeCase1Customer> customers = new List<SkipTokenPagingEdgeCase1Customer>
    {
        new SkipTokenPagingEdgeCase1Customer { Id = 2, CreditLimit = 2 },
        new SkipTokenPagingEdgeCase1Customer { Id = 4, CreditLimit = 30 },
        new SkipTokenPagingEdgeCase1Customer { Id = 6, CreditLimit = 35 },
        new SkipTokenPagingEdgeCase1Customer { Id = 7, CreditLimit = 5 },
        new SkipTokenPagingEdgeCase1Customer { Id = 9, CreditLimit = 25 },
    };

    [EnableQuery(PageSize = 2)]
    public ActionResult<IEnumerable<SkipTokenPagingEdgeCase1Customer>> Get()
    {
        return customers;
    }
}

public class ContainmentPagingCustomersController : ODataController
{
    [EnableQuery(PageSize = 2)]
    public ActionResult Get()
    {
        return Ok(ContainmentPagingDataSource.Customers);
    }

    [EnableQuery(PageSize = 2)]
    public ActionResult GetOrders(int key)
    {
        var customer = ContainmentPagingDataSource.Customers.SingleOrDefault(d => d.Id == key);

        if (customer == null)
        {
            return BadRequest();
        }

        return Ok(customer.Orders);
    }
}

public class ContainmentPagingCompanyController : ODataController
{
    private static readonly ContainmentPagingCustomer company = new ContainmentPagingCustomer
    {
        Id = 1,
        Orders = ContainmentPagingDataSource.Orders.Take(ContainmentPagingDataSource.TargetSize).ToList()
    };

    [EnableQuery(PageSize = 2)]
    public ActionResult Get()
    {
        return Ok(company);
    }

    [EnableQuery(PageSize = 2)]
    public ActionResult GetOrders()
    {
        return Ok(company.Orders);
    }
}

public class NoContainmentPagingCustomersController : ODataController
{
    [EnableQuery(PageSize = 2)]
    public ActionResult Get()
    {
        return Ok(NoContainmentPagingDataSource.Customers);
    }

    [EnableQuery(PageSize = 2)]
    public ActionResult GetOrders(int key)
    {
        var customer = NoContainmentPagingDataSource.Customers.SingleOrDefault(d => d.Id == key);

        if (customer == null)
        {
            return BadRequest();
        }

        return Ok(customer.Orders);
    }
}
public class UntypedPagingCustomerOrdersController : ODataController
{
    public ActionResult Get()
    {
        ODataQuerySettings querySettings = new ODataQuerySettings();
        IEdmModel model = HttpContext.ODataFeature().Model;
        ODataQueryOptions queryOptions = CreateQueryOptions(model);
        SetSelectExpandClauseOnODataFeature(model.EntityContainer.FindEntitySet("UntypedPagingCustomerOrders").EntityType, new Dictionary<string, string> { { "$expand", "Orders" } });

        return Ok(queryOptions.ApplyTo(NoContainmentPagingDataSource.UntypedCustomerOrders.AsQueryable(), querySettings, AllowedQueryOptions.All & ~AllowedQueryOptions.SkipToken));
    }

    private ODataQueryOptions CreateQueryOptions(IEdmModel model)
    {
        ODataQueryContext context = CreateQueryContext(model, Request.ODataFeature().Path);
        ODataQueryOptions queryOptions = new ODataQueryOptions(context, Request);
        return queryOptions;
    }

    private static ODataQueryContext CreateQueryContext(IEdmModel model, ODataPath path)
    {
        IEdmStructuredType edmStructuredType = null;
        foreach (var segment in path)
        {
            if (segment.EdmType is IEdmCollectionType collectionType)
                edmStructuredType = collectionType.ElementType.AsEntity().EntityDefinition();
            if (segment.EdmType is IEdmEntityType entityType)
                edmStructuredType = entityType;
            if (segment.EdmType is IEdmComplexType complexType)
                edmStructuredType = complexType;
        }
        if (edmStructuredType == null)
        {
            throw new ArgumentException("No structured type in path");
        }

        return new ODataQueryContext(model, edmStructuredType, path);
    }

    private void SetSelectExpandClauseOnODataFeature(IEdmType edmEntityType, IDictionary<string, string> options = null)
    {
        if (!Request.IsCountRequest() && Request.ODataFeature().SelectExpandClause == null)
        {
            SelectExpandClause selectExpand;

            ODataPath odataPath = Request.ODataFeature().Path;
            var segment = odataPath.FirstSegment as EntitySetSegment;
            IEdmNavigationSource source = segment?.EntitySet;
            ODataQueryOptionParser parser = new ODataQueryOptionParser(Request.GetModel(), edmEntityType, source, options, Request.ODataFeature().Services);
            selectExpand = parser.ParseSelectAndExpand();

            //Set the SelectExpand Clause on the ODataFeature otherwise OData formatter won't show the expand and select properties in the response.
            Request.ODataFeature().SelectExpandClause = selectExpand;
        }
    }
}

public class ContainmentPagingMenusController : ODataController
{
    [EnableQuery(PageSize = 2, MaxExpansionDepth = 4)]
    public ActionResult Get()
    {
        return Ok(ContainmentPagingDataSource.Menus);
    }

    [EnableQuery(PageSize = 2, MaxExpansionDepth = 4)]
    public ActionResult GetFromContainmentPagingExtendedMenu()
    {
        return Ok(ContainmentPagingDataSource.Menus.OfType<ContainmentPagingExtendedMenu>());
    }

    [EnableQuery(PageSize = 2, MaxExpansionDepth = 4)]
    public ActionResult GetTabsFromContainmentPagingExtendedMenu(int key)
    {
        var menu = ContainmentPagingDataSource.Menus.OfType<ContainmentPagingExtendedMenu>().SingleOrDefault(d => d.Id == key);

        if (menu == null)
        {
            return BadRequest();
        }

        return Ok(menu.Tabs);
    }

    [EnableQuery(PageSize = 2, MaxExpansionDepth = 4)]
    public ActionResult GetPanelsFromContainmentPagingExtendedMenu(int key)
    {
        var menu = ContainmentPagingDataSource.Menus.OfType<ContainmentPagingExtendedMenu>().SingleOrDefault(d => d.Id == key);

        if (menu == null)
        {
            return BadRequest();
        }

        return Ok(menu.Panels);
    }
}

public class ContainmentPagingRibbonController : ODataController
{
    private static readonly ContainmentPagingMenu ribbon = new ContainmentPagingExtendedMenu
    {
        Id = 1,
        Tabs = ContainmentPagingDataSource.Tabs.Take(ContainmentPagingDataSource.TargetSize).ToList()
    };

    [EnableQuery(PageSize = 2, MaxExpansionDepth = 4)]
    public ActionResult Get()
    {
        return Ok(ribbon);
    }

    [EnableQuery(PageSize = 2, MaxExpansionDepth = 4)]
    public ActionResult GetFromContainmentPagingExtendedMenu()
    {
        return Ok(ribbon as ContainmentPagingExtendedMenu);
    }

    [EnableQuery(PageSize = 2, MaxExpansionDepth = 4)]
    [HttpGet("ContainmentPagingRibbon/Microsoft.AspNetCore.OData.E2E.Tests.ServerSidePaging.ContainmentPagingExtendedMenu/Tabs")]
    public ActionResult GetTabsFromContainmentPagingExtendedMenu()
    {
        return Ok((ribbon as ContainmentPagingExtendedMenu).Tabs);
    }
}

public class CollectionPagingCustomersController : ODataController
{
    private const int TargetSize = 3;
    private static readonly List<CollectionPagingCustomer> customers = new List<CollectionPagingCustomer>(
        Enumerable.Range(1, TargetSize).Select(idx => new CollectionPagingCustomer
        {
            Id = idx,
            Tags = new List<string> { "Tier 1", "Gen-Z", "HNW" },
            Categories = new List<CollectionPagingCategory>
            {
                CollectionPagingCategory.Retailer,
                CollectionPagingCategory.Wholesaler,
                CollectionPagingCategory.Distributor
            },
            Locations = new List<CollectionPagingLocation>(
                Enumerable.Range(1, TargetSize).Select(dx => new CollectionPagingLocation
                {
                    Street = $"Street {idx}{dx}"
                }))
        }));

    [EnableQuery(PageSize = 2)]
    public ActionResult Get()
    {
        return Ok(customers);
    }
}
