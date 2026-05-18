//-----------------------------------------------------------------------------
// <copyright file="AdvancedCastTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
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
                .AddRouteComponents("maxfunctioncalldepth", model));
        }

        [Theory]
        [InlineData(4)]
        [InlineData(6)]
        [InlineData(8)]
        [InlineData(10)]
        [InlineData(12)]
        public async Task UseNestedCastForDifferentDepthWorksFine_ForNullPropagation(int depth)
        {
            // Arrange
            string expr = "Name eq 'x'";
            for (int i = 0; i < depth; i++)
            {
                expr = $"cast({expr},Edm.String)";
            }

            string queryUrl = $"nullpropagation/Items?$filter={expr} eq 'x'";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // VulnerableTests
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        public async Task UseNestedCastForDifferentDepthWorksFine_ForNonNullPropogation(int depth)
        {
            // Arrange
            string expr = "Name eq 'x'";
            for (int i = 0; i < depth; i++)
            {
                expr = $"cast({expr},Edm.String)";
            }

            string filterStr = expr + " eq 'x'";
            this.output.WriteLine("Filter string: {0}", filterStr);

            string queryUrl = $"nonnullpropagation/Items?$filter={filterStr}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // VulnerableTests
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        public async Task UseNestedCastWithNestedContainsForDifferentDepthWorksFine_ForNullPropogation(int depth)
        {
            // Arrange
            string expr = BuildMixedExpression(depth);

            string queryUrl = $"nullpropagation/Items?$filter={expr}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // VulnerableTests
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        public async Task UseNestedCastWithNestedContainsForDifferentDepthWorksFine_ForNonNullPropagation(int depth)
        {
            // Arrange
            string expr = BuildMixedExpression(depth);

            string queryUrl = $"nonnullpropagation/Items?$filter={expr}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // VulnerableTests
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
        }

        [Theory]
        [InlineData("nonnullpropagation")]
        [InlineData("nullpropagation")]
        public async Task UseNestedCastWithNestedContainsThrowsExceedDefaultMaxFunctionCallDepth(string prefix)
        {
            // Arrange
            string expr = BuildMixedExpression(8);

            string queryUrl = $"{prefix}/Items?$filter={expr}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();

            // Act
            var exception = await Assert.ThrowsAsync<ODataException>(async () =>
            {
                await client.SendAsync(request);
            });

            Assert.NotNull(exception);
            Assert.Equal(Error.Format(SRResources.SingleValueFunctionCallTooDeep,16, 15), exception.Message);
        }

        [Theory]
        [InlineData(4)]
        [InlineData(6)]
        [InlineData(8)]
        [InlineData(10)]
        [InlineData(12)]
        [InlineData(18)]
        [InlineData(20)]
        public async Task UseNestedCastWithNestedContainsWithMaxFunctionCallDepthReconfigurationForDifferentDepthWorksFine(int depth)
        {
            // Arrange
            string expr = BuildMixedExpression(depth);

            string queryUrl = $"maxfunctioncalldepth/Items?$filter={expr}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // VulnerableTests
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
        }

        [Fact]
        public async Task UseDeepDepthWithMaxFunctionCallDepthReconfigurationThrowsFromODataLibrary()
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
            return Ok(queryOptions.ApplyTo(_items.AsQueryable(), new ODataQuerySettings { MaxFunctionCallDepth = 100, HandleNullPropagation = HandleNullPropagationOption.False }));
        }
    }

    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

}
