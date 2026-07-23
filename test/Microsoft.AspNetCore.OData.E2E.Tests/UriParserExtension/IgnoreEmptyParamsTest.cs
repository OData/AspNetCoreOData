//-----------------------------------------------------------------------------
// <copyright file="IgnoreEmptyParamsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//--

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.UriParserExtension;

public class IgnoreEmptyParamsTest : WebApiTestBase<IgnoreEmptyParamsTest>
{
    public IgnoreEmptyParamsTest(WebApiTestFixture<IgnoreEmptyParamsTest> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        services.ConfigureControllers(typeof(CustomersController), typeof(OrdersController), typeof(MetadataController));

        IEdmModel model = UriParserExtenstionEdmModel.GetEdmModel();

        services.AddControllers().AddOData(opt =>
        {
            opt.AddRouteComponents("odata", model).Count().Filter().OrderBy().Select().Expand().SetMaxTop(null);
            opt.EnableNoDollarQueryOptions = true;
        });
    }

    public static TheoryDataSet<string, string, string> IgnoreEmptyParamsCases
    {
        get
        {
            return new TheoryDataSet<string, string, string>()
            {
                { "Get", "Customers?$top=10&$skip=0", "Customers?$top=10&$skip=0&%20" },
                { "Get", "Customers?$select=Name,Id", "Customers?%20=foo&$select=Name,Id" },
                { "Get", "Customers?$orderby=Name", "Customers?%20=foo$orderby=Name" },
                { "Get", "Customers?$filter=Name eq 'test'", "Customers?$filter=Name eq 'test'&" },
                { "Get", "Customers?$count=true", "Customers?%20=%20&$count=true" },

                { "Get", "Customers?top=10&skip=0", "Customers?top=10&skip=0&%20" },
                { "Get", "Customers?select=Name,Id", "Customers?&%20=foo&select=Name,Id" },
                { "Get", "Customers?orderby=Name", "Customers?%20=foo&orderby=Name" },
                { "Get", "Customers?filter=Name eq 'test'", "Customers?filter=Name eq 'test'&" },
                { "Get", "Customers?count=true", "Customers?%20=%20&count=true" },

                { "Get", "Customers", "Customers?%20" },
                { "Get", "Customers(1)", "Customers(1)?=foo" },
            };
        }
    }

    [Theory]
    [MemberData(nameof(IgnoreEmptyParamsCases))]
    public async Task ParserIgnoresEmptyParamsTest(string method, string baselinePath, string emptyParamsPath)
    {
        // Baseline scenario: query params are all correct
        HttpClient client = CreateClient();

        var baselineUri = $"odata/{baselinePath}";
        HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), baselineUri);
        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string baselineResponse = await response.Content.ReadAsStringAsync();

        // Test scenario: some params are injected in the query string, such as empty spaces
        var emptyParamsUri = $"odata/{emptyParamsPath}";
        request = new HttpRequestMessage(new HttpMethod(method), emptyParamsUri);
        response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string emptyParamsResponse = await response.Content.ReadAsStringAsync();

        // Expected behavior: empty params are ignored and responses from both scenarios are equivalent
        Assert.Equal(baselineResponse, emptyParamsResponse);
    }
}
