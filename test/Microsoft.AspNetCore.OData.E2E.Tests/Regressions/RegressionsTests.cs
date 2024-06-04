//-----------------------------------------------------------------------------
// <copyright file="RegressionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Regressions
{
    public class RegressionsTests : WebApiTestBase<RegressionsTests>
    {
        private readonly ITestOutputHelper _output;

        public RegressionsTests(WebApiTestFixture<RegressionsTests> fixture, ITestOutputHelper output)
            : base(fixture)
        {
            _output = output;
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            services.AddSqlite<RegressionsDbContext>($"Data Source=UsersWithNullContext.db");

            IEdmModel edmModel = GetEdmModel();

            services.ConfigureControllers(typeof(UsersController));

            services.AddControllers().AddOData(opt =>
                opt.EnableQueryFeatures()
                .AddRouteComponents("odata", edmModel));
        }

        [Fact]
        public async Task QueryUsersWithNullReferenceKey_Works()
        {
            // Arrange
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync("odata/users");

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payloadBody = await response.Content.ReadAsStringAsync();

            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#Users\"," +
              "\"value\":[" +
                "{\"UserId\":1,\"Name\":\"Alex\",\"Age\":35,\"DataFileRef\":null}," +
                "{\"UserId\":2,\"Name\":\"Amanda\",\"Age\":29,\"DataFileRef\":2}," +
                "{\"UserId\":3,\"Name\":\"Lara\",\"Age\":25,\"DataFileRef\":null}" +
              "]" +
            "}", payloadBody);
        }

        // This is a failing regression test, see https://github.com/OData/AspNetCoreOData/issues/1035
        // It was fail with: System.InvalidOperationException : Nullable object must have a value.
        [Fact]
        public async Task QueryUsersWithNullReferenceKeyUsingDollarExpand_Works()
        {
            // Arrange
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync($"odata/users?$expand=Files");

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payloadBody = await response.Content.ReadAsStringAsync();

            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#Users(Files())\"," +
                "\"value\":[" +
                  "{\"UserId\":1,\"Name\":\"Alex\",\"Age\":35,\"DataFileRef\":null,\"Files\":null}," +
                  "{\"UserId\":2,\"Name\":\"Amanda\",\"Age\":29,\"DataFileRef\":2,\"Files\":{\"FileId\":2,\"FileName\":\"uyr65euit5.pdf\"}}," +
                  "{\"UserId\":3,\"Name\":\"Lara\",\"Age\":25,\"DataFileRef\":null,\"Files\":null}" +
                "]" +
              "}", payloadBody);
        }

        public static TheoryDataSet<string, string> SingleUserQueryCases
        {
            get
            {
                var data = new TheoryDataSet<string, string>
                {
                    {
                        "odata/users/1",
                        "{CONTEXTURI,\"UserId\":1,\"Name\":\"Alex\",\"Age\":35,\"DataFileRef\":null,\"Files\":null}"
                    },
                    {
                        "odata/users/2",
                        "{CONTEXTURI,\"UserId\":2,\"Name\":\"Amanda\",\"Age\":29,\"DataFileRef\":2,\"Files\":{\"FileId\":2,\"FileName\":\"uyr65euit5.pdf\"}}"
                    },
                    {
                        "odata/users/3",
                        "{CONTEXTURI,\"UserId\":3,\"Name\":\"Lara\",\"Age\":25,\"DataFileRef\":null,\"Files\":null}"
                    }
                };

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(SingleUserQueryCases))]
        public async Task QuerySingleUserWithNullReferenceKeyUsingDollarExpand_Works(string request, string expected)
        {
            // Arrange
            HttpClient client = CreateClient();
            expected = expected.Replace("CONTEXTURI", "\"@odata.context\":\"http://localhost/odata/$metadata#Users(Files())/$entity\"");

            // Act
            HttpResponseMessage response = await client.GetAsync($"{request}?$expand=Files");

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payloadBody = await response.Content.ReadAsStringAsync();

            Assert.Equal(expected, payloadBody);
        }

        public static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<User>("Users");
            return builder.GetEdmModel();
        }
    }
}