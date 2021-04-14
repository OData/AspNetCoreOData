// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ODataOrderByTest
{
    public class ODataOrderByTest : WebApiTestBase<ODataOrderByTest>
    {
        public ODataOrderByTest(WebApiTestFixture<ODataOrderByTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            string connectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=OrderByColumnTest8";
            services.AddDbContext<OrderByContext>(opt => opt.UseLazyLoadingProxies().UseSqlServer(connectionString));

            IEdmModel edmModel = GetEdmModel();
            services.ConfigureControllers(typeof(OrderByItemsController));
            services.AddOData(opt => opt.AddModel("odata", edmModel).OrderBy().Expand().SetMaxTop(null));
        }

        public static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Item>("Items");
            builder.EntitySet<Item2>("Items2");
            builder.EntitySet<ItemWithEnum>("ItemsWithEnum");
            builder.EntitySet<ItemWithoutColumn>("ItemsWithoutColumn");
            builder.EnumType<SmallNumber>();
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task TestStableOrder_WithCompositeKeyOrderedByColumnAttribute_UseColumnAttributeToDetermineTheKeyOrder()
        {   // Arrange
            await TestOrderedQuery<Item>("Items");
        }

        [Fact]
        public async Task TestStableOrder_WithCompositeKeyOrderedByColumnAttribute_UseColumnAttributeToDetermineTheKeyOrder2()
        {
            await TestOrderedQuery<Item2>("Items2");
        }

        [Fact]
        public async Task TestStableOrder_WithCompositeKeyOrderedByColumnAttribute_AndContainingEnums_UseColumnAttributeToDetermineTheKeyOrder()
        {
            await TestOrderedQuery<ItemWithEnum>("ItemsWithEnum");
        }

        [Fact]
        public async Task TestStableOrder_WithCompositeKeyNotOrdered_OrderTheKeyByPropertyName()
        {
            await TestOrderedQuery<ItemWithoutColumn>("ItemsWithoutColumn");
        }

        private async Task TestOrderedQuery<T>(string entitySet) where T : OrderedItem, new()
        {
            // Arrange
            var requestUri = $"odata/{entitySet}?$top=10";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var rawResult = await response.Content.ReadAsStringAsync();
            var jsonResult = JObject.Parse(rawResult);
            var jsonValue = jsonResult.SelectToken("value");
            Assert.NotNull(jsonValue);
            var concreteResult = jsonValue.ToObject<List<T>>();
            Assert.NotEmpty(concreteResult);
            var expected = Enumerable.Range(1, 4).Select(i => new T() { ExpectedOrder = i }).ToList();
            Assert.Equal(expected, concreteResult, new OrderedItemComparer<T>());
        }

        private sealed class OrderedItemComparer<T> : IEqualityComparer<T> where T : OrderedItem
        {
            public bool Equals(T x, T y)
            {
                return x.ExpectedOrder == y.ExpectedOrder;
            }

            public int GetHashCode(T obj)
            {
                return obj.ExpectedOrder.GetHashCode();
            }
        }
    }
}