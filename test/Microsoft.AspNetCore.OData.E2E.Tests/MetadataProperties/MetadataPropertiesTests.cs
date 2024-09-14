//-----------------------------------------------------------------------------
// <copyright file="MetadataPropertiesTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.MetadataProperties.Core;
using Microsoft.AspNetCore.OData.E2E.Tests.MetadataProperties.Plant1;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.MetadataProperties;

public class MetadataPropertiesTests : WebApiTestBase<MetadataPropertiesTests>
{
    public MetadataPropertiesTests(WebApiTestFixture<MetadataPropertiesTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        var nonContainedNavPropInContainedNavSourceModel = MetadataPropertiesEdmModel.GetEdmModelWithNonContainedNavPropInContainedNavSource();
        var containedNavPropertyInContainedNavSourceModel = MetadataPropertiesEdmModel.GetEdmModelWithContainedNavPropInContainedNavSource();

        services.ConfigureControllers(typeof(SitesController));

        services.AddControllers().AddOData(
            options => options.EnableQueryFeatures()
            .AddRouteComponents(
                routePrefix: "NonContainedNavPropInContainedNavSource",
                model: nonContainedNavPropInContainedNavSourceModel)
            .AddRouteComponents(
                routePrefix: "ContainedNavPropInContainedNavSource",
                model: containedNavPropertyInContainedNavSourceModel));
    }

    [Theory]
    [InlineData("NonContainedNavPropInContainedNavSource", "Sites(1)/Plants(1)")]
    [InlineData("ContainedNavPropInContainedNavSource", "Sites(1)/Plants(1)/Pipelines(1)/Plant")]
    public async Task TestExpandPlantNavigationPropertyOnContainedNavigationSource(string routePrefix, string plantResourceBase)
    {
        // Arrange
        var requestUri = $"{routePrefix}/Sites(1)/Plants(1)/Pipelines(1)?$expand=Plant";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
        var client = CreateClient();
        var typeofPipeline = typeof(Pipeline);
        var typeofPlant = typeof(Plant);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsObject<JObject>();

        Assert.EndsWith($"{routePrefix}/$metadata#Sites(1)/Plants(1)/Pipelines/{typeofPipeline}(Plant())/$entity", result.Value<string>("@odata.context"));
        Assert.Equal($"#{typeofPipeline}", result.Value<string>("@odata.type"));
        Assert.EndsWith("Sites(1)/Plants(1)/Pipelines(1)", result.Value<string>("@odata.id"));
        Assert.EndsWith($"Sites(1)/Plants(1)/Pipelines(1)/{typeofPipeline}", result.Value<string>("@odata.editLink"));
        Assert.Equal(1, result.Value<int>("Id"));
        Assert.Equal("Pipeline 1", result.Value<string>("Name"));
        Assert.Equal(100, result.Value<int?>("Length"));
        Assert.EndsWith($"/Sites(1)/Plants(1)/Pipelines(1)/{typeofPipeline}/Plant/$ref", result.Value<string>("Plant@odata.associationLink"));
        Assert.EndsWith($"/Sites(1)/Plants(1)/Pipelines(1)/{typeofPipeline}/Plant", result.Value<string>("Plant@odata.navigationLink"));

        var plant = result.GetValue("Plant") as JObject;
        Assert.NotNull(plant);
        Assert.Equal($"#{typeofPlant}", plant.Value<string>("@odata.type"));
        Assert.EndsWith($"{plantResourceBase}", plant.Value<string>("@odata.id"));
        Assert.EndsWith($"{plantResourceBase}", plant.Value<string>("@odata.editLink"));
        Assert.Equal(1, plant.Value<int>("Id"));
        Assert.Equal("Plant 1", plant.Value<string>("Name"));
        Assert.EndsWith($"{routePrefix}/{plantResourceBase}/Site/$ref", plant.Value<string>("Site@odata.associationLink"));
        Assert.EndsWith($"{routePrefix}/{plantResourceBase}/Site", plant.Value<string>("Site@odata.navigationLink"));
        Assert.EndsWith($"{routePrefix}/{plantResourceBase}/Pipelines/$ref", plant.Value<string>("Pipelines@odata.associationLink"));
        Assert.EndsWith($"{routePrefix}/{plantResourceBase}/Pipelines", plant.Value<string>("Pipelines@odata.navigationLink"));
    }

