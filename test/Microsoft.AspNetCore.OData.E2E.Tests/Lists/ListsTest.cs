//-----------------------------------------------------------------------------
// <copyright file="ListsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------
#if NET8_0_OR_GREATER
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Lists
{
    public class ListsTest : WebApiTestBase<ListsTest>
    {
        private Dictionary<string, string> _map;

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            string currentDirectory = Environment.CurrentDirectory;
            services.ConfigureControllers(typeof(ProductsController), typeof(MetadataController));
            var keepAliveConnection = new SqliteConnection("DataSource=:memory:");
            keepAliveConnection.Open();

            services.AddDbContext<ListsContext>(options =>
                options.UseSqlite(keepAliveConnection));

            IEdmModel model1 = ListsEdmModel.GetConventionModel();

            services.AddControllers().AddOData(opt => opt.Count().Filter().Expand().Select().OrderBy().SetMaxTop(5)
                .AddRouteComponents("convention", model1));

            var serviceProvider = services.BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ListsContext>();
                context.Database.OpenConnection(); // Open connection to in-memory database
                context.Database.EnsureCreated(); // Ensure the schema is created
            }
        }

        public ListsTest(WebApiTestFixture<ListsTest> fixture)
           : base(fixture)
        {
             _map = new Dictionary<string, string>
                {
                    {"ListTestString", "String"},
                    {"ListTestBool", "Boolean"},
                    {"ListTestInt", "Int32"},
                    {"ListTestDouble", "Double"},
                    {"ListTestFloat", "Single"},
                    {"ListTestDateTime", "DateTimeOffset"},
                    {"ListTestUri", "Microsoft.AspNetCore.OData.E2E.Tests.Lists.Uri"},
                    {"ListTestOrder", "Microsoft.AspNetCore.OData.E2E.Tests.Lists.Order"},
                    {"ListTestUint", "UInt32"},
                };
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task QueryEntitySet(string format)
        {
            // Arrange
            await ResetDatasource();
            string requestUri = "/convention/Products?$format=" + format;
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var json = await response.Content.ReadAsObject<JObject>();
            var results = json.GetValue("value") as JArray;
            Assert.Equal<int>(5, results.Count);
            if (format == "application/json;odata.metadata=full")
            {
                var typeOfListTestString = results[0]["ListTestString@odata.type"].ToString();
                Assert.Equal("#Collection(String)", typeOfListTestString);

                var typeOfListTestBool = results[0]["ListTestBool@odata.type"].ToString();
                Assert.Equal("#Collection(Boolean)", typeOfListTestBool);

                var typeOfListTestFloat = results[0]["ListTestFloat@odata.type"].ToString();
                Assert.Equal("#Collection(Single)", typeOfListTestFloat);

                var typeOfListTestDouble = results[0]["ListTestDouble@odata.type"].ToString();
                Assert.Equal("#Collection(Double)", typeOfListTestDouble);

                var typeOfListTestInt = results[0]["ListTestInt@odata.type"].ToString();
                Assert.Equal("#Collection(Int32)", typeOfListTestInt);

                var typeOfListTestDateTime = results[0]["ListTestDateTime@odata.type"].ToString();
                Assert.Equal("#Collection(DateTimeOffset)", typeOfListTestDateTime);

                var typeOfListTestUri = results[0]["ListTestUri@odata.type"].ToString();
                Assert.Equal("#Collection(Microsoft.AspNetCore.OData.E2E.Tests.Lists.Uri)", typeOfListTestUri);
            }
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task QueryEntity(string format)
        {
            await ResetDatasource();
            HttpClient client = CreateClient();
            string requestUri = "/convention/Products('1')?$format=" + format;

            HttpResponseMessage response = await client.GetAsync(requestUri);
            Assert.True(response.IsSuccessStatusCode);

            var result = await response.Content.ReadAsObject<JObject>();
            if (format == "application/json;odata.metadata=full")
            {
                var typeOfListTestString = result["ListTestString@odata.type"].ToString();
                Assert.Equal("#Collection(String)", typeOfListTestString);

                var typeOfListTestBool = result["ListTestBool@odata.type"].ToString();
                Assert.Equal("#Collection(Boolean)", typeOfListTestBool);

                var typeOfListTestFloat = result["ListTestFloat@odata.type"].ToString();
                Assert.Equal("#Collection(Single)", typeOfListTestFloat);

                var typeOfListTestDouble = result["ListTestDouble@odata.type"].ToString();
                Assert.Equal("#Collection(Double)", typeOfListTestDouble);

                var typeOfListTestInt = result["ListTestInt@odata.type"].ToString();
                Assert.Equal("#Collection(Int32)", typeOfListTestInt);

                var typeOfListTestDateTime = result["ListTestDateTime@odata.type"].ToString();
                Assert.Equal("#Collection(DateTimeOffset)", typeOfListTestDateTime);

                var typeOfListTestUri = result["ListTestUri@odata.type"].ToString();
                Assert.Equal("#Collection(Microsoft.AspNetCore.OData.E2E.Tests.Lists.Uri)", typeOfListTestUri);
            }
        }

        [Theory]
        [InlineData("/convention/Products/$count", 5)]
        [InlineData("/convention/Products/$count?$filter=Name eq 'Product1'", 1)]
        public async Task QueryEntitySetCount(string requestUri, int expectedCount)
        {
            // Arrange
            await ResetDatasource();
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            string count = await response.Content.ReadAsStringAsync();
            Assert.Equal<int>(expectedCount, int.Parse(count));
        }

        [Theory]
        [InlineData("/convention/Products/$count", true)]
        [InlineData("/convention/Products/$count", false)]
        public async Task QueryEntitySetCountEncoding(string requestUri, bool sendAcceptCharset)
        {
            // Arrange
            await ResetDatasource();
            HttpClient client = CreateClient();
            client.DefaultRequestHeaders.AcceptCharset.Clear();
            if (sendAcceptCharset)
            {
                client.DefaultRequestHeaders.AcceptCharset.ParseAdd("utf-8");
            }

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var blob = await response.Content.ReadAsByteArrayAsync();
            Assert.Equal("utf-8", response.Content.Headers.ContentType.CharSet, StringComparer.OrdinalIgnoreCase);
            var count = Encoding.UTF8.GetString(blob);
            Assert.Equal(5, int.Parse(count));
            Assert.False((blob[0] == 0xEF) && (blob[1] == 0xBB) && (blob[2] == 0xBF));
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full", "ListTestString")]
        [InlineData("application/json;odata.metadata=full", "ListTestBool")]
        [InlineData("application/json;odata.metadata=full", "ListTestFloat")]
        [InlineData("application/json;odata.metadata=full", "ListTestDouble")]
        [InlineData("application/json;odata.metadata=full", "ListTestInt")]
        [InlineData("application/json;odata.metadata=full", "ListTestDateTime")]
        //[InlineData("application/json;odata.metadata=full", "ListTestUri")]
        [InlineData("application/json;odata.metadata=full", "ListTestOrder")]
        public async Task QueryEntitySetSelect(string format, string select)
        {
            // Arrange
            await ResetDatasource();
            string requestUri = "/convention/Products?$expand=ListTestOrder&$format=" + format + "&$select=" + select;
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var json = await response.Content.ReadAsObject<JObject>();
            var results = json.GetValue("value") as JArray;
            Assert.Equal<int>(5, results.Count);

            var typeOfListTestString = results[0][$"{select}@odata.type"].ToString();
            Assert.Equal($"#Collection({_map[select]})", typeOfListTestString);

        }

        [Theory]
        [InlineData("application/json;odata.metadata=full", "ListTestString")]
        [InlineData("application/json;odata.metadata=full", "ListTestBool")]
        [InlineData("application/json;odata.metadata=full", "ListTestFloat")]
        [InlineData("application/json;odata.metadata=full", "ListTestDouble")]
        [InlineData("application/json;odata.metadata=full", "ListTestInt")]
        [InlineData("application/json;odata.metadata=full", "ListTestDateTime")]
        //[InlineData("application/json;odata.metadata=full", "ListTestUri")]
        [InlineData("application/json;odata.metadata=full", "ListTestOrder")]
        public async Task QueryEntitySelect(string format, string select)
        {
            // Arrange
            await ResetDatasource();
            string requestUri = "/convention/Products('1')?$expand=ListTestOrder&$format=" + format + "&$select=" + select;
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var result = await response.Content.ReadAsObject<JObject>();

            var typeOfListTestString = result[$"{select}@odata.type"].ToString();
            Assert.Equal($"#Collection({_map[select]})", typeOfListTestString);
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full", "ListTestString", "/any(t: t eq 'Test99')", 2)]
        [InlineData("application/json;odata.metadata=full", "ListTestString", "/all(t: contains(t, 'Test98'))", 2)]
        [InlineData("application/json;odata.metadata=full", "ListTestBool", "/any(b: b eq true)", 4)]
        [InlineData("application/json;odata.metadata=full", "ListTestBool", "/all(b: b eq true)", 1)]
        [InlineData("application/json;odata.metadata=full", "ListTestFloat", "/any(f: f lt 4.0)", 2)]
        [InlineData("application/json;odata.metadata=full", "ListTestFloat", "/all(f: f lt 4.0)", 1)]
        [InlineData("application/json;odata.metadata=full", "ListTestDouble", "/any(d: d ge 5.5)", 3)]
        [InlineData("application/json;odata.metadata=full", "ListTestDouble", "/all(d: d gt 5.5)", 2)]
        [InlineData("application/json;odata.metadata=full", "ListTestInt", "/any(i: i le 5)", 2)]
        [InlineData("application/json;odata.metadata=full", "ListTestInt", "/all(i: i le 5)", 1)]
        //[InlineData("application/json;odata.metadata=full", "ListTestDateTime", "/any(d: d gt 2024-07-26T00:00:00Z)", 4)]
        //[InlineData("application/json;odata.metadata=full", "ListTestDateTime", "/all(d: d gt 2024-07-26T00:00:00Z)", 4)]
        //[InlineData("application/json;odata.metadata=full", "ListTestUri")]
        public async Task QueryEntitySetFilter(string format, string filter, string expr, int expected)
        {
            // Arrange
            await ResetDatasource();
            string requestUri = "/convention/Products?$format=" + format + "&$filter=" + filter+expr;
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var json = await response.Content.ReadAsObject<JObject>();
            var results = json.GetValue("value") as JArray;
            Assert.Equal<int>(expected, results.Count);

            var typeOfListTestString = results[0][$"{filter}@odata.type"].ToString();
            Assert.Equal($"#Collection({_map[filter]})", typeOfListTestString);
        }

        private async Task<HttpResponseMessage> ResetDatasource()
        {
            HttpClient client = CreateClient();
            var response = await client.PostAsync("convention/ResetDataSource", null);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            return response;
        }
    }
}
#endif
