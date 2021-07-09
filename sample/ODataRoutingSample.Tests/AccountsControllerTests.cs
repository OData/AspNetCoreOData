using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using ODataRoutingSample.Controllers;
using ODataRoutingSample.Models;
using Xunit;

namespace ODataRoutingSample.Tests
{
    /// <summary>
    /// This is a suggestion how to unit test a controller which will be very useful for the devs
    /// </summary>
    public class AccountsControllerTests
    {
        // It is not mocked because it is not used
        private readonly ILogger<WeatherForecastController> _mockedLogger = null;
        private AccountsController _accountsController;

        public AccountsControllerTests()
        {
            _accountsController = new AccountsController(_mockedLogger);
        }

        [Fact]
        public void AccountsController_GetTopTwoAccounts_ShouldReturnTopTwoAccounts()
        {
            // Arrange
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Account>("Account");
            var edmModel = modelBuilder.GetEdmModel();

            var options = new ODataOptions();
            options.AddModel("odata", EdmCoreModel.Instance);
            options.AddModel("my{data}", edmModel);

            HttpRequest request = RequestFactory.Create("GET",
                "http://localhost/api?$top=2&$count=true",
                dataOptions => dataOptions.AddModel("odata", edmModel));

            var oDataQueryContext = new ODataQueryContext(edmModel, typeof(Account), new ODataPath());

            var aDataQueryOptions = new ODataQueryOptions<Account>(oDataQueryContext, request);

            // Act
            _accountsController = new AccountsController(_mockedLogger);
            var result = _accountsController.Get(aDataQueryOptions);
            var accounts = (IQueryable<Account>) ((OkObjectResult) result).Value;

            // Assert
            Assert.Equal(2, accounts.Count());
        }

        [Fact]
        public void AccountsController_SelectAccountWithNameEqualHot_ShouldReturnAccountWithHotName()
        {
            // Arrange
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Account>("Account");
            var edmModel = modelBuilder.GetEdmModel();

            var options = new ODataOptions();
            options.AddModel("odata", EdmCoreModel.Instance);
            options.AddModel("my{data}", edmModel);

            HttpRequest request = RequestFactory.Create("GET",
                "http://localhost/api?filter=Name eq 'Hot'",
                dataOptions => dataOptions.AddModel("odata", edmModel));

            var oDataQueryContext = new ODataQueryContext(edmModel, typeof(Account), new ODataPath());

            var aDataQueryOptions = new ODataQueryOptions<Account>(oDataQueryContext, request);

            // Act
            _accountsController = new AccountsController(_mockedLogger);
            var result = _accountsController.Get(aDataQueryOptions);
            var accounts = (IQueryable<Account>)((OkObjectResult)result).Value;

            // Assert
            Assert.Equal("Hot", accounts.FirstOrDefault()?.Name);
        }
    }
}
