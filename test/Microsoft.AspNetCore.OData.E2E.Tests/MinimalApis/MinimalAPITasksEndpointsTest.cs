//-----------------------------------------------------------------------------
// <copyright file="MinimalAPITasksEndpointsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.MinimalApis;

public class MinimalAPITasksEndpointsTest : IClassFixture<MinimalTestFixture<MinimalAPITasksEndpointsTest>>
{
    private readonly MinimalTestFixture<MinimalAPITasksEndpointsTest> _factory;

    public MinimalAPITasksEndpointsTest(MinimalTestFixture<MinimalAPITasksEndpointsTest> factory)
    {
        _factory = factory;
    }

    protected static void ConfigureServices(IServiceCollection services)
    {
        services.AddOData(opt => opt.EnableAll());// global configuration.
        services.AddSingleton<IMiniTodoTaskRepository, MiniTodoTaskInMemoryRepository>();
    }

    protected static void ConfigureAPIs(WebApplication app)
    {
        var model = MinimalEdmModel.GetAllEntitySetEdmModel();

        // Use Group to config
        var group = app.MapGroup("odata")
            .WithODataModel(model)
            .WithODataResult()
            .AddODataQueryEndpointFilter(querySetup: s => s.PageSize = 2);

        group.MapGet("tasks", (IMiniTodoTaskRepository db) => db.GetTasks());
    }

    [Fact]
    public async Task QueryTasks_WithODataOnGroup_ReturnsODataPayload()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync("/odata/tasks?$orderby=Created&$select=Description");
        var content = await result.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("{\"@odata.context\":\"http://localhost/$metadata#Tasks(Description)\"," +
            "\"value\":[" +
              "{\"Description\":\"Boil Rice\"}," +
              "{\"Description\":\"Cook Pizza\"}" +
            "]," +
            "\"@odata.nextLink\":\"http://localhost/odata/tasks?$orderby=Created&$select=Description&$skiptoken=Created-2021-04-22,Id-13\"}",
            content);
    }
}
