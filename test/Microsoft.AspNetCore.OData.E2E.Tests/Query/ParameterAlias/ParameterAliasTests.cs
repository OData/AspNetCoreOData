//-----------------------------------------------------------------------------
// <copyright file="ParameterAliasTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Query.ParameterAlias;

public class ParameterAliasTests : WebODataTestBase<ParameterAliasTests.Startup>
{
    private const int AcceptedBranchingDepth = 6;
    private const int LargeBranchingDepth = 20;
    private const int RejectedBranchingDepth = 7;

    public class Startup : TestStartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(ParameterAliasCustomersController));

            IEdmModel model = ParameterAliasEdmModel.GetEdmModel();
            services.AddControllers().AddOData(options => options
                .AddRouteComponents("odata", model)
                .Filter()
                .OrderBy());
        }
    }

    public ParameterAliasTests(WebODataTestFixture<Startup> fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task OrderByWithoutParameterAlias_ReturnsOk()
    {
        // Act
        HttpResponseMessage response = await Client.GetAsync("odata/ParameterAliasCustomers?$orderby=Id");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task OrderByWithSingleParameterAlias_ReturnsOk()
    {
        // Act
        HttpResponseMessage response = await Client.GetAsync(
            "odata/ParameterAliasCustomers?$orderby=@p0&@p0=Id");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task OrderByWithMultipleParameterAliases_ReturnsOk()
    {
        // Act
        HttpResponseMessage response = await Client.GetAsync(
            "odata/ParameterAliasCustomers?$orderby=@p0,@p1&@p0=Name&@p1=Id");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task OrderByWithBranchingParameterAliasesWithinLimit_ReturnsOk()
    {
        // Arrange
        var queryParts = new List<string> { "$orderby=@p0" };
        for (int i = 0; i < AcceptedBranchingDepth - 1; i++)
        {
            queryParts.Add($"@p{i}=@p{i + 1} add @p{i + 1}");
        }

        queryParts.Add($"@p{AcceptedBranchingDepth - 1}=Id");
        string requestUri = $"odata/ParameterAliasCustomers?{string.Join("&", queryParts).Replace(" ", "%20")}";

        // Act
        HttpResponseMessage response = await Client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task OrderByWithBranchingParameterAliasesBeyondLimit_ReturnsBadRequest()
    {
        // Arrange
        var queryParts = new List<string> { "$orderby=@p0" };
        for (int i = 0; i < RejectedBranchingDepth - 1; i++)
        {
            queryParts.Add($"@p{i}=@p{i + 1} add @p{i + 1}");
        }

        queryParts.Add($"@p{RejectedBranchingDepth - 1}=Id");
        string requestUri = $"odata/ParameterAliasCustomers?{string.Join("&", queryParts).Replace(" ", "%20")}";

        // Act
        HttpResponseMessage response = await Client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains($"The parameter alias expansion count exceeds the maximum allowed value of '{ParameterAliasNodeTranslator.MaxParameterAliasExpansionCount}'.", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task OrderByWithLargeBranchingParameterAliasGraph_ReturnsBadRequest()
    {
        // Arrange
        var queryParts = new List<string> { "$orderby=@p0" };
        for (int i = 0; i < LargeBranchingDepth - 1; i++)
        {
            queryParts.Add($"@p{i}=@p{i + 1} add @p{i + 1}");
        }

        queryParts.Add($"@p{LargeBranchingDepth - 1}=Id");
        string requestUri = $"odata/ParameterAliasCustomers?{string.Join("&", queryParts).Replace(" ", "%20")}";

        // Act
        HttpResponseMessage response = await Client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains($"The parameter alias expansion count exceeds the maximum allowed value of '{ParameterAliasNodeTranslator.MaxParameterAliasExpansionCount}'.", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task OrderByWithCombinedBranchingParameterAliasesBeyondLimit_ReturnsBadRequest()
    {
        // Arrange
        var queryParts = new List<string> { "$orderby=@a0,@b0" };
        AddBranchingAliases(queryParts, "a", AcceptedBranchingDepth);
        AddBranchingAliases(queryParts, "b", AcceptedBranchingDepth);
        string requestUri = $"odata/ParameterAliasCustomers?{string.Join("&", queryParts).Replace(" ", "%20")}";

        // Act
        HttpResponseMessage response = await Client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains($"The parameter alias expansion count exceeds the maximum allowed value of '{ParameterAliasNodeTranslator.MaxParameterAliasExpansionCount}'.", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task OrderByWithCombinedParameterAliasChainsAtLimit_ReturnsOk()
    {
        // Arrange
        var queryParts = new List<string> { "$orderby=@a0,@b0" };
        AddParameterAliasChain(queryParts, "a", ParameterAliasNodeTranslator.MaxParameterAliasExpansionCount / 2, "Id");
        AddParameterAliasChain(queryParts, "b", ParameterAliasNodeTranslator.MaxParameterAliasExpansionCount / 2, "Name");
        string requestUri = $"odata/ParameterAliasCustomers?{string.Join("&", queryParts)}";

        // Act
        HttpResponseMessage response = await Client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task OrderByWithCombinedParameterAliasChainsOneOverLimit_ReturnsBadRequest()
    {
        // Arrange
        var queryParts = new List<string> { "$orderby=@a0,@b0" };
        AddParameterAliasChain(queryParts, "a", ParameterAliasNodeTranslator.MaxParameterAliasExpansionCount / 2, "Id");
        AddParameterAliasChain(queryParts, "b", (ParameterAliasNodeTranslator.MaxParameterAliasExpansionCount / 2) + 1, "Name");
        string requestUri = $"odata/ParameterAliasCustomers?{string.Join("&", queryParts)}";

        // Act
        HttpResponseMessage response = await Client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains($"The parameter alias expansion count exceeds the maximum allowed value of '{ParameterAliasNodeTranslator.MaxParameterAliasExpansionCount}'.", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task FilterWithSingleParameterAlias_ReturnsOk()
    {
        // Act
        HttpResponseMessage response = await Client.GetAsync(
            "odata/ParameterAliasCustomers?$filter=Id%20eq%20@p0&@p0=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task FilterWithNullParameterAlias_ReturnsOk()
    {
        // Act
        HttpResponseMessage response = await Client.GetAsync(
            "odata/ParameterAliasCustomers?$filter=Name%20eq%20@p0&@p0=null");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task FilterWithBranchingParameterAliasesWithinLimit_ReturnsOk()
    {
        // Arrange
        var queryParts = new List<string> { "$filter=@p0 gt 0" };
        AddBranchingAliases(queryParts, "p", AcceptedBranchingDepth);
        string requestUri = $"odata/ParameterAliasCustomers?{string.Join("&", queryParts).Replace(" ", "%20")}";

        // Act
        HttpResponseMessage response = await Client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task FilterWithBranchingParameterAliasesBeyondLimit_ReturnsBadRequest()
    {
        // Arrange
        var queryParts = new List<string> { "$filter=@p0 gt 0" };
        for (int i = 0; i < RejectedBranchingDepth - 1; i++)
        {
            queryParts.Add($"@p{i}=@p{i + 1} add @p{i + 1}");
        }

        queryParts.Add($"@p{RejectedBranchingDepth - 1}=Id");
        string requestUri = $"odata/ParameterAliasCustomers?{string.Join("&", queryParts).Replace(" ", "%20")}";

        // Act
        HttpResponseMessage response = await Client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains($"The parameter alias expansion count exceeds the maximum allowed value of '{ParameterAliasNodeTranslator.MaxParameterAliasExpansionCount}'.", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task AcceptedRequestAfterRejectedRequest_ReturnsOk()
    {
        // Arrange
        var queryParts = new List<string> { "$orderby=@p0" };
        for (int i = 0; i < RejectedBranchingDepth - 1; i++)
        {
            queryParts.Add($"@p{i}=@p{i + 1} add @p{i + 1}");
        }

        queryParts.Add($"@p{RejectedBranchingDepth - 1}=Id");
        string rejectedRequestUri = $"odata/ParameterAliasCustomers?{string.Join("&", queryParts).Replace(" ", "%20")}";

        // Act
        HttpResponseMessage rejectedResponse = await Client.GetAsync(rejectedRequestUri);
        HttpResponseMessage acceptedResponse = await Client.GetAsync(
            "odata/ParameterAliasCustomers?$orderby=@p0&@p0=Id");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, rejectedResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, acceptedResponse.StatusCode);
    }

    private static void AddBranchingAliases(ICollection<string> queryParts, string prefix, int depth)
    {
        for (int i = 0; i < depth - 1; i++)
        {
            queryParts.Add($"@{prefix}{i}=@{prefix}{i + 1} add @{prefix}{i + 1}");
        }

        queryParts.Add($"@{prefix}{depth - 1}=Id");
    }

    private static void AddParameterAliasChain(
        ICollection<string> queryParts,
        string prefix,
        int length,
        string propertyName)
    {
        for (int i = 0; i < length - 1; i++)
        {
            queryParts.Add($"@{prefix}{i}=@{prefix}{i + 1}");
        }

        queryParts.Add($"@{prefix}{length - 1}={propertyName}");
    }
}
