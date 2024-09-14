//-----------------------------------------------------------------------------
// <copyright file="SpatialTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.Commons;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Spatial;

public class SpatialTests : WebODataTestBase<SpatialTests.SpatialTestsStartup>
{
    public class SpatialTestsStartup : TestStartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(SpatialCustomersController), typeof(MetadataController));

            IEdmModel model = IsofEdmModel.GetEdmModel();
            services.AddControllers().AddOData(options => options.AddRouteComponents("odata", model));
        }
    }

    public SpatialTests(WebODataTestFixture<SpatialTestsStartup> factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task SpatialModelMetadataTest()
    {
        // Arrange & Act
        HttpResponseMessage response = await this.Client.GetAsync("odata/$metadata");

        // Assert
        var stream = await response.Content.ReadAsStreamAsync();
        IODataResponseMessage message = new ODataMessageWrapper(stream, response.Content.Headers);
        var reader = new ODataMessageReader(message);
        var edmModel = reader.ReadMetadataDocument();

        var customer = edmModel.SchemaElements.OfType<IEdmEntityType>().Single(et => et.Name == "SpatialCustomer");
        Assert.Equal(5, customer.Properties().Count());

        var locationProperty = customer.DeclaredProperties.Single(p => p.Name == "Location");
        Assert.Equal("Edm.GeographyPoint", locationProperty.Type.FullName());

        var regionProperty = customer.DeclaredProperties.Single(p => p.Name == "Region");
        Assert.Equal("Edm.GeographyLineString", regionProperty.Type.FullName());

        var homePointProperty = customer.DeclaredProperties.Single(p => p.Name == "HomePoint");
        Assert.Equal("Edm.GeometryPoint", homePointProperty.Type.FullName());
    }

    [Fact]
    public async Task QuerySpatialEntity()
    {
        // Arrange & Act
        HttpResponseMessage response = await Client.GetAsync("odata/SpatialCustomers(2)");
        JObject responseString = await response.Content.ReadAsObject<JObject>();

        // Assert
        Assert.True(HttpStatusCode.OK == response.StatusCode);

        JArray regionData = responseString["Region"]["coordinates"] as JArray;
        Assert.NotNull(regionData);
        Assert.Equal(2, regionData.Count);

        Assert.Equal(66.0, regionData[0][0]);
        Assert.Equal(55.0, regionData[0][1]);
        Assert.Equal(JValue.CreateNull(), regionData[0][2]);
        Assert.Equal(0.0, regionData[0][3]);

        Assert.Equal(44.0, regionData[1][0]);
        Assert.Equal(33, regionData[1][1]);
        Assert.Equal(JValue.CreateNull(), regionData[1][2]);
        Assert.Equal(12.3, regionData[1][3]);
    }

    [Fact]
    public async Task PostSpatialEntity()
    {
        // Arrange
        const string payload = @"{
  ""Location"":{
""type"":""Point"",""coordinates"":[
  7,8,9,10
],""crs"":{
  ""type"":""name"",""properties"":{
    ""name"":""EPSG:4326""
  }
}
  },""Region"":{
""type"":""LineString"",""coordinates"":[
  [
    1.0,1.0
  ],[
    3.0,3.0
  ],[
    4.0,4.0
  ],[
    0.0,0.0
  ]
],""crs"":{
  ""type"":""name"",""properties"":{
    ""name"":""EPSG:4326""
  }
}
  },
  ""Name"":""Sam"",
  ""HomePoint"": {
""type"": ""Point"",
""coordinates"": [
  4.0,
  10.0
],
""crs"": {
  ""type"": ""name"",
  ""properties"": {
    ""name"": ""EPSG:0""
  }
}
  }
}";

        // Act
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "odata/SpatialCustomers");
        request.Content = new StringContent(payload);
        request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
        request.Content.Headers.ContentLength = payload.Length;
        HttpResponseMessage response = await Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task UpdateSpatialEntity()
    {
        // Arrange
        const string payload = @"{
  ""Location"":{
""type"":""Point"",""coordinates"":[
  7,8,9,10
],""crs"":{
  ""type"":""name"",""properties"":{
    ""name"":""EPSG:4326""
  }
}
  }
}";

        // Act
        HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("Patch"), "odata/SpatialCustomers(3)");
        request.Content = new StringContent(payload);
        request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
        request.Content.Headers.ContentLength = payload.Length;
        HttpResponseMessage response = await Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
