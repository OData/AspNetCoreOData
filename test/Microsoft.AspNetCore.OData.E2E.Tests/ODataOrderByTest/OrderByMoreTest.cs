//-----------------------------------------------------------------------------
// <copyright file="ODataOrderByMoreTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ODataOrderByTest
{
    public class ODataOrderByMoreTest : WebApiTestBase<ODataOrderByMoreTest>
    {
        public ODataOrderByMoreTest(WebApiTestFixture<ODataOrderByMoreTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(BooksController));
            services.AddControllers()
                .AddOData(); // without any configuration
        }

        [Theory]
        [InlineData("/books?orderby=ISBN&top=1")]
        [InlineData("/books?orderby=isbn&top=1")]
        [InlineData("/books?$orderby=ISBN&top=1")]
        [InlineData("/books?$orderby=isbn&top=1")]
        public async Task TestOrderBy_WithDifferentPropertyCase(string requestUri)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            //
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var rawResult = await response.Content.ReadAsStringAsync();

            Assert.Equal("[{\"isbn\":\"063-6-920-02371-5\",\"id\":2}]", rawResult);
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class BooksController : ControllerBase
    {
        private static IList<Book> _books = new List<Book>
        {
            new Book
            {
                Id = 1,
                ISBN = "978-0-321-87758-1"
            },
            new Book
            {
                Id = 2,
                ISBN = "063-6-920-02371-5",
            }
        };

        [HttpGet]
        public IEnumerable<Book> Get(ODataQueryOptions<Book> queryOptions)
        {
            var queryable = (IQueryable<Book>)queryOptions.ApplyTo(_books.AsQueryable());
            return queryable.ToList();
        }
    }

    public class Book
    {
        public int Id { get; set; }
        public string ISBN { get; set; }
    }
}
