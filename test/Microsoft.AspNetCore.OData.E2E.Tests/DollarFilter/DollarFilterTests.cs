//-----------------------------------------------------------------------------
// <copyright file="DollarFilterTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter;

public class DollarFilterTests : WebApiTestBase<DollarFilterTests>
{
    public DollarFilterTests(WebApiTestFixture<DollarFilterTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        IEdmModel model = DollarFilterEdmModel.GetEdmModel();

        services.ConfigureControllers(
            typeof(PeopleController),
            typeof(ProductsController),
            typeof(CustomersController));

        services.AddControllers().AddOData(opt =>
            opt.Filter().Select().AddRouteComponents("odata", model));
    }

    [Theory]
    [InlineData("('a''bc')", "[{\"Id\":1,\"SSN\":\"a'bc\"}]")]
    [InlineData("('''def')", "[{\"Id\":2,\"SSN\":\"'def\"}]")]
    [InlineData("('xyz''')", "[{\"Id\":3,\"SSN\":\"xyz'\"}]")]
    [InlineData("('''pqr''')", "[{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('a''bc','''def')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"}]")]
    [InlineData("('a''bc','xyz''')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":3,\"SSN\":\"xyz'\"}]")]
    [InlineData("('a''bc','''pqr''')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''def','a''bc')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"}]")]
    [InlineData("('''def','xyz''')", "[{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"}]")]
    [InlineData("('''def','''pqr''')", "[{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('xyz''','a''bc')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":3,\"SSN\":\"xyz'\"}]")]
    [InlineData("('xyz''','''def')", "[{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"}]")]
    [InlineData("('xyz''','''pqr''')", "[{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''pqr''','a''bc')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''pqr''','''def')", "[{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''pqr''','xyz''')", "[{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('a''bc','''def','xyz''')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"}]")]
    [InlineData("('a''bc','''def','''pqr''')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('a''bc','xyz''','''def')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"}]")]
    [InlineData("('a''bc','xyz''','''pqr''')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('a''bc','''pqr''','''def')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('a''bc','''pqr''','xyz''')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''def','a''bc','xyz''')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"}]")]
    [InlineData("('''def','a''bc','''pqr''')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''def','xyz''','a''bc')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"}]")]
    [InlineData("('''def','xyz''','''pqr''')", "[{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''def','''pqr''','a''bc')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''def','''pqr''','xyz''')", "[{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('xyz''','a''bc','''def')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"}]")]
    [InlineData("('xyz''','a''bc','''pqr''')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('xyz''','''def','''pqr''')", "[{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('xyz''','''def','a''bc')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"}]")]
    [InlineData("('xyz''','''pqr''','a''bc')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('xyz''','''pqr''','''def')", "[{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''pqr''','a''bc','''def')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''pqr''','a''bc','xyz''')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''pqr''','''def','a''bc')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''pqr''','''def','xyz''')", "[{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''pqr''','xyz''','a''bc')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''pqr''','xyz''','''def')", "[{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    public async Task TestSingleQuotesOnInExpression(string inExpr, string partialResult)
    {
        // Arrange
        var queryUrl = $"odata/People?$filter=SSN in {inExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();

        Assert.EndsWith($"$metadata#People\",\"value\":{partialResult}}}", result);
    }

    [Theory]
    [InlineData("DynamicSingleValuedProperty eq 'a'")]
    [InlineData("DeclaredSingleValuedProperty eq 'a'")]
    public async Task TestEqualOperatorOnSingleValuedProperty(string filterExpr)
    {
        // Arrange
        var queryUrl = $"odata/Products?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();
        Assert.EndsWith(
            "/$metadata#Products\"," +
            "\"value\":[" +
            "{\"Id\":\"accdx34g-3d16-473e-9251-378c68de859e\"," +
            "\"DeclaredSingleValuedProperty\":\"a\"," +
            "\"DeclaredCollectionValuedProperty\":[1,2,3]," +
            "\"DynamicSingleValuedProperty\":\"a\"," +
            "\"DynamicCollectionValuedProperty@odata.type\":\"#Collection(Int32)\",\"DynamicCollectionValuedProperty\":[1,2,3]," +
            "\"DynamicMixedCollectionValuedProperty\":[\"a\",2,3]}]}",
            result);
    }

    [Theory]
    [InlineData("DynamicSingleValuedProperty ne 'a'")]
    [InlineData("DeclaredSingleValuedProperty ne 'a'")]
    public async Task TestNotEqualOperatorOnSingleValuedProperty(string filterExpr)
    {
        // Arrange
        var queryUrl = $"odata/Products?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();
        Assert.EndsWith(
            "/$metadata#Products\"," +
            "\"value\":[" +
            "{\"Id\":\"abc8fh64-3d16-473e-9251-378c68de859f\"," +
            "\"DeclaredSingleValuedProperty\":\"b\"," +
            "\"DeclaredCollectionValuedProperty\":[2,3,4]," +
            "\"DynamicSingleValuedProperty\":\"b\"," +
            "\"DynamicCollectionValuedProperty@odata.type\":\"#Collection(Int32)\",\"DynamicCollectionValuedProperty\":[2,3,4]," +
            "\"DynamicMixedCollectionValuedProperty\":[2,\"b\",4]}," +
            "{\"Id\":\"abc8fh64-3d16-473e-9251-378c68de859g\"," +
            "\"DeclaredSingleValuedProperty\":\"c\"," +
            "\"DeclaredCollectionValuedProperty\":[3,4,5]," +
            "\"DynamicSingleValuedProperty\":\"c\"," +
            "\"DynamicCollectionValuedProperty@odata.type\":\"#Collection(Int32)\",\"DynamicCollectionValuedProperty\":[3,4,5]," +
            "\"DynamicMixedCollectionValuedProperty\":[3,4,\"c\"]}]}",
            result);
    }

    [Theory]
    [InlineData("DynamicSingleValuedProperty in ('b','c')")]
    [InlineData("DeclaredSingleValuedProperty in ('b','c')")]
    public async Task TestInOperatorOnSingleValuedProperty(string filterExpr)
    {
        // Arrange
        var queryUrl = $"odata/Products?$filter= {filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();
        Assert.EndsWith(
            "/$metadata#Products\"," +
            "\"value\":[" +
            "{\"Id\":\"abc8fh64-3d16-473e-9251-378c68de859f\"," +
            "\"DeclaredSingleValuedProperty\":\"b\"," +
            "\"DeclaredCollectionValuedProperty\":[2,3,4]," +
            "\"DynamicSingleValuedProperty\":\"b\"," +
            "\"DynamicCollectionValuedProperty@odata.type\":\"#Collection(Int32)\",\"DynamicCollectionValuedProperty\":[2,3,4]," +
            "\"DynamicMixedCollectionValuedProperty\":[2,\"b\",4]}," +
            "{\"Id\":\"abc8fh64-3d16-473e-9251-378c68de859g\"," +
            "\"DeclaredSingleValuedProperty\":\"c\"," +
            "\"DeclaredCollectionValuedProperty\":[3,4,5]," +
            "\"DynamicSingleValuedProperty\":\"c\"," +
            "\"DynamicCollectionValuedProperty@odata.type\":\"#Collection(Int32)\",\"DynamicCollectionValuedProperty\":[3,4,5]," +
            "\"DynamicMixedCollectionValuedProperty\":[3,4,\"c\"]}]}",
            result);
    }

    [Theory]
    [InlineData("DynamicCollectionValuedProperty/any(d:d eq 4)")]
    [InlineData("DeclaredCollectionValuedProperty/any(d:d eq 4)")]
    public async Task TestAnyOperatorOnCollectionValuedProperty(string filterExpr)
    {
        // Arrange
        var queryUrl = $"odata/Products?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();
        Assert.EndsWith(
            "/$metadata#Products\"," +
            "\"value\":[" +
            "{\"Id\":\"abc8fh64-3d16-473e-9251-378c68de859f\"," +
            "\"DeclaredSingleValuedProperty\":\"b\"," +
            "\"DeclaredCollectionValuedProperty\":[2,3,4]," +
            "\"DynamicSingleValuedProperty\":\"b\"," +
            "\"DynamicCollectionValuedProperty@odata.type\":\"#Collection(Int32)\",\"DynamicCollectionValuedProperty\":[2,3,4]," +
            "\"DynamicMixedCollectionValuedProperty\":[2,\"b\",4]}," +
            "{\"Id\":\"abc8fh64-3d16-473e-9251-378c68de859g\"," +
            "\"DeclaredSingleValuedProperty\":\"c\"," +
            "\"DeclaredCollectionValuedProperty\":[3,4,5]," +
            "\"DynamicSingleValuedProperty\":\"c\"," +
            "\"DynamicCollectionValuedProperty@odata.type\":\"#Collection(Int32)\",\"DynamicCollectionValuedProperty\":[3,4,5]," +
            "\"DynamicMixedCollectionValuedProperty\":[3,4,\"c\"]}]}",
            result);
    }

    [Theory]
    [InlineData("DynamicCollectionValuedProperty/all(d:d gt 2)")]
    [InlineData("DeclaredCollectionValuedProperty/all(d:d gt 2)")]
    public async Task TestAllOperatorOnCollectionValuedProperty(string filterExpr)
    {
        // Arrange
        var queryUrl = $"odata/Products?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();
        Assert.EndsWith(
            "/$metadata#Products\"," +
            "\"value\":[" +
            "{\"Id\":\"abc8fh64-3d16-473e-9251-378c68de859g\"," +
            "\"DeclaredSingleValuedProperty\":\"c\"," +
            "\"DeclaredCollectionValuedProperty\":[3,4,5]," +
            "\"DynamicSingleValuedProperty\":\"c\"," +
            "\"DynamicCollectionValuedProperty@odata.type\":\"#Collection(Int32)\",\"DynamicCollectionValuedProperty\":[3,4,5]," +
            "\"DynamicMixedCollectionValuedProperty\":[3,4,\"c\"]}]}",
            result);
    }

    [Fact]
    public async Task TestAnyOperatorOnDynamicMixedCollectionValuedProperty()
    {
        // Arrange
        var queryUrl = $"odata/Products?$filter=DynamicMixedCollectionValuedProperty/any(d:d eq 2)";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();
        Assert.EndsWith(
            "/$metadata#Products\"," +
            "\"value\":[" +
            "{\"Id\":\"accdx34g-3d16-473e-9251-378c68de859e\"," +
            "\"DeclaredSingleValuedProperty\":\"a\"," +
            "\"DeclaredCollectionValuedProperty\":[1,2,3]," +
            "\"DynamicSingleValuedProperty\":\"a\"," +
            "\"DynamicCollectionValuedProperty@odata.type\":\"#Collection(Int32)\",\"DynamicCollectionValuedProperty\":[1,2,3]," +
            "\"DynamicMixedCollectionValuedProperty\":[\"a\",2,3]}," +
            "{\"Id\":\"abc8fh64-3d16-473e-9251-378c68de859f\"," +
            "\"DeclaredSingleValuedProperty\":\"b\"," +
            "\"DeclaredCollectionValuedProperty\":[2,3,4]," +
            "\"DynamicSingleValuedProperty\":\"b\"," +
            "\"DynamicCollectionValuedProperty@odata.type\":\"#Collection(Int32)\",\"DynamicCollectionValuedProperty\":[2,3,4]," +
            "\"DynamicMixedCollectionValuedProperty\":[2,\"b\",4]}]}",
            result);
    }

    [Theory]
    [InlineData("any(d:d eq 2)")]
    [InlineData("any(d:d eq 'Sue')")]
    [InlineData("any(d:d eq null)")]
    [InlineData("all(d:d ne 3)")]
    public async Task TestLambdaOperatorsOnUntypedCollectionPropertyAsync(string lambdaExpr)
    {
        // Arrange
        var queryUrl = $"odata/Customers?$filter=UntypedCollectionProperty/{lambdaExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();
        Assert.EndsWith(
            "/$metadata#Customers\"," +
            "\"value\":[" +
            "{\"Id\":1," +
            "\"Addresses\":[]," +
            "\"UntypedCollectionProperty\":[" +
            "\"Black\"," +
            "2," +
            "{\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Address\",\"Street\":\"Broadway Street\"}," +
            "\"Sue\"," +
            "[\"x\",\"y\",\"z\"]," +
            "null]}]}",
            result);
    }

    [Theory]
    [InlineData("any(d:d eq 7)")]
    [InlineData("all(d:d gt 5)")]
    [InlineData("any(d:d ne 5)")]
    [InlineData("any(d:d gt 5)")]
    [InlineData("any(d:d ge 11)")]
    [InlineData("any(d:d le 11 and d gt 5)")]
    [InlineData("any(d:d lt 12 and d gt 5)")]
    public async Task TestLambdaOperatorsOnDynamicCollectionValuedPropertyOnComplexTypeAsync(string lambdaExpr)
    {
        // Arrange
        var queryUrl = $"odata/Customers(2)/Addresses?$filter=Floors/{lambdaExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();

        Assert.EndsWith("/$metadata#Customers(2)/Addresses\"," +
            "\"value\":[" +
            "{\"Street\":\"One Microsoft Way\"," +
            "\"Floors@odata.type\":\"#Collection(Int32)\",\"Floors\":[7,8,9,10,11]}]}",
            result);
    }
}
