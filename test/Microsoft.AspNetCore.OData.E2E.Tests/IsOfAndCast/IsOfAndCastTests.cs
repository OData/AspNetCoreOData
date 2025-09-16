//-----------------------------------------------------------------------------
// <copyright file="IsOfAndCastTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast
{
    public class IsOfAndCastTests : WebApiTestBase<IsOfAndCastTests>
    {
        public IsOfAndCastTests(WebApiTestFixture<IsOfAndCastTests> fixture) : base(fixture) { }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            IEdmModel model = IsOfAndCastEdmModel.GetEdmModel();

            services.ConfigureControllers(
                typeof(IsOfAndCastController));

            services.AddControllers().AddOData(opt => 
                opt.AddRouteComponents("odata", model).Count().Filter().OrderBy().Expand().SetMaxTop(null).Select());
        }

        [Theory]
        [InlineData("cast(Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.JetPlane) ne null")]
        [InlineData("cast('Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.JetPlane') ne null")]
        [InlineData("isof(Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.JetPlane)")]
        [InlineData("isof('Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.JetPlane')")]
        public async Task Filter_ReturnsOnlyEntitiesOfDerivedType_WhenUsingCastOrIsOf(string filter)
        {
            // Arrange
            var requestUri = $"odata/Products?$filter={filter}";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);
            var payload = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var expectedJson = """
{
  "@odata.context": "http://localhost/odata/$metadata#Products",
  "value": [
    {
      "@odata.type": "#Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.JetPlane",
      "ID": 4,
      "Name": "Product4",
      "Domain": "Civil",
      "Weight": 1200.0,
      "Speed": 1000,
      "Model": "Airbus A320",
      "JetType": "Turbofan"
    },
    {
      "@odata.type": "#Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.JetPlane",
      "ID": 5,
      "Name": "Product5",
      "Domain": "Military",
      "Weight": 1800.0,
      "Speed": 1500,
      "Model": "F-22 Raptor",
      "JetType": "Afterburning Turbofan"
    }
  ]
}
""";
            using var expectedDoc = JsonDocument.Parse(expectedJson);
            using var actualDoc = JsonDocument.Parse(payload);

            Assert.Equal(expectedDoc.ToString(), actualDoc.ToString());
        }


        [Theory]
        [InlineData("expand=Products(filter=cast(Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.JetPlane) ne null)")]
        [InlineData("expand=Products(filter=cast('Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.JetPlane') ne null)")]
        [InlineData("expand=Products(filter=isof(Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.JetPlane))")]
        [InlineData("expand=Products(filter=isof('Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.JetPlane'))")]
        public async Task ExpandNavigationProperty_FiltersForDerivedTypeEntities_UsingCastOrIsOf(string query)
        {
            // Arrange
            var requestUri = $"odata/orders?{query}";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);
            var payload = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var expectedJson = """
{
  "@odata.context": "http://localhost/odata/$metadata#Orders(Products())",
  "value": [
    { "OrderID": 1, "Location": { "City": "City1" }, "Products": [] },
    {
      "OrderID": 2,
      "Location": {
        "@odata.type": "#Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.HomeAddress",
        "City": "City2",
        "HomeNo": "100NO"
      },
      "Products": [
        {
          "@odata.type": "#Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.JetPlane",
          "ID": 4,
          "Name": "Product4",
          "Domain": "Civil",
          "Weight": 1200.0,
          "Speed": 1000,
          "Model": "Airbus A320",
          "JetType": "Turbofan"
        },
        {
          "@odata.type": "#Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.JetPlane",
          "ID": 5,
          "Name": "Product5",
          "Domain": "Military",
          "Weight": 1800.0,
          "Speed": 1500,
          "Model": "F-22 Raptor",
          "JetType": "Afterburning Turbofan"
        }
      ]
    },
    {
      "OrderID": 3,
      "Location": { "City": "City3" },
      "Products": [
        {
          "@odata.type": "#Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.JetPlane",
          "ID": 4,
          "Name": "Product4",
          "Domain": "Civil",
          "Weight": 1200.0,
          "Speed": 1000,
          "Model": "Airbus A320",
          "JetType": "Turbofan"
        }
      ]
    },
    {
      "OrderID": 4,
      "Location": {
        "@odata.type": "#Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.HomeAddress",
        "City": "City4",
        "HomeNo": "200NO"
      },
      "Products": [
        {
          "@odata.type": "#Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.JetPlane",
          "ID": 5,
          "Name": "Product5",
          "Domain": "Military",
          "Weight": 1800.0,
          "Speed": 1500,
          "Model": "F-22 Raptor",
          "JetType": "Afterburning Turbofan"
        }
      ]
    }
  ]
}
""";
            using var expectedDoc = JsonDocument.Parse(expectedJson);
            using var actualDoc = JsonDocument.Parse(payload);

            Assert.Equal(expectedDoc.ToString(), actualDoc.ToString());
        }

        [Theory]
        [InlineData("filter=cast(Location, Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.HomeAddress) ne null")]
        [InlineData("filter=cast(Location, 'Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.HomeAddress') ne null")]
        [InlineData("filter=isof(Location, Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.HomeAddress)")]
        [InlineData("filter=isof(Location, 'Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.HomeAddress')")]
        public async Task Filter_ReturnsEntitiesWithComplexPropertyOfDerivedType_UsingCastOrIsOf(string query)
        {
            // Arrange
            var requestUri = $"odata/orders?{query}";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);
            var payload = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var expectedJson = """
{
  "@odata.context": "http://localhost/odata/$metadata#Orders",
  "value": [
    {
      "OrderID": 2,
      "Location": {
        "@odata.type": "#Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.HomeAddress",
        "City": "City2",
        "HomeNo": "100NO"
      }
    },
    {
      "OrderID": 4,
      "Location": {
        "@odata.type": "#Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.HomeAddress",
        "City": "City4",
        "HomeNo": "200NO"
      }
    }
  ]
}
""";
            using var expectedDoc = JsonDocument.Parse(expectedJson);
            using var actualDoc = JsonDocument.Parse(payload);

            Assert.Equal(expectedDoc.ToString(), actualDoc.ToString());
        }

        [Theory]
        [InlineData("filter=cast(Location, Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.HomeAddress)/HomeNo eq '100NO'")]
        [InlineData("filter=cast(Location, 'Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.HomeAddress')/HomeNo eq '100NO'")]
        [InlineData("filter=isof(Location, Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.HomeAddress)&Location/HomeNo eq '100NO'")]
        [InlineData("filter=isof(Location, 'Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.HomeAddress')&Location/HomeNo eq '100NO'")]
        public async Task Filter_ReturnsEntitiesWithComplexPropertyValueMatch_UsingCastOrIsOf(string query)
        {
            // Arrange
            var requestUri = $"odata/orders?{query}";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);
            var payload = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var expectedJson = """
{
  "@odata.context": "http://localhost/odata/$metadata#Orders",
  "value": [
    {
      "OrderID": 2,
      "Location": {
        "@odata.type": "#Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.HomeAddress",
        "City": "City2",
        "HomeNo": "100NO"
      }
    }
  ]
}
""";
            using var expectedDoc = JsonDocument.Parse(expectedJson);
            using var actualDoc = JsonDocument.Parse(payload);

            Assert.Equal(expectedDoc.ToString(), actualDoc.ToString());
        }

        [Theory]
        [InlineData("filter=cast(Location, Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.HomeAddress)/HomeNo eq '1NO'")]
        [InlineData("filter=cast(Location, 'Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.HomeAddress')/HomeNo eq '1NO'")]
        [InlineData("filter=isof(Location, Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.HomeAddress) and cast(Location, Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.HomeAddress)/HomeNo eq '1NO'")]
        [InlineData("filter=isof(Location, 'Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.HomeAddress') and cast(Location, 'Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.HomeAddress')/HomeNo eq '1NO'")]
        public async Task Filter_ReturnsNoEntitiesWithNonMatchingComplexPropertyValue_UsingCastOrIsOf(string query)
        {
            // Arrange
            var requestUri = $"odata/orders?{query}";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);
            var payload = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var expectedJson = """
{
  "@odata.context": "http://localhost/odata/$metadata#Orders",
  "value": []
}
""";
            using var expectedDoc = JsonDocument.Parse(expectedJson);
            using var actualDoc = JsonDocument.Parse(payload);

            Assert.Equal(expectedDoc.ToString(), actualDoc.ToString());
        }

        [Theory]
        [InlineData("cast(Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.HomeAddress) ne null", 
            "Encountered invalid type cast. 'Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.HomeAddress' is not assignable from 'Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.Product'.")]
        [InlineData("cast('System.String') ne null", 
            "Cast or IsOf Function must have a type in its arguments.")]
        [InlineData("cast(Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.UnknownType) ne null", 
            "The child type 'Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.UnknownType' in a cast was not an entity type. Casts can only be performed on entity types.")]
        public async Task Filter_FailsWithInvalidTypeInCast(string filter, string errorMessage)
        {
            // Arrange
            var requestUri = $"odata/Products?$filter={filter}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);
            var payload = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Contains(errorMessage, payload);
        }

        [Theory]
        [InlineData("isof(Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.HomeAddress)",
            "Encountered invalid type cast. 'Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.HomeAddress' is not assignable from 'Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.Product'.")]
        [InlineData("isof('System.String')", 
            "Cast or IsOf Function must have a type in its arguments.")]
        [InlineData("isof(Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.UnknownType)", 
            "The child type 'Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast.UnknownType' in a cast was not an entity type. Casts can only be performed on entity types.")]
        public async Task Filter_FailsWithInvalidTypeInIsOf(string filter, string errorMessage)
        {
            // Arrange
            var requestUri = $"odata/Products?$filter={filter}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);
            var payload = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Contains(errorMessage, payload);
        }
    }
}
