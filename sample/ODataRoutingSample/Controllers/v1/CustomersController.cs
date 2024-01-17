//-----------------------------------------------------------------------------
// <copyright file="CustomersController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataRoutingSample.Models;

namespace ODataRoutingSample.Controllers.v1
{
    [ODataRouteComponent("v1")]
    public class CustomersController : ControllerBase
    {
        private MyDataContext _context;

        public CustomersController(MyDataContext context)
        {
            _context = context;
            if (_context.Customers.Count() == 0)
            {
                IList<Customer> customers = GetCustomers();

                foreach (var customer in customers)
                {
                    _context.Customers.Add(customer);
                }

                _context.SaveChanges();
            }
        }

        // For example: http://localhost:5000/v1/customers?$apply=groupby((Name), aggregate($count as count))&$orderby=name desc
        [HttpGet]
        //// removing this is very important: [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Filter)]
        public IActionResult Get()
        {
            ODataQueryContext context = new ODataQueryContext(Request.GetModel(), Request.GetModel().FindType("ODataRoutingSample.Models.Customer") as IEdmEntityType, path: null);
            var options = new ODataQueryOptions(context, Request);

            var fixedQueryOptions = GenerateString(options.RawValues, AllowedQueryOptions.All & ~AllowedQueryOptions.Expand);
            var expands = options
                .SelectExpand?
                .SelectExpandClause
                .SelectedItems
                .OfType<ExpandedNavigationSelectItem>()
                .Select(expand => string.Join("/", expand.PathToNavigationProperty.Select(segment => segment.Identifier)));
            if (expands != null)
            {
                fixedQueryOptions = (string.IsNullOrEmpty(fixedQueryOptions) ? "?" : $"{fixedQueryOptions}&") + "$expand=" + string.Join(",", expands);

                options.Request.QueryString = new Microsoft.AspNetCore.Http.QueryString(fixedQueryOptions);
                var newOptions = new ODataQueryOptions(options.Context, options.Request);

                Request.ODataFeature().SelectExpandClause = newOptions.SelectExpand.SelectExpandClause;

                var updatedClause = Request.ODataFeature().SelectExpandClause;
            }

            return Ok(GetCustomers());
        }

        private static IReadOnlyDictionary<AllowedQueryOptions, string> GenerateDictionary(ODataRawQueryOptions odataRawQueryOptions)
        {
            var dictionary = new Dictionary<AllowedQueryOptions, string>();
            if (!string.IsNullOrEmpty(odataRawQueryOptions.Apply))
            {
                var option = AllowedQueryOptions.Apply;
                dictionary[option] = "$" + option.ToString().ToLowerInvariant() + "=" + odataRawQueryOptions.Apply;
            }

            //// TODO

            if (!string.IsNullOrEmpty(odataRawQueryOptions.Filter))
            {
                var option = AllowedQueryOptions.Filter;
                dictionary[option] = "$" + option.ToString().ToLowerInvariant() + "=" + odataRawQueryOptions.Filter;
            }

            if (!string.IsNullOrEmpty(odataRawQueryOptions.Select))
            {
                var option = AllowedQueryOptions.Select;
                dictionary[option] = "$" + option.ToString().ToLowerInvariant() + "=" + odataRawQueryOptions.Select;
            }

            return dictionary;
        }

        private static string GenerateString(ODataRawQueryOptions odataRawQueryOptions, AllowedQueryOptions allowedQueryOptions)
        {
            var options = string.Join("&", ExtractOptions(odataRawQueryOptions, allowedQueryOptions));
            if (!string.IsNullOrEmpty(options))
            {
                return options = "?" + options;
            }

            return options;
        }

        private static IEnumerable<AllowedQueryOptions> GenerateAllowedQueryOptions(AllowedQueryOptions allowedQueryOptions)
        {
            foreach (AllowedQueryOptions allowedQueryOption in Enum.GetValues(typeof(AllowedQueryOptions)))
            {
                if ((allowedQueryOptions & allowedQueryOption) == allowedQueryOption)
                {
                    yield return allowedQueryOption;
                }
            }
        }

        private static IEnumerable<string> ExtractOptions(ODataRawQueryOptions odataRawQueryOptions, AllowedQueryOptions allowedQueryOptions)
        {
            var dictionary = GenerateDictionary(odataRawQueryOptions);
            var options = GenerateAllowedQueryOptions(allowedQueryOptions);
            foreach (var option in options)
            {
                if (dictionary.TryGetValue(option, out var value))
                {
                    yield return value;
                }
            }
        }

        [HttpGet]
        [EnableQuery]
        public Customer Get(int key)
        {
            // Be noted: without the NoTracking setting, the query for $select=HomeAddress with throw exception:
            // A tracking query projects owned entity without corresponding owner in result. Owned entities cannot be tracked without their owner...
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return new Customer
            {
                Id = key,
                Name = "Name + " + key
            };
        }

        [HttpPost]
        public IActionResult Post([FromBody] Customer newCustomer)
        {
            return Ok();
        }

        [HttpPost]
        public string RateByName(int key, [FromODataBody] string name, [FromODataBody] int age)
        {
            return key + name + ": " + age;
        }

        [HttpPost]
        [EnableQuery]
        public IActionResult BoundAction(int key, ODataActionParameters parameters)
        {
            return Ok($"BoundAction of Customers with key {key} : {System.Text.Json.JsonSerializer.Serialize(parameters)}");
        }

        private static IList<Customer> GetCustomers()
        {
            return new List<Customer>
            {
                new Customer
                {
                    Id = 1,
                    Name = "Jonier",
                    FavoriteColor = Color.Red,
                    HomeAddress = new Address { City = "Redmond", Street = "156 AVE NE" },
                    Amount = 3,
                    FavoriteAddresses = new List<Address>
                    {
                        new Address { City = "Redmond", Street = "256 AVE NE" },
                        new Address { City = "Redd", Street = "56 AVE NE" },
                    },
                    Foo = new Foo()
                    {
                        Id = 5,
                        SomeProp = "asdf",
                    }
                },
                new Customer
                {
                    Id = 2,
                    Name = "Sam",
                    FavoriteColor = Color.Blue,
                    HomeAddress = new CnAddress { City = "Bellevue", Street = "Main St NE", Postcode = "201100" },
                    Amount = 6,
                    FavoriteAddresses = new List<Address>
                    {
                        new Address { City = "Red4ond", Street = "456 AVE NE" },
                        new Address { City = "Re4d", Street = "51 NE" },
                    },
                },
                new Customer
                {
                    Id = 3,
                    Name = "Peter",
                    FavoriteColor = Color.Green,
                    HomeAddress = new UsAddress { City = "Hollewye", Street = "Main St NE", Zipcode = "98029" },
                    Amount = 4,
                    FavoriteAddresses = new List<Address>
                    {
                        new Address { City = "R4mond", Street = "546 NE" },
                        new Address { City = "R4d", Street = "546 AVE" },
                    },
                },
                new Customer
                {
                    Id = 4,
                    Name = "Sam",
                    FavoriteColor = Color.Red,
                    HomeAddress = new UsAddress { City = "Banff", Street = "183 St NE", Zipcode = "111" },
                    Amount = 5,
                    FavoriteAddresses = new List<Address>
                    {
                        new Address { City = "R4m11ond", Street = "116 NE" },
                        new Address { City = "Jesper", Street = "5416 AVE" },
                    }
                }
            };
        }
    }
}
