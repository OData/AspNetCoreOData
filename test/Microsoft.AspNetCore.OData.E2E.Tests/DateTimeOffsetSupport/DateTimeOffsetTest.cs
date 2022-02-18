//-----------------------------------------------------------------------------
// <copyright file="DateTimeOffsetTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DateTimeOffsetSupport
{
    public class DateTimeOffsetTest : WebApiTestBase<DateTimeOffsetTest>
    {
        #region Configuration and Static Members
        public DateTimeOffsetTest(WebApiTestFixture<DateTimeOffsetTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            string connectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=DateTimeOffsetSupport8";
            services.AddDbContext<FilesContext>(opt => opt.UseLazyLoadingProxies().UseSqlServer(connectionString));

            services.ConfigureControllers(typeof(FilesController));

            services.AddControllers().AddOData(opt => opt.Count().Filter().OrderBy().Expand().SetMaxTop(null).Select()
                .AddRouteComponents("convention", DateTimeOffsetEdmModel.GetConventionModel())
                .AddRouteComponents("explicit", DateTimeOffsetEdmModel.GetExplicitModel()));
        }
        #endregion

        #region Helper methods
        private string Serialize(Object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        private async Task<File> DeserializeAsync(HttpResponseMessage response)
        {
            return JsonConvert.DeserializeObject<File>(await response.Content.ReadAsStringAsync());
        }

        private async Task<IList<File>> DeserializeListAsync(HttpResponseMessage response)
        {
            JObject json = JObject.Parse(await response.Content.ReadAsStringAsync());
            IList<JToken> value = json["value"].Children().ToList();
            IList<File> files = new List<File>();

            foreach (JToken token in value) 
            {
                var file = JsonConvert.DeserializeObject<File>(token.ToString());
                files.Add(file);
            }

            return files;
        }
        #endregion

        #region CRUD on DateTimeOffset related entity
        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task QueryFileEntityTest(string mode)
        {
            // Arrange
            string requestUri = $"{mode}/Files(3)";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            File file = await DeserializeAsync(response);

            Assert.Equal(new File()
            {
                FileId = 3,
                Name = "File #3",
                CreatedDate = new DateTimeOffset(2018, 4, 15, 16, 24, 08, TimeSpan.FromHours(-8)),
                DeleteDate = new DateTimeOffset(2021, 1, 15, 16, 24, 8, TimeSpan.FromHours(-8))
            }, file);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task UpdateFileEntityTestRoundTrip(string mode)
        {
            HttpClient client = CreateClient();
            string requestUri = $"{mode}/Files(2)";

            // GET ~/Files(2)
            HttpResponseMessage response = await client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            File file2 = await DeserializeAsync(response);

            // set
            var now = DateTimeOffset.Now;
            var contentObject = new { CreatedDate = now };
            string content = Serialize(contentObject);

            // Patch ~/Files(2)
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            request.Content = new StringContent(content);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Content.Headers.ContentLength = content.Length;

            response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // GET ~/Files(2)
            response = await client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            file2.CreatedDate = now;
            File newFile2 = await DeserializeAsync(response);
            Assert.Equal(file2, newFile2);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task CreateDeleteFileEntityRoundTrip(string mode)
        {
            HttpClient client = CreateClient();
            HttpResponseMessage response = await client.PostAsync("/ResetDataSource", null);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Arrange
            string filesUri = $"{mode}/Files";

            File newFile = new File()
            {
                Name = "FileName",
                CreatedDate = DateTimeOffset.Now
            };
            string content = Serialize(newFile);

            // POST ~/Files
            HttpRequestMessage postRequest = new HttpRequestMessage(HttpMethod.Post, filesUri);
            postRequest.Content = new StringContent(content);
            postRequest.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            postRequest.Content.Headers.ContentLength = content.Length;

            var postResponse = await client.SendAsync(postRequest);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

            var postResult = await DeserializeAsync(postResponse);
            newFile.FileId = postResult.FileId;
            Assert.Equal(newFile, postResult);

            string fileUri = filesUri + $"({postResult.FileId})";

            // GET ~/Files(?)
            HttpResponseMessage getResponse = await client.GetAsync(fileUri);
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var getResult = JsonConvert.DeserializeObject<File>(await getResponse.Content.ReadAsStringAsync());
            Assert.Equal(getResult, postResult);

            // Delete ~/Files(?)
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, fileUri);
            var deleteResponse = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // GET ~/Files(?)
            getResponse = await client.GetAsync(fileUri);
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task CreateFileEntity_Works_UsingDifferentPropertyNameCase()
        {
            // Arrange
            HttpClient client = CreateClient();
            string filesUri = $"convention/Files";
            string content =
                "{" +
                    "\"fileid\":99," + // use a special ID to test
                    "\"naMe\":\"abc\"," +
                    "\"creaTeddate\":\"2021-10-28T21:33:26+08:00\"," +
                    "\"deLEteDate\":\"2021-11-01T10:48:12+08:00\"" +
                "}";

            // Act: POST ~/Files
            HttpRequestMessage postRequest = new HttpRequestMessage(HttpMethod.Post, filesUri);
            postRequest.Content = new StringContent(content);
            postRequest.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var postResponse = await client.SendAsync(postRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);

            string payload = await postResponse.Content.ReadAsStringAsync();
            Assert.Contains("PropertyCaseInsensitive", payload);
        }
        #endregion

        #region Query option on DateTime

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task CanSelectDateTimeProperty(string mode)
        {
            // Arrange
            HttpClient client = CreateClient();

            string requestUri = $"{mode}/Files(4)?$select=CreatedDate";

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string payload = await response.Content.ReadAsStringAsync();

            Assert.Contains("$metadata#Files(CreatedDate)/$entity\",\"CreatedDate\":\"2025-03-15T16:24:08-08:00\"}", payload);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task CanFilterDateTimeProperty(string mode)
        {
            // Arrange
            HttpClient client = CreateClient();
            string requestUri = $"{mode}/Files?$filter=year(CreatedDate) eq 2020";

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseFileList = await DeserializeListAsync(response);
            File actual = Assert.Single(responseFileList);
            Assert.Equal("File #1", actual.Name);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task CanOrderByDateTimeProperty(string mode)
        {
            // Arrange
            HttpClient client = CreateClient();
            HttpResponseMessage response = await client.PostAsync("/ResetDataSource", null);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            string requestUri = $"{mode}/Files?$orderby=CreatedDate";

            // Act
            response = await client.GetAsync(requestUri + " desc");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseFileList = await DeserializeListAsync(response);

            Assert.Equal(new[] { 4, 2, 1, 3, 5 }, responseFileList.Select(f => f.FileId));

            // Act
            response = await client.GetAsync(requestUri + " asc");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseFileList2 = await DeserializeListAsync(response);

            Assert.Equal(new[] { 5, 3, 1, 2, 4 }, responseFileList2.Select(f => f.FileId));
        }
        #endregion

        #region now() Function Tests
        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task Now_FilterDateTimePropertyWithGt(string mode)
        {
            // Arrange
            HttpClient client = CreateClient();

            string requestUri = $"{mode}/Files?$filter=CreatedDate gt now()";

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await DeserializeListAsync(response);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task NowFilterDateTimePropertyWithLt(string mode)
        {
            // Arrange
            HttpClient client = CreateClient();

            string requestUri = $"{mode}/Files?$filter=CreatedDate lt now()";

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await DeserializeListAsync(response);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task NowFilterDateTimePropertyWithDayFunction(string mode)
        {
            // Arrange
            HttpClient client = CreateClient();
            string requestUri = $"{mode}/Files?$filter=day(CreatedDate) eq day(now())";

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await DeserializeListAsync(response);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task NowFilterDateTimePropertyWithMonthFunction(string mode)
        {
            // Arrange
            HttpClient client = CreateClient();

            string requestUri = $"{mode}/Files?$filter=month(CreatedDate) eq month(now())";

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await DeserializeListAsync(response);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task NowFilterDateTimePropertyWithYearFunction(string mode)
        {
            // Arrange
            HttpClient client = CreateClient();

            string requestUri = $"{mode}/Files?$filter=year(CreatedDate) ge year(now())";

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await DeserializeListAsync(response);
        }
        #endregion
    }
}
