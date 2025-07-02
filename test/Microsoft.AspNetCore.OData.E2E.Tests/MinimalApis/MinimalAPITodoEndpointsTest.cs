//-----------------------------------------------------------------------------
// <copyright file="MinimalAPITodoEndpointsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.MinimalApis;

public class MinimalAPITodoEndpointsTest : IClassFixture<MinimalTestFixture<MinimalAPITodoEndpointsTest>>
{
    private MinimalTestFixture<MinimalAPITodoEndpointsTest> _factory;
    private HttpClient _client;

    public MinimalAPITodoEndpointsTest(MinimalTestFixture<MinimalAPITodoEndpointsTest> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    protected static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IMiniTodoTaskRepository, MiniTodoTaskInMemoryRepository>();
    }

    protected static void ConfigureAPIs(WebApplication app)
    {
        app.MapGet("v0/todos", (IMiniTodoTaskRepository db) => db.GetTodos());

        app.MapGet("v1/todos", (IMiniTodoTaskRepository db) => db.GetTodos())
            .WithODataResult();

        IEdmModel model = MinimalEdmModel.GetEdmModel();

        app.MapGet("v0/todos/{id}", (IMiniTodoTaskRepository db, int id) => db.GetTodo(id));

        app.MapGet("v1/todos/{id}", (IMiniTodoTaskRepository db, int id) => db.GetTodo(id))
            .WithODataResult()
            .WithODataModel(model);

        // Use ODataQueryOptions<T>
        app.MapGet("v2/todos", (IMiniTodoTaskRepository db, ODataQueryOptions<MiniTodo> queryOptions)
            => queryOptions.ApplyTo(db.GetTodos().AsQueryable(), new ODataQuerySettings()))
            .WithODataResult()
            .WithODataModel(model);

        // Use Filter
        app.MapGet("v2/todos/{id}", (IMiniTodoTaskRepository db, int id) => db.GetTodo(id))
            .AddODataQueryEndpointFilter()
            .WithODataResult()
            .WithODataModel(model)
            .WithODataVersion(Microsoft.OData.ODataVersion.V401)
            .WithODataBaseAddressFactory(h => new Uri("http://localhost/v2"))
            .WithODataOptions(opt => opt.EnableAll().SetCaseInsensitive(true));

        // DeltaSet<T> endpoint
        app.MapPatch("v2/todos", (IMiniTodoTaskRepository db, DeltaSet<MiniTodo> changes) => $"Patch : '{changes.Count}' to todos")
            .WithODataResult()
            .WithODataModel(model)
            .WithODataVersion(Microsoft.OData.ODataVersion.V401)
            .WithODataBaseAddressFactory(h => new Uri("http://localhost/v2"))
            .WithODataPathFactory(
             (h, t) =>
             {
                 IEdmEntitySet todos = model.FindDeclaredEntitySet("Todos");
                 return new ODataPath(new EntitySetSegment(todos));
             });
    }

    [Fact]
    public async Task QueryTodos_ReturnsNormalJsonPayload()
    {
        // Arrange & Act
        var result = await _client.GetAsync("/v0/todos");
        var content = await result.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("[{\"id\":1,\"owner\":\"Peter\",\"title\":\"Cooking\",\"isDone\":false,\"tasks\":[{\"id\":11,\"description\":\"Boil Rice\",\"created\":\"2021-04-22\",\"isComplete\":true,\"priority\":1},{\"id\":12,\"description\":\"Cook Potate\",\"created\":\"2022-04-22\",\"isComplete\":false,\"priority\":1},{\"id\":13,\"description\":\"Cook Pizza\",\"created\":\"2021-04-22\",\"isComplete\":false,\"priority\":2}]}," +
            "{\"id\":2,\"owner\":\"Wu\",\"title\":\"English Practice\",\"isDone\":true,\"tasks\":[{\"id\":21,\"description\":\"Read English book\",\"created\":\"2024-02-11\",\"isComplete\":true,\"priority\":1},{\"id\":22,\"description\":\"Watch video\",\"created\":\"2022-03-04\",\"isComplete\":false,\"priority\":2}]}," +
            "{\"id\":3,\"owner\":\"John\",\"title\":\"Shopping\",\"isDone\":true,\"tasks\":[{\"id\":31,\"description\":\"Buy bread\",\"created\":\"2022-02-11\",\"isComplete\":false,\"priority\":3},{\"id\":32,\"description\":\"Buy washing machine\",\"created\":\"2023-12-14\",\"isComplete\":true,\"priority\":2}]}," +
            "{\"id\":4,\"owner\":\"Sam\",\"title\":\"Clean House\",\"isDone\":false,\"tasks\":[{\"id\":41,\"description\":\"Clean carpet\",\"created\":\"2025-02-11\",\"isComplete\":false,\"priority\":2},{\"id\":42,\"description\":\"Clean bathroom\",\"created\":\"2025-12-14\",\"isComplete\":true,\"priority\":1}]}]", content);
    }

