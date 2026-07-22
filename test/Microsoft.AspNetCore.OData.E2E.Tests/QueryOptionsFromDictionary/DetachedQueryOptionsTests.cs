//-----------------------------------------------------------------------------
// <copyright file="DetachedQueryOptionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.QueryOptionsFromDictionary;

public class DetachedQueryOptionsTests : WebApiTestBase<DetachedQueryOptionsTests>
{
    public DetachedQueryOptionsTests(WebApiTestFixture<DetachedQueryOptionsTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        services.ConfigureControllers(typeof(DetachedQueryOptionsController));
    }

    private async Task<List<DetachedCustomer>> PostQueryAsync(Dictionary<string, string> queryOptions)
    {
        var client = CreateClient();
        var body = JsonConvert.SerializeObject(queryOptions);
        var content = new StringContent(body, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("api/detachedcustomers/apply", content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return await response.Content.ReadAsObject<List<DetachedCustomer>>();
    }

    [Fact]
    public async Task Apply_Filter_FromDictionary_ReturnsFilteredCustomers()
    {
        // Act
        var result = await PostQueryAsync(new Dictionary<string, string> { { "$filter", "Age gt 30" } });

        // Assert
        Assert.Equal(new[] { 3, 4 }, result.Select(c => c.Id));
    }

    [Fact]
    public async Task Apply_OrderByTop_FromDictionary_ReturnsOrderedTop()
    {
        // Act
        var result = await PostQueryAsync(new Dictionary<string, string>
        {
            { "$orderby", "Age desc" },
            { "$top", "2" }
        });

        // Assert
        // Ages: Charlie(40), Dave(35), Alice(30), Eve(28), Bob(25) -> top 2 => 3, 4.
        Assert.Equal(new[] { 3, 4 }, result.Select(c => c.Id));
    }

    [Fact]
    public async Task Apply_Skip_FromDictionary_ReturnsSkippedCustomers()
    {
        // Act
        var result = await PostQueryAsync(new Dictionary<string, string>
        {
            { "$orderby", "Id asc" },
            { "$skip", "2" }
        });

        // Assert
        Assert.Equal(new[] { 3, 4, 5 }, result.Select(c => c.Id));
    }

    [Fact]
    public async Task Apply_Count_FromDictionary_AppliesWithoutError()
    {
        // Act
        var result = await PostQueryAsync(new Dictionary<string, string>
        {
            { "$filter", "Age gt 30" },
            { "$count", "true" }
        });

        // Assert
        Assert.Equal(new[] { 3, 4 }, result.Select(c => c.Id));
    }

    [Fact]
    public async Task Apply_CombinedOptions_FromDictionary_ReturnsExpected()
    {
        // Act
        var result = await PostQueryAsync(new Dictionary<string, string>
        {
            { "$filter", "Age gt 25" },
            { "$orderby", "Name asc" },
            { "$top", "2" }
        });

        // Assert
        // Age gt 25 -> Alice(1), Charlie(3), Dave(4), Eve(5); order by Name asc -> Alice, Charlie, Dave, Eve; top 2 => 1, 3.
        Assert.Equal(new[] { 1, 3 }, result.Select(c => c.Id));
    }

    [Fact]
    public async Task Apply_EmptyDictionary_FromDictionary_ReturnsAllCustomers()
    {
        // Act
        var result = await PostQueryAsync(new Dictionary<string, string>());

        // Assert
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result.Select(c => c.Id));
    }
}
