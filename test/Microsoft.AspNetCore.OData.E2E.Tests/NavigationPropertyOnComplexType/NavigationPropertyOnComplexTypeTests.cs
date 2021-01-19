// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType
{
    // TODO: the test cases in this class hangs on the Azure Build pipeline, don't know the root cause yet.
#if false
    public class NavigationPropertyOnComplexTypeTests : WebODataTestBase<NavigationPropertyOnComplexTypeTests.Startup>
    {
        public class Startup : TestStartupBase
        {
            public override void ConfigureServices(IServiceCollection services)
            {
                services.ConfigureControllers(typeof(PeopleController));

                IEdmModel model = ModelGenerator.GetConventionalEdmModel();
                services.AddOData(options => options.AddModel("odata", model).SetMaxTop(2).Expand().Select().OrderBy().Filter());
            }
        }

        private const string PeopleBaseUrl = "odata/People";

        public NavigationPropertyOnComplexTypeTests(WebODataTestFixture<Startup> factory)
            : base(factory)
        {
        }

        [Fact]
        public void QueryNavigationPropertyOnComplexProperty()
        {
            // Arrange : GET ~/People(1)/HomeLocation/ZipCode
            string requestUri = PeopleBaseUrl + "(1)/HomeLocation/ZipCode";

            string equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#ZipCodes/$entity\"," +
                "\"Zip\":98052,\"City\":\"Redmond\",\"State\":\"Washington\"}";

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, contains: null, equals: equals);
        }

        [Fact]
        public void QueryComplexTypePropertyWithSelectAndExpand()
        {
            // Arrange
            string requestUri = PeopleBaseUrl + "(1)/HomeLocation?$select=Street&$expand=ZipCode";

            string equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(1)/HomeLocation(Street,ZipCode())\"," +
                "\"Street\":\"110th\"," +
                "\"ZipCode\":{\"Zip\":98052,\"City\":\"Redmond\",\"State\":\"Washington\"}}";

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, contains: null, equals: equals);
        }

        [Fact]
        public void QueryCollectionComplexTypePropertyWithSelectAndExpand()
        {
            // Arrange
            string requestUri = PeopleBaseUrl + "(1)/RepoLocations?$select=Street&$expand=ZipCode";

            // Act
            string result = ExecuteAndVerifyQueryRequest(requestUri);

            // Assert
            JObject jObj = JObject.Parse(result);
            Assert.Equal("BASE_ADDRESS/odata/$metadata#People(1)/RepoLocations(Street,ZipCode())", jObj["@odata.context"]);

            var array = jObj["value"] as JArray;
            Assert.Equal(3, array.Count);

            for (int i = 0; i < 3; i++)
            {
                JObject item = array[i] as JObject;
                Assert.Equal(new[] { "Street", "ZipCode" },
                    item.Properties().Where(p => !p.Name.StartsWith("@")).Select(p => p.Name));
                string street = "1" + (1 + i) + "0th"; // 110th, 120th, 130th
                Assert.Equal(street, array[i]["Street"].ToString());
            }
        }

        [Fact]
        public void QueryEntityWithExpandOnNavigationPropertyOfComplexProperty()
        {
            // Arrange
            string requestUri = PeopleBaseUrl + "(1)?$expand=HomeLocation/ZipCode";

            // Includes all properties of People(1) and expand the ZipCode on HomeLocation
            string contains = ",\"Id\":1," +
                "\"Name\":\"Kate\"," +
                "\"Age\":5," +
                "\"Taxes\":[7,5,9]," +
                "\"HomeLocation\":{\"Street\":\"110th\",\"TaxNo\":19,\"Emails\":[\"E1\",\"E3\",\"E2\"],";

            // Act & Assert
            string result = ExecuteAndVerifyQueryRequest(requestUri, contains);

            Assert.Contains("}],\"ZipCode\":{\"Zip\":98052,\"City\":\"Redmond\",\"State\":\"Washington\"}},\"RepoLoc", result);
        }

        [Fact]
        public void QueryEntityWithExpandOnNavigationPropertyOfComplexTypePropertyAndSelectOnOtherProperty()
        {
            // Arrange
            string requestUri = PeopleBaseUrl + "(1)?$expand=HomeLocation/ZipCode&$select=Name";

            // Only includes Name, HomeLocation and expand ZipCode on HomeLocation
            // Be noted: The output should not include "Primitive properties" in "HomeLocation".
            // The issue is form ODL, it includes an "PathSelectItem (HomeLocation)" in the SelectExpandClause.
            // See detail at: https://github.com/OData/odata.net/issues/1574
            string contains = "\"Name\":\"Kate\"," +
                "\"HomeLocation\":{\"Street\":\"110th\",\"TaxNo\":19,\"Emails\":[\"E1\",\"E3\",\"E2\"]," +
                "\"RelatedInfo\":{\"AreaSize\":101,\"CountyName\":\"King\"}," +
                "\"AdditionInfos\":[{\"AreaSize\":102,\"CountyName\":\"King1\"},{\"AreaSize\":103,\"CountyName\":\"King2\"},{\"AreaSize\":104,\"CountyName\":\"King3\"}]," +
                "\"ZipCode\":{\"Zip\":98052,\"City\":\"Redmond\",\"State\":\"Washington\"}}";

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, contains);
        }

        [Fact]
        public void QueryEntityWithExpandOnNavigationPropertyOfComplexTypePropertyAndSelectOnComplexProperty()
        {
            // Arrange
            string requestUri = PeopleBaseUrl + "(1)?$expand=HomeLocation/ZipCode&$select=HomeLocation/Street";

            string equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation/Street,HomeLocation/ZipCode())/$entity\"," +
                "\"HomeLocation\":{\"Street\":\"110th\",\"ZipCode\":{\"Zip\":98052,\"City\":\"Redmond\",\"State\":\"Washington\"}}}";

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, contains: null, equals: equals);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public void QueryEntityWithExpandOnMultipleNavigationPropertiesOfComplexTypeProperty(int key)
        {
            // Arrange
            string requestUri = PeopleBaseUrl + "(" + key + ")?$expand=HomeLocation/ZipCode,PreciseLocation/ZipCode&$select=Name";

            // only includes Name, HomeLocation, PreciseLocation and expand ZipCode on HomeLocation
            string equals;
            if (key == 1)
            {
                equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(Name,HomeLocation/ZipCode(),PreciseLocation/ZipCode())/$entity\"," +
                    "\"Name\":\"Kate\"," +
                    "\"HomeLocation\":{\"Street\":\"110th\",\"TaxNo\":19,\"Emails\":[\"E1\",\"E3\",\"E2\"],\"RelatedInfo\":{\"AreaSize\":101,\"CountyName\":\"King\"},\"AdditionInfos\":[{\"AreaSize\":102,\"CountyName\":\"King1\"},{\"AreaSize\":103,\"CountyName\":\"King2\"},{\"AreaSize\":104,\"CountyName\":\"King3\"}],\"ZipCode\":{\"Zip\":98052,\"City\":\"Redmond\",\"State\":\"Washington\"}}," +
                    "\"PreciseLocation\":null}";
            }
            else
            {

                equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(Name,HomeLocation/ZipCode(),PreciseLocation/ZipCode())/$entity\"," +
                    "\"Name\":\"Carlos\"," +
                    "\"HomeLocation\":null," +
                    "\"PreciseLocation\":{" +
                      "\"Street\":\"50th\"," +
                      "\"TaxNo\":0,\"Emails\":[],\"Latitude\":\"12\",\"Longitude\":\"22\"," +
                      "\"RelatedInfo\":null," +
                      "\"AdditionInfos\":[]," +
                      "\"ZipCode\":{\"Zip\":35816,\"City\":\"Huntsville\",\"State\":\"Alabama\"}}}";
            }

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, null, equals);
        }

        [Fact]
        public void QueryEntityWithExpandOnNavigationPropertiesOnDeepComplexTypeProperty()
        {
            // Arrange
            string requestUri = PeopleBaseUrl + "(2)?$expand=OrderInfo/BillLocation/ZipCode&$select=OrderInfo";

            string contains = "odata/$metadata#People(OrderInfo,OrderInfo/BillLocation/ZipCode())/$entity\"," +
              "\"OrderInfo\":{" +
                "\"BillLocation\":{" +
                  "\"Street\":\"110th\"," +
                  "\"TaxNo\":0," +
                  "\"Emails\":[]," +
                  "\"RelatedInfo\":null,\"AdditionInfos\":[]," +
                  "\"ZipCode\":{" +
                    "\"Zip\":98052," +
                    "\"City\":\"Redmond\"," +
                    "\"State\":\"Washington\"" +
                  "}" +
                "}," +
                "\"SubInfo\":null" +
              "}";

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, contains);
        }

        [Fact]
        public void QueryEntityWithReferenceOnNavigationPropertiesOfComplexProperty()
        {
            // Arrange
            string requestUri = PeopleBaseUrl + "(1)?$expand=HomeLocation/ZipCode/$ref&$select=HomeLocation/Street";

            string contains = "odata/$metadata#People(HomeLocation/Street,HomeLocation/ZipCode,HomeLocation/ZipCode/$ref())/$entity\"," +
              "\"HomeLocation\":{" +
                  "\"Street\":\"110th\"," +
                  "\"ZipCode\":{" +
                    "\"@odata.id\":\"ZipCodes(98052)\"" +
                  "}" +
                "}" +
              "}";

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, contains);
        }

        [Fact]
        public void QueryEntityWithReferenceOnCollectionNavigationPropertiesOfComplexProperty()
        {
            // Arrange
            string requestUri = PeopleBaseUrl + "(1)?$expand=HomeLocation/DetailCodes/$ref&$select=HomeLocation/Street";

            string contains = "odata/$metadata#People(HomeLocation/Street,HomeLocation/DetailCodes,HomeLocation/DetailCodes/$ref())/$entity\"," +
              "\"HomeLocation\":{" +
                  "\"Street\":\"110th\"," +
                  "\"DetailCodes\":[" +
                    "{" +
                       "\"@odata.id\":\"ZipCodes(98052)\"" +
                    "}," +
                    "{" +
                       "\"@odata.id\":\"ZipCodes(35816)\"" +
                    "}," +
                    "{" +
                       "\"@odata.id\":\"ZipCodes(10048)\"" +
                    "}" +
                  "]" +
                "}" +
              "}";

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, contains);
        }

        [Fact]
        public void QueryEntityWithReferenceOnNavigationPropertiesOfDeepComplexProperty()
        {
            // Arrange
            string requestUri = PeopleBaseUrl + "(2)?$expand=OrderInfo/BillLocation/ZipCode/$ref&$select=OrderInfo/BillLocation/Street";

            string contains = "odata/$metadata#People(OrderInfo/BillLocation/Street,OrderInfo/BillLocation/ZipCode,OrderInfo/BillLocation/ZipCode/$ref())/$entity\"," +
              "\"OrderInfo\":{" +
                "\"BillLocation\":{" +
                  "\"Street\":\"110th\"," +
                  "\"ZipCode\":{" +
                    "\"@odata.id\":\"ZipCodes(98052)\"" +
                  "}" +
                "}" +
              "}" +
            "}";

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, contains);
        }

        [Fact]
        public async Task DeserializingNavigationPropertyOnComplexType()
        {
            // Arrange
            string url =  PeopleBaseUrl + "(1)/HomeLocation/ZipCode/$ref";
            string payload = "{\"Zip\":98038,\"City\":\"Redmond\",\"State\":\"Washington\"}";
            HttpContent content = new StringContent(payload, Encoding.UTF8, mediaType: "application/json");
            content.Headers.ContentLength = payload.Length;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = content;
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));

            // Act
            HttpResponseMessage response = await this.Client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(payload.TrimStart('{'), result);
        }

        [Fact]
        public void QueryComplexWithExpandOnDerivedNavigationProperty()
        {
            // Arrange
            string requestUri = PeopleBaseUrl + "(4)/HomeLocation?$expand=Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation/Area";

            string contains = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(4)/HomeLocation(Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation/Area())\"," +
                "\"Street\":\"120th\",\"TaxNo\":17,\"Emails\":[\"E7\",\"E4\",\"E5\"],\"Latitude\":\"12.8\",\"Longitude\":\"22.9\",\"Rela";

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, contains: contains, equals: null);
        }

        [Fact]
        public void QueryComplexWithTypeCastAndExpandOnDerivedNavigationProperty()
        {
            // Arrange
            string requestUri = PeopleBaseUrl + "(4)/HomeLocation/Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation?$expand=Area";

            string contains = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(4)/HomeLocation/Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation(Area())\"," +
                "\"Street\":\"120th\",\"TaxNo\":17,\"Emails\":[\"E7\",\"E4\",\"E5\"],\"Latitude\":\"12.8\",\"Longitude\":\"22.9\",\"Rela";

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, contains: contains, equals: null);
        }

        [Fact]
        public void QueryEntityWithExpandOnComplexWithTypeCastAndDerivedNavigationProperty()
        {
            // Arrange
            string requestUri = PeopleBaseUrl + "(4)?$expand=HomeLocation/Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation/Area";

            string contains = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation/Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation/Area())/$entity\"," +
                "\"Id\":4,\"Name\":\"Jones\",\"Age\":9,";

            // Act & Assert
            var result = ExecuteAndVerifyQueryRequest(requestUri, contains: contains, equals: null);

            Assert.Contains("\"HomeLocation\":{\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation\",\"Street\":\"12", result);
        }

        [Fact]
        public void QueryEntityWithExpandNavigationPropertyOnRecursiveComplexProperty()
        {
            // Arrange
            string requestUri = PeopleBaseUrl + "(4)?$expand=OrderInfo/SubInfo/BillLocation/ZipCode&$select=OrderInfo/SubInfo/BillLocation/Street";

            string equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(OrderInfo/SubInfo/BillLocation/Street,OrderInfo/SubInfo/BillLocation/ZipCode())/$entity\"," +
                "\"OrderInfo\":{" +
                   "\"SubInfo\":{" +
                     "\"BillLocation\":{" +
                        "\"Street\":\"110th\"," +
                        "\"ZipCode\":{\"Zip\":35816,\"City\":\"Huntsville\",\"State\":\"Alabama\"}}}}}";

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, contains: null, equals: equals);
        }

        private string ExecuteAndVerifyQueryRequest(string requestUri, string contains = null, string equals = null)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            // Act
            HttpResponseMessage response = Client.SendAsync(request).Result;

            // Assert
            string result = response.Content.ReadAsStringAsync().Result;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // replace the real address using "BASE_ADDRESS"
            string odataContext = "\"@odata.context\":\"";
            int start = result.IndexOf(odataContext) + odataContext.Length;
            int end = result.IndexOf("/odata/$metadata");
            string uri = result.Substring(start, end - start);
            result = result.Replace(uri, "BASE_ADDRESS");

            if (contains != null)
            {
                Assert.Contains(contains, result);
            }

            if (equals != null)
            {
                Assert.Equal(equals, result);
            }

            return result;
        }
    }
#endif
}