    [Theory]
    [InlineData("NonContainedNavPropInContainedNavSource", "Sites(1)")]
    [InlineData("ContainedNavPropInContainedNavSource", "Sites(1)/Plants(1)/Site")]
    public async Task TestExpandSiteNavigationPropertyOnContainedNavigationSource(string routePrefix, string siteResourceBase)
    {
        // Arrange
        var requestUri = $"{routePrefix}/Sites(1)/Plants(1)?$expand=Site";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
        var client = CreateClient();
        var typeofPlant = typeof(Plant);
        var typeofSite = typeof(Site);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsObject<JObject>();

        Assert.EndsWith($"{routePrefix}/$metadata#Sites(1)/Plants(Site())/$entity", result.Value<string>("@odata.context"));
        Assert.Equal($"#{typeofPlant}", result.Value<string>("@odata.type"));
        Assert.EndsWith("Sites(1)/Plants(1)", result.Value<string>("@odata.id"));
        Assert.EndsWith("Sites(1)/Plants(1)", result.Value<string>("@odata.editLink"));
        Assert.Equal(1, result.Value<int>("Id"));
        Assert.Equal("Plant 1", result.Value<string>("Name"));
        Assert.EndsWith($"{routePrefix}/Sites(1)/Plants(1)/Pipelines/$ref", result.Value<string>("Pipelines@odata.associationLink"));
        Assert.EndsWith($"{routePrefix}/Sites(1)/Plants(1)/Pipelines", result.Value<string>("Pipelines@odata.navigationLink"));
        Assert.EndsWith($"{routePrefix}/Sites(1)/Plants(1)/Site/$ref", result.Value<string>("Site@odata.associationLink"));
        Assert.EndsWith($"{routePrefix}/Sites(1)/Plants(1)/Site", result.Value<string>("Site@odata.navigationLink"));

        var site = result.GetValue("Site") as JObject;
        Assert.NotNull(site);
        Assert.Equal($"#{typeofSite}", site.Value<string>("@odata.type"));
        Assert.EndsWith($"{siteResourceBase}", site.Value<string>("@odata.id"));
        Assert.EndsWith($"{siteResourceBase}", site.Value<string>("@odata.editLink"));
        Assert.Equal(1, site.Value<int>("Id"));
        Assert.Equal("Site 1", site.Value<string>("Name"));
        Assert.EndsWith($"{routePrefix}/{siteResourceBase}/Plants/$ref", site.Value<string>("Plants@odata.associationLink"));
        Assert.EndsWith($"{routePrefix}/{siteResourceBase}/Plants", site.Value<string>("Plants@odata.navigationLink"));
    }

    [Theory]
    [InlineData("NonContainedNavPropInContainedNavSource", "Sites(1)/Plants(2)")]
    [InlineData("ContainedNavPropInContainedNavSource", "Sites(1)/Plants(2)/Pipelines({0})/Plant")]
    public async Task TestExpandPipelinesNavigationPropertyOnContainedNavigationSource(string routePrefix, string plantResourceBase)
    {
        // Arrange
        var requestUri = $"{routePrefix}/Sites(1)/Plants(2)?$expand=Pipelines($expand=Plant)";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsObject<JObject>();

        Action<JObject, string, string> verifyPlantAction = (plant, localRoutePrefix, localPlantResourceBase) =>
        {
            Assert.NotNull(plant);
            Assert.Equal($"#{typeof(Plant)}", plant.Value<string>("@odata.type"));
            Assert.EndsWith($"{localPlantResourceBase}", plant.Value<string>("@odata.id"));
            Assert.EndsWith($"{localPlantResourceBase}", plant.Value<string>("@odata.editLink"));
            Assert.Equal(2, plant.Value<int>("Id"));
            Assert.Equal($"Plant 2", plant.Value<string>("Name"));
            Assert.EndsWith($"{localRoutePrefix}/{localPlantResourceBase}/Pipelines/$ref", plant.Value<string>("Pipelines@odata.associationLink"));
            Assert.EndsWith($"{localRoutePrefix}/{localPlantResourceBase}/Pipelines", plant.Value<string>("Pipelines@odata.navigationLink"));
            Assert.EndsWith($"{localRoutePrefix}/{localPlantResourceBase}/Site/$ref", plant.Value<string>("Site@odata.associationLink"));
            Assert.EndsWith($"{localRoutePrefix}/{localPlantResourceBase}/Site", plant.Value<string>("Site@odata.navigationLink"));
        };

        verifyPlantAction(result, routePrefix, "Sites(1)/Plants(2)");

        var pipelines = result.GetValue("Pipelines") as JArray;
        Assert.NotNull(pipelines);
        Assert.Equal(2, pipelines.Count);

        var pipelineAt0 = pipelines[0] as JObject;
        var pipelineAt1 = pipelines[1] as JObject;

        Action<JObject, string, int> verifyPipelineAction = (pipeline, localRoutePrefix, pipelineId) =>
        {
            var typeofPipeline = typeof(Pipeline);

            Assert.NotNull(pipeline);
            Assert.Equal($"#{typeofPipeline}", pipeline.Value<string>("@odata.type"));
            Assert.EndsWith($"Sites(1)/Plants(2)/Pipelines({pipelineId})", pipeline.Value<string>("@odata.id"));
            Assert.EndsWith($"Sites(1)/Plants(2)/Pipelines({pipelineId})/{typeofPipeline}", pipeline.Value<string>("@odata.editLink"));
            Assert.Equal(pipelineId, pipeline.Value<int>("Id"));
            Assert.Equal($"Pipeline {pipelineId}", pipeline.Value<string>("Name"));
            Assert.Equal(pipelineId * 100, pipeline.Value<int?>("Length"));
            Assert.EndsWith($"/Sites(1)/Plants(2)/Pipelines({pipelineId})/{typeofPipeline}/Plant/$ref", pipeline.Value<string>("Plant@odata.associationLink"));
            Assert.EndsWith($"/Sites(1)/Plants(2)/Pipelines({pipelineId})/{typeofPipeline}/Plant", pipeline.Value<string>("Plant@odata.navigationLink"));

            var plant = pipeline.GetValue("Plant") as JObject;
            verifyPlantAction(plant, localRoutePrefix, string.Format(plantResourceBase, pipelineId));
        };

        verifyPipelineAction(pipelineAt0, routePrefix, 3); // Pipeline Id = 3
        verifyPipelineAction(pipelineAt1, routePrefix, 4); // Pipeline Id = 4
    }

