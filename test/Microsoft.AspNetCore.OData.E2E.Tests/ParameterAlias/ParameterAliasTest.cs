// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.E2E.Tests.ParameterAlias;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.OData.E2E.Tests
{

    public class ParameterAliasTests : WebApiTestBase<ParameterAliasTests>
    {

        public ParameterAliasTests(WebApiTestFixture<ParameterAliasTests> fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
            ConfigureServicesAction = (IServiceCollection services) =>
            {
                services.ConfigureControllers(typeof(TradesController));

                services.AddControllers().AddOData(opt => opt.Count().Filter().OrderBy().Expand().SetMaxTop(null)
                    .AddModel(GetModel()));
            };
        }

        private static IEdmModel GetModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<Trade> tradesConfiguration = builder.EntitySet<Trade>("Trades");

            //Add bound function
            var boundFunction = tradesConfiguration.EntityType.Collection.Function("GetTradingVolume");
            boundFunction.Parameter<string>("productName");
            boundFunction.Parameter<CountryOrRegion>("PortingCountryOrRegion");
            boundFunction.Returns<long?>();

            //Add bound function
            boundFunction = tradesConfiguration.EntityType.Collection.Function("GetTopTrading");
            boundFunction.Parameter<string>("productName");
            boundFunction.ReturnsFromEntitySet<Trade>("Trades");
            boundFunction.IsComposable = true;

            //Add unbound function
            var unboundFunction = builder.Function("GetTradeByCountry");
            unboundFunction.Parameter<CountryOrRegion>("PortingCountryOrRegion");
            unboundFunction.ReturnsCollectionFromEntitySet<Trade>("Trades");

            builder.Namespace = typeof(CountryOrRegion).Namespace;

           return builder.GetEdmModel();
        }

        [Fact]
        public async Task ParameterAliasInFunctionCall()
        {
            //Unbound function
            string query = "/GetTradeByCountry(PortingCountryOrRegion=@p1)?@p1=Microsoft.AspNetCore.OData.E2E.Tests.ParameterAlias.CountryOrRegion'USA'";
            HttpClient client = CreateClient();

            HttpResponseMessage response = await client.GetAsync(query);
            var json = await response.Content.ReadAsObject<JObject>();
            var result = json["value"] as JArray;
            Assert.Equal(3, result.Count);

            //Bound function
            string requestUri = "/Trades/Microsoft.AspNetCore.OData.E2E.Tests.ParameterAlias.GetTradingVolume(productName=@p1,PortingCountryOrRegion=@p2)?@p1='Rice'&@p2=Microsoft.AspNetCore.OData.E2E.Tests.ParameterAlias.CountryOrRegion'USA'";
            response = await client.GetAsync(requestUri);
            json = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(1000, (long)json["value"]);
        }

        [Theory]
        [InlineData("?$filter=contains(@p1, @p2)&@p1=Description&@p2='Export'", 3)]   //Reference property and primitive type
        [InlineData("?@p1=startswith(Description,'Import')&$filter=@p1", 3)]  //Reference expression
        [InlineData("?$filter=TradingVolume eq @p1", 1)]  //Reference nullable value
        public async Task ParameterAliasInFilter(string queryOption, int expectedResult)
        {
            string requestBaseUri = "/Trades";
            HttpClient client = CreateClient();

            HttpResponseMessage response = await client.GetAsync(requestBaseUri + queryOption);

            var json = await response.Content.ReadAsObject<JObject>();
            var result = json["value"] as JArray;
            Assert.Equal(expectedResult, result.Count);
        }

        [Theory]
        [InlineData("?$orderby=@p1&@p1=PortingCountryOrRegion", "Australia")]
        [InlineData("?$orderby=ProductName,@p2 desc,PortingCountryOrRegion desc&@p2=TradingVolume", "USA")]
        public async Task ParameterAliasInOrderby(string queryOption, string expectedPortingCountry)
        {
            string requestBaseUri = "/Trades";
            HttpClient client = CreateClient();

            HttpResponseMessage response = await client.GetAsync(requestBaseUri + queryOption);

            var json = await response.Content.ReadAsObject<JObject>();
            var result = json["value"] as JArray;
            Assert.Equal(expectedPortingCountry, result.First["PortingCountryOrRegion"]);
            Assert.Equal("Corn", result.First["ProductName"]);
            Assert.Equal(8000, result.First["TradingVolume"]);
        }

        [Theory]
        //Use multi times in different place
        [InlineData("/GetTradeByCountry(PortingCountryOrRegion=@p1)?@p1=Microsoft.AspNetCore.OData.E2E.Tests.ParameterAlias.CountryOrRegion'USA'&$filter=PortingCountryOrRegion eq @p1 and @p2 gt 1000&$orderby=@p2&@p2=TradingVolume", 1, 0)]
        //Reference property under complex type
        [InlineData("/Trades?$filter=@p1 gt 0&$orderby=@p1&@p1=TradeLocation/ZipCode", 3, 1)]
        public async Task MiscParameterAlias(string queryUri, int expectedEntryCount, int expectedZipCode)
        {
            HttpClient client = CreateClient();

            HttpResponseMessage response = await client.GetAsync(queryUri);
            string responseContent = await response.Content.ReadAsStringAsync();
            Output.WriteLine(responseContent);

            var json = await response.Content.ReadAsObject<JObject>();
            var result = json["value"] as JArray;
            Assert.Equal(expectedEntryCount, result.Count);
            Assert.Equal(expectedZipCode, result.First["TradeLocation"]["ZipCode"]);
        }

        [Fact]
        public async Task ParameterAliasWithUnresolvedPathSegment()
        {
            HttpClient client = CreateClient();

            var queryUri = "/Trades/Microsoft.AspNetCore.OData.E2E.Tests.ParameterAlias.GetTopTrading(productName=@p1)/unknown?@p1='Corn'";
            var response = await client.GetAsync(queryUri);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            //var json = await response.Content.ReadAsObject<JObject>();
            //Assert.Equal("Corn", (string)json["value"]);
        }

    }

}
