//-----------------------------------------------------------------------------
// <copyright file="DollarComputeWithSkipTokenTests.cs" company=".NET Foundation">
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
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarCompute
{
    public class DollarComputeWithSkipTokenTests : WebApiTestBase<DollarComputeWithSkipTokenTests>
    {
        private readonly ITestOutputHelper output;

        public DollarComputeWithSkipTokenTests(WebApiTestFixture<DollarComputeWithSkipTokenTests> fixture, ITestOutputHelper output)
            : base(fixture)
        {
            this.output = output;
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            IEdmModel edmModel = DollarComputeEdmModel.GetStudentModel();

            services.ConfigureControllers(typeof(StudentsController));

            services.AddControllers().AddOData(opt =>
                opt.Count().Filter().OrderBy().Expand().SetMaxTop(null).SkipToken().Select().AddRouteComponents("odata", edmModel));
        }

        public static TheoryDataSet<string, string> MediaTypes
        {
            get
            {
                TheoryDataSet<string, string> data = new TheoryDataSet<string, string>();
                data.Add("$orderby=name",
                    "{\"value\":[{\"Id\":7,\"Name\":\"aa\"},{\"Id\":3,\"Name\":\"AA\"}],\"@odata.nextLink\":\"http://localhost/odata/students?$orderby=name&$skiptoken=Name-%27AA%27,Id-3\"}");

                data.Add("$orderby=name desc",
                    "{\"value\":[{\"Id\":4,\"Name\":\"DD\"},{\"Id\":2,\"Name\":\"dd\"}],\"@odata.nextLink\":\"http://localhost/odata/students?$orderby=name%20desc&$skiptoken=Name-%27dd%27,Id-2\"}");

                data.Add("$orderby=lowername&$compute=tolower(name) as lowername",
                    "{\"value\":[{\"Id\":3,\"Name\":\"AA\"},{\"Id\":7,\"Name\":\"aa\"}],\"@odata.nextLink\":\"http://localhost/odata/students?$orderby=lowername&$compute=tolower%28name%29%20as%20lowername&$skiptoken=lowername-%27aa%27,Id-7\"}");

                data.Add("$orderby=lowername desc&$compute=tolower(name) as lowername",
                    "{\"value\":[{\"Id\":2,\"Name\":\"dd\"},{\"Id\":4,\"Name\":\"DD\"}],\"@odata.nextLink\":\"http://localhost/odata/students?$orderby=lowername%20desc&$compute=tolower%28name%29%20as%20lowername&$skiptoken=lowername-%27dd%27,Id-4\"}");

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task QueryForResourceSet_WithDollarCompute_UsedDollarOrderBy(string query, string expected)
        {
            // Arrange
            string queryUrl = $"odata/students?{query}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payloadBody = await response.Content.ReadAsStringAsync();

            Assert.Equal(expected, payloadBody);
        }
    }
}