    [Fact]
    public async Task QueryTodos_WithODataResult_ReturnsODataJsonPayload()
    {
        // Arrange & Act
        var result = await _client.GetAsync("/v1/todos");
        var content = await result.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("{\"@odata.context\":\"http://localhost/$metadata#MiniTodo\"," +
            "\"value\":[" +
            "{\"Id\":1,\"Owner\":\"Peter\",\"Title\":\"Cooking\",\"IsDone\":false}," +
            "{\"Id\":2,\"Owner\":\"Wu\",\"Title\":\"English Practice\",\"IsDone\":true}," +
            "{\"Id\":3,\"Owner\":\"John\",\"Title\":\"Shopping\",\"IsDone\":true}," +
            "{\"Id\":4,\"Owner\":\"Sam\",\"Title\":\"Clean House\",\"IsDone\":false}" +
          "]}", content);
    }

    [Fact]
    public async Task QueryTodo_WithODataResult_WithModel_ReturnsODataJsonPayload()
    {
        // Arrange & Act
        var result = await _client.GetAsync("/v1/todos/3");
        var content = await result.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("{\"@odata.context\":\"http://localhost/$metadata#Todos/$entity\"," +
            "\"Id\":3," +
            "\"Owner\":\"John\"," +
            "\"Title\":\"Shopping\"," +
            "\"IsDone\":true," +
            "\"Tasks\":[" +
               "{\"Id\":31,\"Description\":\"Buy bread\",\"Created\":\"2022-02-11\",\"IsComplete\":false,\"Priority\":3}," +
               "{\"Id\":32,\"Description\":\"Buy washing machine\",\"Created\":\"2023-12-14\",\"IsComplete\":true,\"Priority\":2}]}",
            content);
    }

    [Fact]
    public async Task QueryTodos_WithODataResult_WithModel_UsingODataQuery_ReturnsODataJsonPayload()
    {
        // Arrange & Act
        var result = await _client.GetAsync("/v2/todos?$select=Owner,Tasks($select=Description)");
        var content = await result.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("{\"@odata.context\":\"http://localhost/$metadata#Todos(Owner,Tasks/Description)\"," +
            "\"value\":[" +
              "{\"Owner\":\"Peter\",\"Tasks\":[{\"Description\":\"Boil Rice\"},{\"Description\":\"Cook Potate\"},{\"Description\":\"Cook Pizza\"}]}," +
              "{\"Owner\":\"Wu\",\"Tasks\":[{\"Description\":\"Read English book\"},{\"Description\":\"Watch video\"}]}," +
              "{\"Owner\":\"John\",\"Tasks\":[{\"Description\":\"Buy bread\"},{\"Description\":\"Buy washing machine\"}]}," +
              "{\"Owner\":\"Sam\",\"Tasks\":[{\"Description\":\"Clean carpet\"},{\"Description\":\"Clean bathroom\"}]}]}",
            content);
    }

    [Fact]
    public async Task QueryTodos_WithODataResult_WithModel_UsingFilter_ReturnsODataJsonPayload()
    {
        // Arrange & Act
        var result = await _client.GetAsync("/v2/todos/4?$select=title,Tasks($select=Description;$orderby=id desc)");
        var content = await result.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("{\"@context\":\"http://localhost/v2/$metadata#Todos(Title,Tasks/Description)/$entity\"," +
            "\"Title\":\"Clean House\"," +
            "\"Tasks\":[{\"Description\":\"Clean bathroom\"},{\"Description\":\"Clean carpet\"}]}",
            content);
    }

    [Fact]
    public async Task PatchChangesToTodos_WithODataResult_WithModel_WithPath_ReturnsODataJsonPayload()
    {
        // Arrange & Act
        var payload = @"{
            '@context':'http://localhost/v2/$metadata#Todos/$delta',
            'value':[
                { '@odata.id': 'Todos(42)','Title':'No 42 Todo'},
                { '@odata.context': 'http://localhost/v2/$metadata#Todos/$deletedEntity', 'Id': 'Todos(12)', 'reason':'deleted'}
            ]}";
        
        StringContent stringContent = new StringContent(payload, Encoding.UTF8, "application/json");

        var result = await _client.PatchAsync("/v2/todos", stringContent);
        var content = await result.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("{\"@context\":\"http://localhost/v2/$metadata#Edm.String\",\"value\":\"Patch : '2' to todos\"}", content);
    }
}
