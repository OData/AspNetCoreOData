//-----------------------------------------------------------------------------
// <copyright file="CaseInsensitiveTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.UriParserExtension;

public class CaseInsensitiveTest : WebApiTestBase<CaseInsensitiveTest>
{
    public CaseInsensitiveTest(WebApiTestFixture<CaseInsensitiveTest> fixture)
        :base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        services.ConfigureControllers(typeof(CustomersController), typeof(OrdersController), typeof(MetadataController));

        IEdmModel model = UriParserExtenstionEdmModel.GetEdmModel();

        services.AddControllers().AddOData(opt => opt.AddRouteComponents("odata", model).Count().Filter().OrderBy().Expand().SetMaxTop(null));
    }

    public static TheoryDataSet<string, string, string> CaseInsensitiveCases
    {
        get
        {
            return new TheoryDataSet<string, string, string>()
            {
                // $metadata, $count, $ref, $value
                { "Get", "$metadata", "$meTadata"},
                { "Get", "Customers(1)/Name/$value", "Customers(1)/Name/$vAlue"},
          //      { "Get", "Customers(1)/Orders/$ref", "Customers(1)/Orders/$rEf" },
                { "Get", "Customers/$count", "Customers/$coUNt" },

                // Metadata value
                { "Get", "Customers", "CusTomeRs"},
                { "Get", "Customers(2)", "CusTomeRs(2)"},
                { "Get", "Customers(2)/Name", "CusTomeRs(2)/nAMe"},

          //      { "Get", "Customers(6)/Microsoft.AspNetCore.OData.E2E.Tests.UriParserExtension.VipCustomer/VipProperty", "Customers(6)/Microsoft.AspNetCore.OData.E2E.Tests.UriParserExtension.VipCustomer/vipproPERty"},
                { "Get", "Customers(1)/Default.CalculateSalary(month=2)", "Customers(1)/deFault.calCULateSalary(month=2)" },
                { "Post", "Customers(1)/Default.UpdateAddress", "Customers(1)/deFault.updateaDDress" },
            };
        }
    }

    [Theory]
    [MemberData(nameof(CaseInsensitiveCases))]
    public async Task EnableCaseInsensitiveTest(string method, string caseSensitive, string caseInsensitive)
    {
        // Case sensitive
        HttpClient client = CreateClient();

        var caseSensitiveUri = $"odata/{caseSensitive}";
        HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), caseSensitiveUri);
        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string caseSensitiveResponse = await response.Content.ReadAsStringAsync();

        // Case Insensitive
        var caseInsensitiveUri = $"odata/{caseInsensitive}";
        request = new HttpRequestMessage(new HttpMethod(method), caseInsensitiveUri);
        response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string caseInsensitiveResponse = await response.Content.ReadAsStringAsync();

        Assert.Equal(caseSensitiveResponse, caseInsensitiveResponse);
    }
}