    [Theory]
    [InlineData("NonContainedNavPropInContainedNavSource", "Sites(2)")]
    [InlineData("ContainedNavPropInContainedNavSource", "Sites(2)/Plants({0})/Site")]
    public async Task TestExpandPlantsNavigationPropertyOnNonContainedNavigationSource(string routePrefix, string siteResourceBase)
    {
        // Arrange
        var requestUri = $"{routePrefix}/Sites(2)?$expand=Plants($expand=Site)";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsObject<JObject>();

        Action<JObject, string, string> verifySiteAction = (site, localRoutePrefix, localSiteResourceBase) =>
        {
            Assert.Equal($"#{typeof(Site)}", site.Value<string>("@odata.type"));
            Assert.EndsWith($"{localSiteResourceBase}", site.Value<string>("@odata.id"));
            Assert.EndsWith($"{localSiteResourceBase}", site.Value<string>("@odata.editLink"));
            Assert.Equal(2, site.Value<int>("Id"));
            Assert.Equal("Site 2", site.Value<string>("Name"));
            Assert.EndsWith($"{localRoutePrefix}/{localSiteResourceBase}/Plants/$ref", site.Value<string>("Plants@odata.associationLink"));
            Assert.EndsWith($"{localRoutePrefix}/{localSiteResourceBase}/Plants", site.Value<string>("Plants@odata.navigationLink"));
        };

        verifySiteAction(result, routePrefix, "Sites(2)");

        var plants = result.GetValue("Plants") as JArray;
        Assert.NotNull(plants);
        Assert.Equal(2, plants.Count);

        var plantAt0 = plants[0] as JObject;
        var plantAt1 = plants[1] as JObject;

        Action<JObject, string, int> verifyPlantAction = (plant, localRoutePrefix, plantId) =>
        {
            Assert.NotNull(plant);
            Assert.Equal($"#{typeof(Plant)}", plant.Value<string>("@odata.type"));
            Assert.EndsWith($"Sites(2)/Plants({plantId})", plant.Value<string>("@odata.id"));
            Assert.EndsWith($"Sites(2)/Plants({plantId})", plant.Value<string>("@odata.editLink"));
            Assert.Equal(plantId, plant.Value<int>("Id"));
            Assert.Equal($"Plant {plantId}", plant.Value<string>("Name"));
            Assert.EndsWith($"{localRoutePrefix}/Sites(2)/Plants({plantId})/Pipelines/$ref", plant.Value<string>("Pipelines@odata.associationLink"));
            Assert.EndsWith($"{localRoutePrefix}/Sites(2)/Plants({plantId})/Pipelines", plant.Value<string>("Pipelines@odata.navigationLink"));
            Assert.EndsWith($"{localRoutePrefix}/Sites(2)/Plants({plantId})/Site/$ref", plant.Value<string>("Site@odata.associationLink"));
            Assert.EndsWith($"{localRoutePrefix}/Sites(2)/Plants({plantId})/Site", plant.Value<string>("Site@odata.navigationLink"));

            var site = plant.GetValue("Site") as JObject;
            verifySiteAction(site, localRoutePrefix, string.Format(siteResourceBase, plantId));
        };

        verifyPlantAction(plantAt0, routePrefix, 3); // Plant Id = 3
        verifyPlantAction(plantAt1, routePrefix, 4); // Plant Id = 4
    }
}
