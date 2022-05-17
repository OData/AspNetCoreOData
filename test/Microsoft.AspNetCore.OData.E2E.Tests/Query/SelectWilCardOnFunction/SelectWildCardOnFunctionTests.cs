using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Query.SelectWilCardOnFunction
{

    public class SelectWildCardOnFunctionTests : WebODataTestBase<SelectWildCardOnFunctionTests.Startup>
    {


        public class Startup : TestStartupBase
        {
            public override void ConfigureServices(IServiceCollection services)
            {
                services.ConfigureControllers(typeof(CustomersController));

                IEdmModel model = SelectWildCardOnFunctionEdmModel.GetEdmModel();
                services.AddControllers().AddOData(opt =>
                {
                    opt.Select();
                    opt.RouteOptions.EnableNonParenthesisForEmptyParameterFunction = true;
                    opt.AddRouteComponents("odata", model);
                });
            }
        }
        public SelectWildCardOnFunctionTests(WebODataTestFixture<Startup> fixture)
            : base(fixture)
        {
        }

        /// <summary>
        /// For Select query with wildcard on Function
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SelectWildCardOnFunction_success()
        {
            //Arrange
            string queryUrl = "odata/Customers/GetAllCustomer?$select=*";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

            //Act
            HttpResponseMessage response = await this.Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<Customer> customers = JToken.Parse(await response.Content.ReadAsStringAsync())["value"].ToObject<List<Customer>>();
            foreach(Customer c in customers)
            {
                Assert.NotNull(c.Id);
                Assert.NotNull(c.Name);
                Assert.NotNull(c.Status);
            }
        }
    }


    public class SelectWildCardOnFunctionEdmModel
    {
        public static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Customer>("Customers").EntityType
                .Collection.Function("GetAllCustomer")
                .ReturnsCollectionFromEntitySet<Customer>("GetAllCustomer");

            return builder.GetEdmModel();
        }
    }


    public class Customer
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
    }


    public class CustomersController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<Customer> GetAllCustomer()
        {
            return new List<Customer>()
            {
                new Customer
                {
                    Id = "custId1",
                    Name = "John",
                    Status = "Active"
                },
                 new Customer
                {
                    Id = "custId2",
                    Name = "John",
                    Status = "Active"
                }
             };
        }
    }
}
