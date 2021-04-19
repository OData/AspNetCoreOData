// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DateAndTimeOfDay
{
    public class DateWithEfTest : WebApiTestBase<DateWithEfTest>
    {
        public DateWithEfTest(WebApiTestFixture<DateWithEfTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            string connectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=EdmDateWithEfDbContext8";
            services.AddDbContext<EdmDateWithEfContext>(opt => opt.UseLazyLoadingProxies().UseSqlServer(connectionString));

            services.ConfigureControllers(typeof(EfPeopleController));

            IEdmModel model = DateAndTimeOfDayEdmModel.BuildEfPersonEdmModel();

            // TODO: modify it after implement the DI in Web API.
            // model.SetPayloadValueConverter(new MyConverter());

            services.AddOData(opt => opt.Count().Filter().OrderBy().Expand().SetMaxTop(null)
                .AddModel("odata", model));
        }

        [Theory]
        [InlineData("$filter=Birthday eq null", "2,4")]
        [InlineData("$filter=Birthday ne null", "1,3,5")]
        [InlineData("$filter=Birthday eq 2015-10-01", "1")]
        [InlineData("$filter=Birthday eq 2015-10-03", "3")]
        [InlineData("$filter=Birthday ne 2015-10-03", "1,2,4,5")]
        public async Task CanFilterByDatePropertyForDateTimePropertyOnEf(string filter, string expect)
        {
            // Arrange
            string requestUri = $"odata/EfPeople?{filter}";
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject content = await response.Content.ReadAsObject<JObject>();

            Assert.Equal(expect, string.Join(",", content["value"].Select(e => e["Id"].ToString())));
        }

        [Fact(Skip = "TODO: Processing of the LINQ expression failed")]
        public async Task CanGroupByDatePropertyForDateTimePropertyOnEf()
        {
            // Arrange
            string requestUri = "odata/EfPeople?$apply=groupby((Birthday), aggregate($count as Cnt))";
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CanQuerySingleEntityFromTaskReturnTypeInControllerOnEf()
        {
            // Arrange
            string requestUri = "odata/EfPeople(1)";
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject content = await response.Content.ReadAsObject<JObject>();

            Assert.Equal("1", (string)content["Id"]);
        }
    }

    public class MyConverter : ODataPayloadValueConverter
    {
        public override object ConvertToPayloadValue(object value, IEdmTypeReference edmTypeReference)
        {
            if (edmTypeReference != null && edmTypeReference.IsDate())
            {
                if (value is DateTimeOffset)
                {
                    DateTimeOffset dto = (DateTimeOffset)value;
                    return new Date(dto.Year, dto.Month, dto.Day);
                }
            }

            return base.ConvertToPayloadValue(value, edmTypeReference);
        }
    }
}
