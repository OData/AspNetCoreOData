// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarFormat
{
    public class DollarFormatWithoutAcceptMediaTypeTests : WebApiTestBase<DollarFormatWithoutAcceptMediaTypeTests>
    {
        public DollarFormatWithoutAcceptMediaTypeTests(WebApiTestFixture<DollarFormatWithoutAcceptMediaTypeTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            IEdmModel model = GetEdmModel();
            services.ConfigureControllers(typeof(DollarFormatCustomersController), typeof(MetadataController));
            services.AddControllers().AddOData(opt => opt.AddRouteComponents("odata", model).Count().Filter().OrderBy().Expand().SetMaxTop(null).Select());
        }

        public static TheoryDataSet<string> BasicMediaTypes
        {
            get
            {
                var data = new TheoryDataSet<string>();

                data.Add(Uri.EscapeDataString("json"));
                data.Add(Uri.EscapeDataString("application/json"));
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=none"));
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=minimal"));
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=full"));
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=none;odata.streaming=true"));
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=none;odata.streaming=false"));
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=minimal;odata.streaming=true"));
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=minimal;odata.streaming=false"));
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=full;odata.streaming=true"));
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=full;odata.streaming=false"));

                data.Add(Uri.EscapeDataString("application/json;odata.streaming=true;odata.metadata=none"));
                data.Add(Uri.EscapeDataString("application/json;odata.streaming=false;odata.metadata=none"));
                data.Add(Uri.EscapeDataString("application/json;odata.streaming=true;odata.metadata=minimal"));
                data.Add(Uri.EscapeDataString("application/json;odata.streaming=false;odata.metadata=minimal"));
                data.Add(Uri.EscapeDataString("application/json;odata.streaming=true;odata.metadata=full"));
                data.Add(Uri.EscapeDataString("application/json;odata.streaming=false;odata.metadata=full"));

                data.Add(Uri.EscapeDataString("Json"));
                data.Add(Uri.EscapeDataString("jSoN"));
                data.Add(Uri.EscapeDataString("APPLICATION/JSON;ODATA.METADATA=NONE;odata.streaming=TRUE"));
                data.Add(Uri.EscapeDataString("aPpLiCaTiOn/JsOn;odata.streaming=tRuE;oDaTa.MeTaDaTa=NoNe"));

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(BasicMediaTypes))]
        public async Task QueryEntitySetWithDollarFormatWithoutAcceptMediaTypeTests(string dollarFormat)
        {
            // Arrange
            string requestUri = $"odata/DollarFormatCustomers?$expand=SpecialOrder($select=Detail)&$filter=Id le 5&$orderby=Id desc&$select=Id&$format={dollarFormat}";
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

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
        public async Task QueryEntityWithDollarFormatWithoutAcceptMediaTypeTests(string dollarFormat)
        {
            // Arrange
            string requestUri = $"odata/DollarFormatCustomers(1)?$format={dollarFormat}";
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

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
        public async Task QueryPropertyWithDollarFormatWithoutAcceptMediaTypeTests(string dollarFormat)
        {
            // Arrange
            string requestUri = $"odata/DollarFormatCustomers(1)?$select=Name&$format={dollarFormat}";
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

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
        public async Task QueryNavigationPropertyWithDollarFormatWithoutAcceptMediaTypeTests(string dollarFormat)
        {
            // Arrange
            string requestUri = $"odata/DollarFormatCustomers(1)?$select=SpecialOrder&$expand=SpecialOrder&$format={dollarFormat}";
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

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
        public async Task QueryCollectionWithDollarFormatWithoutAcceptMediaTypeTests(string dollarFormat)
        {
            // Arrange
            string requestUri = $"odata/DollarFormatCustomers(1)?$select=Orders&$expand=Orders&$format={dollarFormat}";
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

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
            Assert.Single(orders);
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
        public async Task QueryServiceDocumentWithDollarFormatWithoutAcceptMediaTypeTests(string dollarFormat)
        {
            // Arrange
            string requestUri = $"odata?$format={dollarFormat}";
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

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
        public async Task QueryMetadataDocumentWithDollarFormatWithoutAcceptMediaTypeTests(string dollarFormat)
        {
            // Arrange
            string lowerDollarFormat = dollarFormat.ToLowerInvariant();
            string requestUri = $"odata/$metadata?$format={dollarFormat}";
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            string payload = await response.Content.ReadAsStringAsync();
            if (lowerDollarFormat.Contains("xml"))
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