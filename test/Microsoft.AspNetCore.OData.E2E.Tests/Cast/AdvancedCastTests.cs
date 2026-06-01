//-----------------------------------------------------------------------------
// <copyright file="AdvancedCastTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Cast
{
    public class AdvancedCastTests : WebApiTestBase<AdvancedCastTests>
    {
        private readonly ITestOutputHelper output;

        public AdvancedCastTests(WebApiTestFixture<AdvancedCastTests> fixture, ITestOutputHelper output)
            : base(fixture)
        {
            this.output = output;
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Item>("Items");
            IEdmModel model = modelBuilder.GetEdmModel();

            services.ConfigureControllers(typeof(ItemsController));

            services.AddControllers().AddOData(opt =>
            opt.Count().Filter().OrderBy().Expand().SetMaxTop(null).Select()
                .AddRouteComponents("nullpropagation", model)
                .AddRouteComponents("nonnullpropagation", model)
                .AddRouteComponents("maxfunctioncalldepth", model)
                .AddRouteComponents("defaultenablequery", model) // default 'EnableQueryAttribute' with 'MaxFunctionCallDepth' = 15
                .AddRouteComponents("reconfigedenablequery", model)); // configured 'EnableQueryAttribute' with 'MaxFunctionCallDepth' = 100
        }

        [Theory]
        [InlineData(4, true)]
        [InlineData(6, true)]
        [InlineData(8, true)]
        [InlineData(10, true)]
        [InlineData(15, true)]
        [InlineData(16, false)]
        [InlineData(20, false)]
        public async Task UseNestedCastForDifferentDepthWorksFine_ForNullPropagation(int depth, bool success)
        {
            // Arrange
            string expr = GetCastExpr(depth); // "cast(....cast(cast(Name eq 'x',Edm.String) eq 'x',Edm.String))))"
            string queryUrl = $"nullpropagation/Items?$filter={expr} eq 'x'"; 

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            if (success)
            {
                // Act
                response = await client.SendAsync(request);

                // Assert
                Assert.NotNull(response);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.NotNull(response.Content);
            }
            else
            {
                var exception = await Assert.ThrowsAsync<ODataException>(async () =>
                {
                    await client.SendAsync(request);
                });

                Assert.NotNull(exception);
                Assert.Equal(Error.Format(SRResources.MaxFunctionCallDepthExceeded, 15, "MaxFunctionCallDepth"), exception.Message);
            }
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(5, true)]
        [InlineData(6, true)]
        [InlineData(7, true)]
        [InlineData(15, true)]
        [InlineData(16, false)]
        [InlineData(50, false)]
        public async Task UseNestedCastForDifferentDepthWorksFine_ForNonNullPropagation(int depth, bool success)
        {
            // Arrange
            string expr = GetCastExpr(depth);
            string filterStr = expr + " eq 'x'"; // "cast(....cast(cast(Name eq 'x',Edm.String) eq 'x',Edm.String)))) eq 'x'"

            string queryUrl = $"nonnullpropagation/Items?$filter={filterStr}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            if (success)
            {
                // Act
                response = await client.SendAsync(request);
                // Assert
                Assert.NotNull(response);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.NotNull(response.Content);
            }
            else
            {
                var exception = await Assert.ThrowsAsync<ODataException>(async () =>
                {
                    await client.SendAsync(request);
                });
                Assert.NotNull(exception);
                Assert.Equal(Error.Format(SRResources.MaxFunctionCallDepthExceeded, 15, "MaxFunctionCallDepth"), exception.Message);
            }
        }

        private static string GetCastExpr(int depth)
        {
            string expr = $"Name eq 'x'";
            for (int i = 0; i < depth; i++)
            {
                expr = $"cast({expr},Edm.String)";
            }
            return expr;
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        // Each depth here produces two nested function calls (contains + cast), so depth 7 = 14 function calls (within the default MaxFunctionCallDepth of 15).
        public async Task UseNestedCastWithNestedContainsForDifferentDepth_ReturnsAsExpected_ForNullPropagation(int depth)
        {
            // Arrange
            string expr = BuildMixedExpression(depth); // contains('a', cast(contains('a', .....('a', cast(Name eq 'x', Edm.String)), Edm.String)), Edm.String)), Edm.String))

            string queryUrl = $"nullpropagation/Items?$filter={expr}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(5, true)]
        [InlineData(6, true)]
        [InlineData(7, true)]
        [InlineData(8, false)]
        [InlineData(12, false)]
        [InlineData(20, false)]
        public async Task UseNestedCastWithNestedContainsForDifferentDepth_ReturnsAsExpected_ForNonNullPropagation(int depth, bool success)
        {
            // Arrange
            string expr = BuildMixedExpression(depth); // contains('a', cast(contains('a', .....('a', cast(Name eq 'x', Edm.String)), Edm.String)), Edm.String)), Edm.String))

            string queryUrl = $"nonnullpropagation/Items?$filter={expr}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            if (success)
            {
                // Act
                response = await client.SendAsync(request);
                // Assert
                Assert.NotNull(response);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.NotNull(response.Content);
            }
            else
            {
                var exception = await Assert.ThrowsAsync<ODataException>(async () =>
                {
                    await client.SendAsync(request);
                });
                Assert.NotNull(exception);
                Assert.Equal(Error.Format(SRResources.MaxFunctionCallDepthExceeded, 15, "MaxFunctionCallDepth"), exception.Message);
            }
        }

        [Theory]
        [InlineData(4, true)]
        [InlineData(6, true)]
        [InlineData(8, true)]
        [InlineData(10, true)]
        [InlineData(12, true)]
        [InlineData(18, false)]
        [InlineData(20, false)]
        public async Task UseNestedCastWithNestedContainsWithMaxFunctionCallDepthReconfigurationForDifferentDepthWorksFine(int depth, bool success)
        {
            // Arrange
            string expr = BuildMixedExpression(depth);

            string queryUrl = $"maxfunctioncalldepth/Items?$filter={expr}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            if (success)
            {
                // Act
                response = await client.SendAsync(request);

                // Assert
                Assert.NotNull(response);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.NotNull(response.Content);
            }
            else
            {
                var exception = await Assert.ThrowsAsync<ODataException>(async () =>
                {
                    await client.SendAsync(request);
                });
                Assert.NotNull(exception);
                Assert.Equal(Error.Format(SRResources.MaxFunctionCallDepthExceeded, 30, "MaxFunctionCallDepth"), exception.Message);
            }
        }

        [Fact]
        public async Task UseVeryDeepDepthWithMaxFunctionCallDepthReconfiguration_ThrowsFromODataLibraryFirst()
        {
            // Arrange
            string expr = BuildMixedExpression(50);

            string queryUrl = $"maxfunctioncalldepth/Items?$filter={expr}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();

            var exception = await Assert.ThrowsAsync<ODataException>(async () =>
            {
                await client.SendAsync(request);
            });

            Assert.NotNull(exception);
            Assert.Equal("Recursion depth exceeded allowed limit.", exception.Message);
        }

        [Theory]
        [InlineData(4, HttpStatusCode.OK)]
        [InlineData(6, HttpStatusCode.OK)]
        [InlineData(7, HttpStatusCode.OK)]
        [InlineData(8, HttpStatusCode.BadRequest)]
        [InlineData(10, HttpStatusCode.BadRequest)]
        public async Task UseDifferentDepth_OnDefaultEnableQuery_ReturnsResponseAsExpected(int depth, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            string expr = BuildMixedExpression(depth);

            string queryUrl = $"defaultenablequery/Items?$filter={expr}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();

            // Act
            var response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(expectedStatusCode, response.StatusCode);
            if (expectedStatusCode == HttpStatusCode.BadRequest)
            {
                string payload = await response.Content.ReadAsStringAsync();
                Assert.Contains(Error.Format(SRResources.MaxFunctionCallDepthExceeded, 15, "MaxFunctionCallDepth"), payload);
            }
        }

        [Theory]
        [InlineData(4, HttpStatusCode.OK)]
        [InlineData(6, HttpStatusCode.OK)]
        [InlineData(7, HttpStatusCode.OK)]
        [InlineData(8, HttpStatusCode.OK)]
        [InlineData(10, HttpStatusCode.OK)]
        [InlineData(18, HttpStatusCode.OK)]
        [InlineData(19, HttpStatusCode.OK)]
        [InlineData(20, HttpStatusCode.BadRequest)] // Even though MaxFunctionCallDepth is 100, ODL rejects this first with 'The node count limit of '100' has been exceeded' (default MaxNodeCount = 100).
        [InlineData(50, HttpStatusCode.BadRequest)]
        public async Task UseDifferentDepth_OnReConfiguredEnableQuery_ReturnsResponseAsExpected(int depth, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            string expr = BuildMixedExpression(depth);

            string queryUrl = $"reconfigedenablequery/Items?$filter={expr}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();

            // Act
            var response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(expectedStatusCode, response.StatusCode);
            if (expectedStatusCode == HttpStatusCode.BadRequest)
            {
                string payload = await response.Content.ReadAsStringAsync();
                // Depending on the depth, ODL may reject the request via either the node-count limit
                // (e.g. "The node count limit of '100' has been exceeded") or the URI parser's
                // recursion-depth guard ("Recursion depth exceeded allowed limit.") before our
                // MaxFunctionCallDepth validator runs. Both are valid 400 rejection paths.
                Assert.True(
                    payload.Contains("node count limit", StringComparison.OrdinalIgnoreCase) ||
                    payload.Contains("Recursion depth exceeded", StringComparison.OrdinalIgnoreCase),
                    $"Unexpected BadRequest payload: {payload}");
            }
        }

        // Note: each depth here produces two nested function calls (contains + cast). So depth 4 = 8 function calls.
        private static string BuildMixedExpression(int depth)
        {
            string expr = "Name eq 'x'";
            for (int i = 0; i < depth; i++)
            {
                expr = $"contains('a',cast({expr},Edm.String))";
            }

            return expr;
        }
    }

    [ODataAttributeRouting]
    public class ItemsController : ControllerBase
    {
        private static readonly List<Item> _items = Enumerable.Range(1, 10).Select(i => new Item { Id = i, Name = $"Item-{i}" }).ToList();

        [HttpGet("nullpropagation/items")]
        public IActionResult GetItemsNullPropagation(ODataQueryOptions<Item> queryOptions)
        {
            return Ok(queryOptions.ApplyTo(_items.AsQueryable())); // the default is true for null propagation for LINQ-To-Object.
        }

        [HttpGet("nonnullpropagation/items")]
        public IActionResult GetItemsNonNullPropagation(ODataQueryOptions<Item> queryOptions)
        {
            return Ok(queryOptions.ApplyTo(_items.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False }));
        }

        [HttpGet("maxfunctioncalldepth/items")]
        public IActionResult GetItemsMaxFunctionCallDepth(ODataQueryOptions<Item> queryOptions)
        {
            // It's developer's responsibility to call 'Validate()' before applying the query to data source to make sure the query is valid and safe to execute.
            int maxFunctionCallDepth = 30;
            queryOptions.Validate(new ODataValidationSettings
            {
                MaxFunctionCallDepth = maxFunctionCallDepth
            });

            return Ok(queryOptions.ApplyTo(_items.AsQueryable(), new ODataQuerySettings { MaxFunctionCallDepth = maxFunctionCallDepth, HandleNullPropagation = HandleNullPropagationOption.False }));
        }

        [EnableQuery(HandleNullPropagation = HandleNullPropagationOption.False)]
        [HttpGet("defaultenablequery/items")]
        public IActionResult GetItemsUsingDefaultEnableQuery()
        {
            return Ok(_items.AsQueryable());
        }

        [EnableQuery(MaxFunctionCallDepth = 100, HandleNullPropagation = HandleNullPropagationOption.False)]
        [HttpGet("reconfigedenablequery/items")]
        public IActionResult GetItemsUsingEnableQuery()
        {
            return Ok(_items.AsQueryable());
        }
    }

    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}
