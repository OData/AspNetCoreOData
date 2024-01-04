//-----------------------------------------------------------------------------
// <copyright file="OrderByAdvancedTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ODataOrderByTest
{
    public class OrderByAdvancedTest : WebApiTestBase<OrderByAdvancedTest>
    {
        public OrderByAdvancedTest(WebApiTestFixture<OrderByAdvancedTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            IEdmModel edmModel = GetEdmModel();

            services.ConfigureControllers(typeof(StudentsController));

            services.AddControllers().AddOData(opt =>
                opt.Count().Filter().OrderBy().Expand().Select().AddRouteComponents("odata", edmModel));
        }

        public static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<OrderByStudent>("Students");
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("$orderby=name", new[] { 2, 1, 6, 5, 3, 4 }, new[] { "a1", "A1", "Ab", "AB", "bb", "Bc" })]
        [InlineData("$orderby=tolower(name)", new[] { 1, 2, 5, 6, 3, 4 }, new[] { "A1", "a1", "AB", "Ab", "bb", "Bc" })]
        [InlineData("$orderby=toupper(name)", new[] { 1, 2, 5, 6, 3, 4 }, new[] { "A1", "a1", "AB", "Ab", "bb", "Bc" })]
        public async Task DollarOrderBy_UsingAdvanced_BuiltInStringFunctions(string orderBy, int[] ids, string[] names)
        {
            // Arrange
            string queryUrl = $"odata/students?{orderBy}&$select=id,name";
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

            JObject payloadBody = await response.Content.ReadAsObject<JObject>();
            (int[] actualIds, string[] actualNames) = GetIds(payloadBody, "Name");

            Assert.True(ids.SequenceEqual(actualIds));
            Assert.True(names.SequenceEqual(actualNames));
        }

        [Theory]
        [InlineData("$orderby=birthday", new[] { 2, 3, 1, 6, 5, 4 }, new[] { "1908", "1987", "2011", "2019", "2019", "2023" })]
        [InlineData("$orderby=year(birthday)", new[] { 2, 3, 1, 5, 6, 4 },new[] { "1908", "1987", "2011", "2019", "2019", "2023" })] // Be noted: #5 goes before #6
        [InlineData("$orderby=month(birthday)", new[] { 1, 6, 4, 5, 3, 2 }, new[] { "2011", "2019", "2023", "2019", "1987", "1908" })]
        public async Task DollarOrderBy_UsingAdvanced_BuiltInDateFunctions(string orderBy, int[] ids, string[] birthdays)
        {
            // Arrange
            string queryUrl = $"odata/students?{orderBy}&$select=id,year&$compute=year(birthday) as year";
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

            JObject payloadBody = await response.Content.ReadAsObject<JObject>();
            (int[] actualIds, string[] actualBirthdays) = GetIds(payloadBody, "year");

            Assert.True(ids.SequenceEqual(actualIds));
            Assert.True(birthdays.SequenceEqual(actualBirthdays));
        }

        [Fact]
        public async Task DollarOrderBy_UsingDollarCount()
        {
            // Arrange
            string queryUrl = $"odata/students?$orderby=Grades/$count&$select=id,number&$compute=Grades/$count as number";
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

            JObject payloadBody = await response.Content.ReadAsObject<JObject>();
            (int[] actualIds, string[] actualNumbers) = GetIds(payloadBody, "number");

            Assert.True(new int[] { 2, 5, 1, 4, 3, 6 }.SequenceEqual(actualIds));
            Assert.True(new int[] { 1, 1, 3, 3, 4, 5 }.SequenceEqual(actualNumbers.Select(a => int.Parse(a))));
        }

        private static (int[], string[]) GetIds(JObject payload, string propertyName)
        {
            JArray value = payload["value"] as JArray;
            Assert.NotNull(value);

            int[] ids = new int[value.Count()];
            string[] properties = new string[value.Count()];
            for (int i = 0; i < value.Count(); i++)
            {
                JObject item = value[i] as JObject;
                ids[i] = (int)item["Id"];
                properties[i] = (string)item[propertyName];
            }

            return (ids, properties);
        }

        [Fact]
        public async Task DollarOrderBy_UsingPropertyPath()
        {
            // Arrange
            string queryUrl = $"odata/students?$orderby=location/city&$select=id,location/city";
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
            Assert.Equal("{\"value\":[" +
                "{\"Id\":3,\"Location\":{\"City\":\"Avat\"}}," +
                "{\"Id\":4,\"Location\":{\"City\":\"Aveneve\"}}," +
                "{\"Id\":5,\"Location\":{\"City\":\"Ces\"}}," +
                "{\"Id\":1,\"Location\":{\"City\":\"Cesar\"}}," +
                "{\"Id\":6,\"Location\":{\"City\":\"Claire\"}}," +
                "{\"Id\":2,\"Location\":{\"City\":\"Debra\"}}" +
              "]}", payloadBody);
        }

        [Fact]
        public async Task DollarOrderBy_UsingPropertyPath_WithBuiltInStringFunction()
        {
            // Arrange
            string queryUrl = $"odata/students?$orderby=tolower(substring(location/city,1,2))&$select=id,location/city,t&$compute=tolower(substring(location/city,1,2)) as t";
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
            Assert.Equal("{\"value\":[" +
                "{\"Id\":2,\"t\":\"eb\",\"Location\":{\"City\":\"Debra\"}}," +
                "{\"Id\":1,\"t\":\"es\",\"Location\":{\"City\":\"Cesar\"}}," +
                "{\"Id\":5,\"t\":\"es\",\"Location\":{\"City\":\"Ces\"}}," +
                "{\"Id\":6,\"t\":\"la\",\"Location\":{\"City\":\"Claire\"}}," +
                "{\"Id\":3,\"t\":\"va\",\"Location\":{\"City\":\"Avat\"}}," +
                "{\"Id\":4,\"t\":\"ve\",\"Location\":{\"City\":\"Aveneve\"}}" +
              "]}", payloadBody);
        }

        [Fact]
        public async Task DollarOrderBy_UsingDollarThisWithinDollarSelect_OnCollection()
        {
            // Arrange
            string queryUrl = "odata/students/6?$select=Grades($orderby=$this)";
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

            // Origin is : 9, 8, 7, 1, 2
            Assert.Equal("{\"Grades\":[1,2,7,8,9]}", payloadBody);
        }


        [Fact]
        public async Task DollarOrderBy_UsingDollarThis_OnCollection()
        {
            // Arrange
            string queryUrl = "odata/students/6/Grades?$orderby=$it";
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

            // Origin is : 9, 8, 7, 1, 2
            Assert.Equal("{\"value\":[1,2,7,8,9]}", payloadBody);
        }
    }
}
