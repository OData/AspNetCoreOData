// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.E2E.Tests.DollarFormat;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.OData.E2E.Tests
{

    public class DollarFormatOverrideAcceptMediaTypeTests : WebApiTestBase<DollarFormatOverrideAcceptMediaTypeTests>
    {

        public DollarFormatOverrideAcceptMediaTypeTests(WebApiTestFixture<DollarFormatOverrideAcceptMediaTypeTests> fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
            ConfigureServicesAction = (services) =>
            {
                IEdmModel model = GetEdmModel();
                services.ConfigureControllers(typeof(DollarFormatCustomersController), typeof(MetadataController));
                services.AddControllers().AddOData(opt => opt.AddModel("odata", model).EnableODataQuery(null));
            };
        }

        public static TheoryDataSet<string> BasicMediaTypes
        {
            get
            {
                return DollarFormatWithoutAcceptMediaTypeTests.BasicMediaTypes;
            }
        }

        [Theory]
        [MemberData(nameof(BasicMediaTypes))]
        public async Task QueryEntitySetWithDollarFormatOverrideAcceptMediaTypeTests(string dollarFormat)
        {
            // Arrange
            string query = $"?$expand=SpecialOrder($select=Detail)&$filter=Id le 5&$orderby=Id desc&$select=Id&$format={dollarFormat}";
            string requestUri = $"odata/DollarFormatCustomers{query}";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);

            if (dollarFormat.ToLowerInvariant().Contains("odata.metadata"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.metadata"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            if (dollarFormat.ToLowerInvariant().Contains("odata.streaming"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.streaming"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            JObject jObj = await response.Content.ReadAsObject<JObject>();
            JArray value = jObj["value"] as JArray;
            Assert.Equal(6, value.Count);
        }

        [Theory]
        [MemberData(nameof(BasicMediaTypes))]
        public async Task QueryEntityWithDollarFormatOverrideAcceptMediaTypeTests(string dollarFormat)
        {
            // Arrange
            string requestUri = $"/odata/DollarFormatCustomers(1)?$format={dollarFormat}";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);

            if (dollarFormat.ToLowerInvariant().Contains("odata.metadata"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.metadata"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            if (dollarFormat.ToLowerInvariant().Contains("odata.streaming"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.streaming"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            JObject jObj = await response.Content.ReadAsObject<JObject>();
            Assert.Equal("Customer Name 1", jObj["Name"]);
        }

        [Theory]
        [MemberData(nameof(BasicMediaTypes))]
        public async Task QueryPropertyWithDollarFormatOverrideAcceptMediaTypeTests(string dollarFormat)
        {
            // Arrange
            string requestUri = $"odata/DollarFormatCustomers(1)?$select=Name&$format={dollarFormat}";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);

            if (dollarFormat.ToLowerInvariant().Contains("odata.metadata"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.metadata"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            if (dollarFormat.ToLowerInvariant().Contains("odata.streaming"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.streaming"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            JObject jObj = await response.Content.ReadAsObject<JObject>();
            Assert.Equal("Customer Name 1", jObj["Name"]);
        }

        [Theory]
        [MemberData(nameof(BasicMediaTypes))]
        public async Task QueryNavigationPropertyWithDollarFormatOverrideAcceptMediaTypeTests(string dollarFormat)
        {
            // Arrange
            string requestUri = $"odata/DollarFormatCustomers(1)?$select=SpecialOrder&$expand=SpecialOrder&$format={dollarFormat}";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);

            if (dollarFormat.ToLowerInvariant().Contains("odata.metadata"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.metadata"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            if (dollarFormat.ToLowerInvariant().Contains("odata.streaming"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.streaming"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            JObject jObj = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(10, jObj["SpecialOrder"]["Id"]);
        }

        [Theory]
        [MemberData(nameof(BasicMediaTypes))]
        public async Task QueryCollectionWithDollarFormatOverrideAcceptMediaTypeTests(string dollarFormat)
        {
            // Arrange
            string requestUri = $"odata/DollarFormatCustomers(2)?$select=Orders&$expand=Orders&$format={dollarFormat}";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);

            if (dollarFormat.ToLowerInvariant().Contains("odata.metadata"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.metadata"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            if (dollarFormat.ToLowerInvariant().Contains("odata.streaming"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.streaming"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            JObject jObj = await response.Content.ReadAsObject<JObject>();
            JArray orders = jObj["Orders"] as JArray;
            Assert.Equal(2, orders.Count);
        }

        [Theory]
        [InlineData("json")]
        [InlineData("application/json")]
        [InlineData("application/json;odata.metadata=none")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=none;odata.streaming=true")]
        [InlineData("application/json;odata.metadata=none;odata.streaming=false")]
        [InlineData("application/json;odata.metadata=minimal;odata.streaming=true")]
        [InlineData("application/json;odata.metadata=minimal;odata.streaming=false")]
        [InlineData("application/json;odata.metadata=full;odata.streaming=true")]
        [InlineData("application/json;odata.metadata=full;odata.streaming=false")]
        public async Task QueryServiceDocumentWithDollarFormatOverrideAcceptMediaTypeTests(string dollarFormat)
        {
            // Arrange
            string requestUri = $"odata?$format={dollarFormat}";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);

            if (dollarFormat.ToLowerInvariant().Contains("odata.metadata"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.metadata"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            if (dollarFormat.ToLowerInvariant().Contains("odata.streaming"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.streaming"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            JObject jObj = await response.Content.ReadAsObject<JObject>();
            JArray value = jObj["value"] as JArray;
            Assert.Equal(2, value.Count);
        }

        [Theory]
        [InlineData("xml")]
        [InlineData("application/xml")]
        [InlineData("json")]
        [InlineData("application/json")]
        public async Task QueryMetadataDocumentWithDollarFormatOverrideAcceptMediaTypeTests(string dollarFormat)
        {
            // Arrange
            string requestUri = $"odata/$metadata?$format={dollarFormat}";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml")); // accept for xml
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            string payload = await response.Content.ReadAsStringAsync();

            if (dollarFormat.Contains("xml"))
            {
                Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
                Assert.Contains("<edmx:Edmx Version=\"4.0\"", payload);
            }
            else
            {
                Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
                Assert.Contains("\"$Version\": \"4.0\",", payload);
            }
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<DollarFormatCustomer>("DollarFormatCustomers");
            builder.EntitySet<DollarFormatOrder>("DollarFormatOrders");
            return builder.GetEdmModel();
        }
    }
}