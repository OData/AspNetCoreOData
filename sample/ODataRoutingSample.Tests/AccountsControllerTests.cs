using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ODataRoutingSample.Controllers;
using ODataRoutingSample.Models;

namespace ODataRoutingSample.Tests
{
    /// <summary>
    /// This is a suggestion how to unit test a controller which will be very useful for the devs
    /// </summary>
    [TestClass]
    public class AccountsControllerTests
    {
        // It is not mocked because it is not used
        private readonly ILogger<WeatherForecastController> _mockedLogger = null;
        private AccountsController _accountsController;

        [TestInitialize]
        public void TestInit()
        {
            _accountsController = new AccountsController(_mockedLogger);
        }

        [TestMethod]
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
                "http://localhost/api?$top=2&$inlinecount=allpages",
                dataOptions => dataOptions.AddModel("odata", edmModel));

            var oDataQueryContext = new ODataQueryContext(edmModel, typeof(Account), new ODataPath());

            var aDataQueryOptions = new ODataQueryOptions<Account>(oDataQueryContext, request);

            // Act
            _accountsController = new AccountsController(_mockedLogger);
            var result = _accountsController.Get(aDataQueryOptions);
            var accounts = (IQueryable<Account>) ((OkObjectResult) result).Value;

            // Assert
            Assert.AreEqual(2, accounts.Count(), "Incorrect returned number of top query!");
        }
    }
}
