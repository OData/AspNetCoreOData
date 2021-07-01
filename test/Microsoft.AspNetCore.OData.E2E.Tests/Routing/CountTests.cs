// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.E2E.Tests.Routing;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.OData.E2E.Tests
{

    public class CountTests : WebApiTestBase<CountTests>
    {

        public CountTests(WebApiTestFixture<CountTests> fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
            ConfigureServicesAction = (IServiceCollection services) =>
            {
                IEdmModel edmModel = GetEdmModel();
                services.ConfigureControllers(typeof(DollarCountEntitiesController), typeof(ODataEndpointController));
                services.AddControllers().AddOData(opt => opt.AddModel(edmModel).Count().OrderBy().Filter().Expand().SetMaxTop(null).Select());
            };
        }

        public static TheoryDataSet<string, int> DollarCountData
        {
            get
            {
                var data = new TheoryDataSet<string, int>();

                // $count follows entity set, structural collection property or navigation collection property.
                data.Add("DollarCountEntities/$count", 10);
                data.Add("DollarCountEntities/$count?$filter=Id gt 5", 5);
                data.Add("DollarCountEntities/Microsoft.AspNetCore.OData.E2E.Tests.Routing.DerivedDollarCountEntity/$count", 5);
                data.Add("DollarCountEntities(5)/StringCollectionProp/$count", 2);
                data.Add("DollarCountEntities(5)/StringCollectionProp/$count?$filter=$it eq '2'", 1);
                data.Add("DollarCountEntities(5)/EnumCollectionProp/$count", 3);
                data.Add("DollarCountEntities(5)/EnumCollectionProp/$count?$filter=$it has Microsoft.AspNetCore.OData.E2E.Tests.Routing.DollarColor'Green'", 2);
                data.Add("DollarCountEntities(5)/TimeSpanCollectionProp/$count", 4);
                data.Add("DollarCountEntities(5)/ComplexCollectionProp/$count", 5);
                data.Add("DollarCountEntities(5)/EntityCollectionProp/$count", 4);

                // $count follows unbound function that returns collection.
                data.Add("UnboundFunctionReturnsPrimitveCollection()/$count", 6);
                data.Add("UnboundFunctionReturnsEnumCollection()/$count", 7);
                data.Add("UnboundFunctionReturnsDateTimeOffsetCollection()/$count", 8);
                data.Add("UnboundFunctionReturnsDateCollection()/$count", 18);
                data.Add("UnboundFunctionReturnsComplexCollection()/$count", 9);
                data.Add("UnboundFunctionReturnsEntityCollection()/$count", 10);
                data.Add("UnboundFunctionReturnsEntityCollection()/Microsoft.AspNetCore.OData.E2E.Tests.Routing.DerivedDollarCountEntity/$count", 11);

                // $count follows bound function that returns collection.
                data.Add("DollarCountEntities/Default.BoundFunctionReturnsPrimitveCollection()/$count", 12);
                data.Add("DollarCountEntities/Default.BoundFunctionReturnsEnumCollection()/$count", 13);
                data.Add("DollarCountEntities/Default.BoundFunctionReturnsDateTimeOffsetCollection()/$count", 14);
                data.Add("DollarCountEntities/Default.BoundFunctionReturnsComplexCollection()/$count", 15);
                data.Add("DollarCountEntities/Default.BoundFunctionReturnsComplexCollection()/$count?$filter=contains(StringProp,'1')", 7);
                data.Add("DollarCountEntities/Default.BoundFunctionReturnsEntityCollection()/$count", 10);
                data.Add("DollarCountEntities/Default.BoundFunctionReturnsEntityCollection()/Microsoft.AspNetCore.OData.E2E.Tests.Routing.DerivedDollarCountEntity/$count", 5);

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(DollarCountData))]
        public async Task DollarCount_Works(string uri, int expectedCount)
        {
            // Arrange & Act
            HttpClient client = CreateClient();
            HttpResponseMessage response = await client.GetAsync(uri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            int actualCount = int.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(expectedCount, actualCount);
        }

        [Fact]
        public async Task GetCollection_Works_WithoutDollarCount()
        {
            // Arrange
            string uri = "DollarCountEntities(5)/StringCollectionProp";
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(uri);

            // Assert
            response.EnsureSuccessStatusCode();
            string responseStr = await response.Content.ReadAsStringAsync();

            using (JsonDocument jsonDoc = JsonDocument.Parse(responseStr))
            {
                JsonElement jsonValue = jsonDoc.RootElement.GetProperty("value");
                Assert.Collection(jsonValue.EnumerateArray(),
                    e =>
                    {
                        Assert.Equal("1", e.GetString());
                    },
                    e =>
                    {
                        Assert.Equal("2", e.GetString());
                    });
            }
        }

        [Fact]
        public async Task Function_Works_WithDollarCountInQueryOption()
        {
            // Arrange
            string uri = "DollarCountEntities/Default.BoundFunctionReturnsComplexCollection()?$count=true";
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(uri);

            // Assert
            response.EnsureSuccessStatusCode();
            string responseStr = await response.Content.ReadAsStringAsync();

            Assert.Contains("\"@odata.count\":15,", responseStr);
        }

        [Fact]
        public async Task GetCount_Throws_DollarCountNotAllowed()
        {
            // Arrange
            string uri = "DollarCountEntities(5)/DollarCountNotAllowedCollectionProp/$count";
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(uri);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            Assert.Contains(
                "The query specified in the URI is not valid. Query option 'Count' is not allowed. " +
                "To allow it, set the 'AllowedQueryOptions' property on EnableQueryAttribute or QueryValidationSettings.",
                result);
        }

        public static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var entityCollection = builder.EntitySet<DollarCountEntity>("DollarCountEntities").EntityType.Collection;

            // Add unbound functions that return collection.
            FunctionConfiguration function = builder.Function("UnboundFunctionReturnsPrimitveCollection");
            function.IsComposable = true;
            function.ReturnsCollection<int>();

            function = builder.Function("UnboundFunctionReturnsEnumCollection");
            function.IsComposable = true;
            function.ReturnsCollection<DollarColor>();

            function = builder.Function("UnboundFunctionReturnsDateTimeOffsetCollection");
            function.IsComposable = true;
            function.ReturnsCollection<DateTimeOffset>();

            function = builder.Function("UnboundFunctionReturnsDateCollection");
            function.IsComposable = true;
            function.ReturnsCollection<Date>();

            function = builder.Function("UnboundFunctionReturnsComplexCollection");
            function.IsComposable = true;
            function.ReturnsCollection<DollarCountComplex>();

            function = builder.Function("UnboundFunctionReturnsEntityCollection");
            function.IsComposable = true;
            function.ReturnsCollectionFromEntitySet<DollarCountEntity>("DollarCountEntities");

            // Add bound functions that return collection.
            function = entityCollection.Function("BoundFunctionReturnsPrimitveCollection");
            function.IsComposable = true;
            function.ReturnsCollection<DateTimeOffset>();

            function = entityCollection.Function("BoundFunctionReturnsEnumCollection");
            function.IsComposable = true;
            function.ReturnsCollection<DollarColor>();

            function = entityCollection.Function("BoundFunctionReturnsDateTimeOffsetCollection");
            function.IsComposable = true;
            function.ReturnsCollection<DateTimeOffset>();

            function = entityCollection.Function("BoundFunctionReturnsComplexCollection");
            function.IsComposable = true;
            function.ReturnsCollection<DollarCountComplex>();

            function = entityCollection.Function("BoundFunctionReturnsEntityCollection");
            function.IsComposable = true;
            function.ReturnsCollectionFromEntitySet<DollarCountEntity>("DollarCountEntities");

            return builder.GetEdmModel();
        }

    }

}