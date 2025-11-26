//-----------------------------------------------------------------------------
// <copyright file="MetadataEndpointsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.MinimalApis;

public class MetadataEndpointsTests : IClassFixture<MinimalTestFixture<MetadataEndpointsTests>>
{
    private readonly MinimalTestFixture<MetadataEndpointsTests> _factory;

    public MetadataEndpointsTests(MinimalTestFixture<MetadataEndpointsTests> factory)
    {
        _factory = factory;
    }

    protected static void ConfigureAPIs(WebApplication app)
    {
        IEdmModel model = MinimalEdmModel.GetEdmModel();

        app.MapODataServiceDocument("v1/$document", model);

        app.MapODataServiceDocument("v2/$document", model)
            .WithODataBaseAddressFactory(h => new Uri("http://localhost/v2"));

        app.MapODataMetadata("v1/$metadata", model);
    }

    [Fact]
    public async Task ServiceDocumentEndpointV1_ShouldReturn_CorrectJsonResult()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync("/v1/$document");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var content = await result.Content.ReadAsStringAsync();

        Assert.Equal("{\"@odata.context\":\"http://localhost/$metadata\"," +
            "\"value\":[" +
              "{\"name\":\"Todos\",\"kind\":\"EntitySet\",\"url\":\"Todos\"}" +
            "]}",
            content);
    }

    [Fact]
    public async Task ServiceDocumentEndpointV2_ShouldReturn_CorrectJsonResult()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync("/v2/$document");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var content = await result.Content.ReadAsStringAsync();

        Assert.Equal("{\"@odata.context\":\"http://localhost/v2/$metadata\"," +
            "\"value\":[" +
              "{\"name\":\"Todos\",\"kind\":\"EntitySet\",\"url\":\"Todos\"}" +
            "]}",
            content);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("?$format=application/xml")]
    public async Task MetadataEndpointForCsdlXml_ShouldReturn_CorrectXmlResult(string query)
    {
        // Arrange
        HttpClient client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync($"/v1/$metadata{query}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var content = await result.Content.ReadAsStringAsync();

        Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<edmx:Edmx Version=\"4.0\" xmlns:edmx=\"http://docs.oasis-open.org/odata/ns/edmx\">" +
              "<edmx:DataServices>" +
                "<Schema Namespace=\"Microsoft.AspNetCore.OData.E2E.Tests.MinimalApis\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">" +
                  "<EntityType Name=\"MiniTodo\">" +
                    "<Key>" +
                      "<PropertyRef Name=\"Id\" />" +
                    "</Key>" +
                    "<Property Name=\"Id\" Type=\"Edm.Int32\" Nullable=\"false\" />" +
                    "<Property Name=\"Owner\" Type=\"Edm.String\" />" +
                    "<Property Name=\"Title\" Type=\"Edm.String\" />" +
                    "<Property Name=\"IsDone\" Type=\"Edm.Boolean\" Nullable=\"false\" />" +
                    "<Property Name=\"Tasks\" Type=\"Collection(Microsoft.AspNetCore.OData.E2E.Tests.MinimalApis.MiniTask)\" />" +
                  "</EntityType>" +
                  "<ComplexType Name=\"MiniTask\">" +
                    "<Property Name=\"Id\" Type=\"Edm.Int32\" Nullable=\"false\" />" +
                    "<Property Name=\"Description\" Type=\"Edm.String\" />" +
                    "<Property Name=\"Created\" Type=\"Edm.DateOnly\" Nullable=\"false\" />" +
                    "<Property Name=\"IsComplete\" Type=\"Edm.Boolean\" Nullable=\"false\" />" +
                    "<Property Name=\"Priority\" Type=\"Edm.Int32\" Nullable=\"false\" />" +
                  "</ComplexType>" +
                "</Schema>" +
                "<Schema Namespace=\"Default\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">" +
                  "<EntityContainer Name=\"Container\">" +
                    "<EntitySet Name=\"Todos\" EntityType=\"Microsoft.AspNetCore.OData.E2E.Tests.MinimalApis.MiniTodo\" />" +
                  "</EntityContainer>" +
                "</Schema>" +
              "</edmx:DataServices>" +
            "</edmx:Edmx>", content);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("?$format=application/json")]
    public async Task MetadataEndpointForCsdlJson_ShouldReturn_CorrectJsonResult(string query)
    {
        // Arrange
        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response;
        if (query == null)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "/v1/$metadata");
            httpRequestMessage.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            response = await client.SendAsync(httpRequestMessage);
        }
        else
        {
            response = await client.GetAsync($"/v1/$metadata{query}");
        }

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(@"{
  ""$Version"": ""4.0"",
  ""$EntityContainer"": ""Default.Container"",
  ""Microsoft.AspNetCore.OData.E2E.Tests.MinimalApis"": {
    ""MiniTodo"": {
      ""$Kind"": ""EntityType"",
      ""$Key"": [
        ""Id""
      ],
      ""Id"": {
        ""$Type"": ""Edm.Int32""
      },
      ""Owner"": {
        ""$Nullable"": true
      },
      ""Title"": {
        ""$Nullable"": true
      },
      ""IsDone"": {
        ""$Type"": ""Edm.Boolean""
      },
      ""Tasks"": {
        ""$Collection"": true,
        ""$Type"": ""Microsoft.AspNetCore.OData.E2E.Tests.MinimalApis.MiniTask"",
        ""$Nullable"": true
      }
    },
    ""MiniTask"": {
      ""$Kind"": ""ComplexType"",
      ""Id"": {
        ""$Type"": ""Edm.Int32""
      },
      ""Description"": {
        ""$Nullable"": true
      },
      ""Created"": {
        ""$Type"": ""Edm.DateOnly""
      },
      ""IsComplete"": {
        ""$Type"": ""Edm.Boolean""
      },
      ""Priority"": {
        ""$Type"": ""Edm.Int32""
      }
    }
  },
  ""Default"": {
    ""Container"": {
      ""$Kind"": ""EntityContainer"",
      ""Todos"": {
        ""$Collection"": true,
        ""$Type"": ""Microsoft.AspNetCore.OData.E2E.Tests.MinimalApis.MiniTodo""
      }
    }
  }
}", content);
    }
}
