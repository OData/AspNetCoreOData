//-----------------------------------------------------------------------------
// <copyright file="SelectImprovementOnComplexTypeTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType;

public class SelectImprovementOnComplexTypeTests : WebODataTestBase<SelectImprovementOnComplexTypeTests.Startup>
{
    public class Startup : TestStartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(PeopleController));

            IEdmModel model = ModelGenerator.GetConventionalEdmModel();
            services.AddControllers().AddOData(options => options.AddRouteComponents("odata", model).SetMaxTop(2).Expand().Select().OrderBy().Filter());
        }
    }

    private const string PeopleBaseUrl = "odata/People";

    public SelectImprovementOnComplexTypeTests(WebODataTestFixture<Startup> factory)
        : base(factory)
    {
    }

    #region SubProperty on Single ComplexProperty
    [Theory]
    [InlineData("HomeLocation/Street,HomeLocation/TaxNo")]
    [InlineData("HomeLocation($select=Street,TaxNo)")]
    public void QueryEntityWithSelectOnSubPrimitivePropertyOfComplexProperty(string select)
    {
        // Arrange
        string requestUri = PeopleBaseUrl + "(1)?$select=" + select;

        string value = "\"HomeLocation\":{\"Street\":\"110th\",\"TaxNo\":19}";
        string equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation/Street,HomeLocation/TaxNo)/$entity\"," + value + "}";

        // Act & Assert
        ExecuteAndVerifyQueryRequest(requestUri, equals);
    }

    [Theory]
    [InlineData("HomeLocation/Emails")]
    [InlineData("HomeLocation($select=Emails)")]
    public void QueryEntityWithSelectOnSubCollectionPrimitivePropertyOfComplexTypeProperty(string select)
    {
        // Arrange
        string requestUri = PeopleBaseUrl + "(1)?$select=" + select;

        string value = "\"HomeLocation\":{\"Emails\":[\"E1\",\"E3\",\"E2\"]}";
        string equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation/Emails)/$entity\"," + value + "}";

        // Act & Assert
        ExecuteAndVerifyQueryRequest(requestUri, equals);
    }

    [Theory]
    [InlineData("HomeLocation/RelatedInfo,HomeLocation/AdditionInfos")]
    [InlineData("HomeLocation($select=RelatedInfo,AdditionInfos)")]
    public void QueryEntityWithSelectOnSubComplexPropertyOfComplexProperty(string select)
    {
        // Arrange
        string requestUri = PeopleBaseUrl + "(1)?$select=" + select;

        string value = "\"HomeLocation\":{\"RelatedInfo\":{\"AreaSize\":101,\"CountyName\":\"King\"}," +
            "\"AdditionInfos\":[" +
              "{\"AreaSize\":102,\"CountyName\":\"King1\"}," +
              "{\"AreaSize\":103,\"CountyName\":\"King2\"}," +
              "{\"AreaSize\":104,\"CountyName\":\"King3\"}" +
            "]}";
        string equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation/RelatedInfo,HomeLocation/AdditionInfos)/$entity\"," + value + "}";

        // Act & Assert
        ExecuteAndVerifyQueryRequest(requestUri, equals);
    }

    [Theory]
    [InlineData("HomeLocation/ZipCode,HomeLocation/Street")]
    [InlineData("HomeLocation($select=ZipCode,Street)")]
    public void QueryEntityWithSelectOnSubNavigationPropertyOfComplexTypeProperty(string select)
    {
        // Arrange
        string requestUri = PeopleBaseUrl + "(1)?$select=" + select + "&$format=application/json;odata.metadata=full";

        string value = "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.Person\"," +
            "\"@odata.id\":\"BASE_ADDRESS/odata/People(1)\"," +
            "\"@odata.editLink\":\"People(1)\"," +
            "\"HomeLocation\":{" +
                "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.Address\"," +
                "\"Street\":\"110th\"," +
                "\"ZipCode@odata.associationLink\":\"BASE_ADDRESS/odata/People(1)/HomeLocation/ZipCode/$ref\"," +
                "\"ZipCode@odata.navigationLink\":\"BASE_ADDRESS/odata/People(1)/HomeLocation/ZipCode\"" +
            "}";
        string equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation/ZipCode,HomeLocation/Street)/$entity\"," + value + "}";

        // Act & Assert
        ExecuteAndVerifyQueryRequest(requestUri, equals);
    }

    [Theory]
    [InlineData("HomeLocation/Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation/Latitude,HomeLocation/Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation/Longitude")]
    [InlineData("HomeLocation($select=Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation/Latitude,Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation/Longitude)")]
    public void QueryEntityWithSelectOnDerivedSubPropertyOfComplexTypeProperty(string select)
    {
        // Arrange
        string requestUri = PeopleBaseUrl + "(2)?$select=" + select;

        string value = "\"HomeLocation\":{\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation\",\"Latitude\":\"12.211\",\"Longitude\":\"231.131\"}";
        string equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation/Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation/Latitude,HomeLocation/Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation/Longitude)/$entity\"," + value + "}";

        // Act & Assert
        ExecuteAndVerifyQueryRequest(requestUri, equals);
    }

    [Theory]
    [InlineData("OrderInfo/DynamicAddress,OrderInfo/DynamicInt")]
    [InlineData("OrderInfo($select=DynamicAddress,DynamicInt)")]
    public void QueryEntityWithSelectOnSubDynamicPropertyOfComplexTypeProperty(string select)
    {
        // Arrange
        string requestUri = PeopleBaseUrl + "(5)?$select=" + select;

        string value = "\"OrderInfo\":{" +
            "\"DynamicInt\":9," +
            "\"DynamicAddress\":{" +
              "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.Address\"," +
              "\"Street\":\"\"," +
              "\"TaxNo\":0," +
              "\"Emails\":[\"abc@1.com\",\"xyz@2.com\"]," +
              "\"RelatedInfo\":null," +
              "\"AdditionInfos\":[]" +
            "}}";
        string equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(OrderInfo/DynamicAddress,OrderInfo/DynamicInt)/$entity\"," + value + "}";

        // Act & Assert
        ExecuteAndVerifyQueryRequest(requestUri, equals);
    }
    #endregion

    #region SubProperty On Collection ComplexProperty
    [Theory]
    [InlineData("RepoLocations/Street,RepoLocations/TaxNo,RepoLocations/RelatedInfo")]
    [InlineData("RepoLocations($select=Street,TaxNo,RelatedInfo)")]
    public void QueryEntityWithSelectOnSubPropertyOfCollectionComplexProperty(string select)
    {
        // Arrange
        string requestUri = PeopleBaseUrl + "(1)?$select=" + select;

        string value = "\"RepoLocations\":[" +
            "{\"Street\":\"110th\",\"TaxNo\":19,\"RelatedInfo\":{\"AreaSize\":101,\"CountyName\":\"King\"}}," +
            "{\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation\",\"Street\":\"120th\",\"TaxNo\":17,\"RelatedInfo\":null}," +
            "{\"Street\":\"130th\",\"TaxNo\":18,\"RelatedInfo\":{\"AreaSize\":201,\"CountyName\":\"Queue\"}}" +
          "]";
        string equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(RepoLocations/Street,RepoLocations/TaxNo,RepoLocations/RelatedInfo)/$entity\"," + value + "}";

        // Act & Assert
        ExecuteAndVerifyQueryRequest(requestUri, equals);
    }

    [Theory]
    [InlineData("RepoLocations/Emails")]
    [InlineData("RepoLocations($select=Emails)")]
    public void QueryEntityWithSelectOnSubCollectionPrimitivePropertyOfCollectionComplexProperty(string select)
    {
        // Arrange
        string requestUri = PeopleBaseUrl + "(4)?$select=" + select;

        string value = "\"RepoLocations\":[" +
            "{\"Emails\":[\"E1\",\"E3\",\"E2\"]}," +
            "{" +
              "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation\"," +
              "\"Emails\":[\"E7\",\"E4\",\"E5\"]" +
            "}" +
          "]";

        string equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(RepoLocations/Emails)/$entity\"," + value + "}";

        // Act & Assert
        ExecuteAndVerifyQueryRequest(requestUri, equals);
    }

    [Theory]
    [InlineData("RepoLocations/RelatedInfo,RepoLocations/AdditionInfos")]
    [InlineData("RepoLocations($select=RelatedInfo,AdditionInfos)")]
    public void QueryEntityWithSelectOnSubComplexPropertyOfCollectionComplexProperty(string select)
    {
        // Arrange
        string requestUri = PeopleBaseUrl + "(5)?$select=" + select;

        string value = "\"RepoLocations\":[{\"RelatedInfo\":{\"AreaSize\":101,\"CountyName\":\"King\"}," +
            "\"AdditionInfos\":[" +
              "{\"AreaSize\":102,\"CountyName\":\"King1\"}," +
              "{\"AreaSize\":103,\"CountyName\":\"King2\"}," +
              "{\"AreaSize\":104,\"CountyName\":\"King3\"}" +
            "]}]";

        string equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(RepoLocations/RelatedInfo,RepoLocations/AdditionInfos)/$entity\"," + value + "}";

        // Act & Assert
        ExecuteAndVerifyQueryRequest(requestUri, equals);
    }

    [Theory]
    [InlineData("RepoLocations/Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation/Latitude")]
    [InlineData("RepoLocations($select=Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation/Latitude)")]
    public void QueryEntityWithSelectOnDerivedSubPropertyOfCollectionComplexTypeProperty(string select)
    {
        // Arrange
        string requestUri = PeopleBaseUrl + "(4)?$select=" + select;

        string value = "\"RepoLocations\":[" +
              "{}," + // Be noted, this is correct.
              "{\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation\",\"Latitude\":\"12.8\"}" +
            "]";
        string equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(RepoLocations/Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation/Latitude)/$entity\"," + value + "}";

        // Act & Assert
        ExecuteAndVerifyQueryRequest(requestUri, equals);
    }
    #endregion

    #region Nested query option on select
    [Theory]
    [InlineData("Taxes", "\"Taxes\":[7,5,9]")]
    // [InlineData("Taxes($filter=$this eq 5)", "\"Taxes\":[5]")]  "TODO: enable these three cases when ODL supports $this
    // [InlineData("Taxes($filter=$this le 8)", "\"Taxes\":[7,5]")]
    public void QueryEntityWithSelectOnCollectionPrimitivePropertyWithNestedFilter(string select, string value)
    {
        // Arrange
        string requestUri = PeopleBaseUrl + "(1)?$select=" + select;
        string equals = string.Format("{{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(Taxes)/$entity\",{0}}}", value);

        // Act & Assert
        ExecuteAndVerifyQueryRequest(requestUri, equals);
    }

    [Theory(Skip = "TODO: enable these three cases when ODL supports $this")]
    [InlineData("Taxes($orderby=$this)", "\"Taxes\":[5,7,9]")]
    [InlineData("Taxes($orderby =$this desc)", "\"Taxes\":[9,7,5]")]
    public void QueryEntityWithSelectOnCollectionPrimitivePropertyWithNestedOrderby(string select, string value)
    {
        // Arrange
        string requestUri = PeopleBaseUrl + "(1)?$select=" + select;
        string equals = string.Format("{{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(Taxes)/$entity\",{0}}}", value);

        // Act & Assert
        ExecuteAndVerifyQueryRequest(requestUri, equals);
    }

    [Theory]
    [InlineData("Taxes($top=1;$skip=1)", "\"Taxes\":[5]")]
    [InlineData("Taxes($top=2)", "\"Taxes\":[7,5]")]
    [InlineData("Taxes($top=2;$skip=1)", "\"Taxes\":[5,9]")]
    public void QueryEntityWithSelectOnCollectionPrimitivePropertyWithNestedTopAndSkip(string select, string value)
    {
        // Arrange
        string requestUri = PeopleBaseUrl + "(1)?$select=" + select;
        string equals = string.Format("{{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(Taxes)/$entity\",{0}}}", value);

        // Act & Assert
        ExecuteAndVerifyQueryRequest(requestUri, equals);
    }

    [Theory(Skip = "TODO: enable these three cases when ODL supports $this")]
    [InlineData("HomeLocation/Emails($filter=$this eq 'E3')")]
    [InlineData("HomeLocation($select=Emails($filter=$this eq 'E3'))")]
    public void QueryEntityWithSelectOnSubCollectionPrimitivePropertyOfComplexTypePropertyWithNestedFilter(string select)
    {
        // Arrange
        string requestUri = PeopleBaseUrl + "(1)?$select=" + select;

        string equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation/Emails)/$entity\"," +
                "\"HomeLocation\":{\"Emails\":[\"E3\"]}}";

        // Act & Assert
        ExecuteAndVerifyQueryRequest(requestUri, equals);
    }

    [Theory(Skip = "TODO: enable these three cases when ODL supports $this")]
    [InlineData("HomeLocation/Emails($orderby=$this)")]
    [InlineData("HomeLocation($select=Emails($orderby=$this desc))")]
    public void QueryEntityWithSelectOnCollectionPrimitivePropertyOfComplexPropertyWithNestedOrderby(string select)
    {
        // Arrange
        string requestUri = PeopleBaseUrl + "(5)?$select=" + select;

        string equals;
        if (select.Contains("$select="))
        {
            equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation/Emails)/$entity\"," +
                "\"HomeLocation\":{\"Emails\":[\"E9\",\"E8\",\"E6\"]}}";
        }
        else
        {
            equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation/Emails)/$entity\"," +
                "\"HomeLocation\":{\"Emails\":[\"E6\",\"E8\",\"E9\"]}}";
        }

        // Act & Assert
        ExecuteAndVerifyQueryRequest(requestUri, equals);
    }

    [Theory]
    [InlineData("HomeLocation/Emails($top=1;$skip=1)", "\"Emails\":[\"E6\"]")]
    [InlineData("HomeLocation/Emails($top=2)", "\"Emails\":[\"E9\",\"E6\"]")]
    [InlineData("HomeLocation/Emails($top=2;$skip=1)", "\"Emails\":[\"E6\",\"E8\"]")]
    public void QueryEntityWithSelectOnCollectionPrimitivePropertyOfComplexPropertyWithNestedTopAndSkip(string select, string value)
    {
        // Arrange
        string requestUri = PeopleBaseUrl + "(5)?$select=" + select;
        string equals = string.Format("{{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation/Emails)/$entity\",\"HomeLocation\":{{{0}}}}}", value);

        // Act & Assert
        ExecuteAndVerifyQueryRequest(requestUri, equals);
    }

    [Theory(Skip = "TODO: enable these three cases when ODL supports $this")]
    [InlineData("RepoLocations/Emails($filter=$this eq 'E3')")]
    [InlineData("RepoLocations($select=Emails($filter=$this eq 'E3'))")]
    public void QueryEntityWithSelectOnSubCollectionPrimitivePropertyOfCollectionComplexTypePropertyWithFilter(string select)
    {
        // Arrange
        string requestUri = PeopleBaseUrl + "(1)?$select=" + select;

        string value = "\"RepoLocations\":[" +
            "{\"Emails\":[\"E3\"]}," +
            "{\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation\",\"Emails\":[]}," +
            "{\"Emails\":[]}" +
          "]";
        string equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(RepoLocations/Emails)/$entity\"," + value + "}";

        // Act & Assert
        ExecuteAndVerifyQueryRequest(requestUri, equals);
    }

    [Theory(Skip = "TODO: enable these three cases when ODL supports $this")]
    [InlineData("RepoLocations/Emails($orderby=$this;$top=1;$skip=2)")]
    [InlineData("RepoLocations($select=Emails($orderby=$this;$top=1;$skip=2))")]
    public void QueryEntityWithSelectOnSubCollectionPrimitivePropertyOfCollectionComplexTypePropertyWithOrderByTopSkip(string select)
    {
        // Arrange
        string requestUri = PeopleBaseUrl + "(1)?$select=" + select;

        string value = "\"RepoLocations\":[" +
            "{\"Emails\":[\"E3\"]}," +
            "{\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation\",\"Emails\":[\"E7\"]}," +
            "{\"Emails\":[\"E9\"]}" +
          "]";
        string equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(RepoLocations/Emails)/$entity\"," + value + "}";

        // Act & Assert
        ExecuteAndVerifyQueryRequest(requestUri, equals);
    }

    [Theory]
    [InlineData("RepoLocations($filter=TaxNo eq 17;$select=AdditionInfos($filter=AreaSize eq 102))")]
    public void QueryEntityWithSelectOnSubCollectionComplexPropertyOfCollectionComplexTypePropertyWithNestedFilter(string select)
    {
        // Arrange
        string requestUri = PeopleBaseUrl + "(1)?$select=" + select;

        string equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(RepoLocations/AdditionInfos)/$entity\"," +
            "\"RepoLocations\":[" +
              "{" +
                "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation\"," +
                "\"AdditionInfos\":[" +
                  "{\"AreaSize\":102,\"CountyName\":\"King1\"}" +
                "]" +
              "}" +
            "]}";

        // Act & Assert
        ExecuteAndVerifyQueryRequest(requestUri, equals);
    }
    #endregion

    [Theory]
    [InlineData("HomeLocation/Street")]
    [InlineData("HomeLocation($select=Street)")]
    public void QueryEntitySetWithSelectOnSubPropertyOfComplexTypeProperty(string select)
    {
        // Arrange
        string requestUri = PeopleBaseUrl + "?$select=" + select;

        string value = "\"value\":[" +
            "{\"HomeLocation\":{\"Street\":\"110th\"}}," +
            "{\"HomeLocation\":{\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation\",\"Street\":\"110th\"}}," +
            "{\"HomeLocation\":{\"Street\":null}}," +
            "{\"HomeLocation\":{\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation\",\"Street\":\"120th\"}}," +
            "{\"HomeLocation\":{\"Street\":\"130th\"}}," +
            "{\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.VipPerson\",\"HomeLocation\":{\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeometryLocation\",\"Street\":\"130th\"}}]";

        string equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation/Street)\"," + value + "}";

        // Act & Assert
        ExecuteAndVerifyQueryRequest(requestUri, equals);
    }

    [Fact]
    public void QueryEntitySetWithSelectOnDerivedProperty()
    {
        // Arrange
        string requestUri = PeopleBaseUrl +
            $"?$select=Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.VipPerson/Bonus";

        string equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.VipPerson/Bonus)\"," +
            "\"value\":[" +
            "{}," +
            "{}," +
            "{}," +
            "{}," +
            "{}," +
            "{\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.VipPerson\",\"Bonus\":99}" +
          "]}";

        // Act & Assert
        ExecuteAndVerifyQueryRequest(requestUri, equals);
    }

    [Fact]
    public void QueryEntitySetWithSelectOnDerivedPropertyWithFullMetadata()
    {
        // Arrange
        string requestUri = PeopleBaseUrl +
            "?$select=Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.VipPerson/Bonus&$format=application/json;odata.metadata=full";

        string equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.VipPerson/Bonus)\"," +
            "\"value\":[" +
            "{" +
               "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.Person\"," +
               "\"@odata.id\":\"BASE_ADDRESS/odata/People(1)\"," +
               "\"@odata.editLink\":\"People(1)\"" +
            "}," +
            "{" +
               "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.Person\"," +
               "\"@odata.id\":\"BASE_ADDRESS/odata/People(2)\"," +
               "\"@odata.editLink\":\"People(2)\"" +
            "}," +
            "{" +
               "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.Person\"," +
               "\"@odata.id\":\"BASE_ADDRESS/odata/People(3)\"," +
               "\"@odata.editLink\":\"People(3)\"" +
            "}," +
            "{" +
               "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.Person\"," +
               "\"@odata.id\":\"BASE_ADDRESS/odata/People(4)\"," +
               "\"@odata.editLink\":\"People(4)\"" +
            "}," +
            "{" +
               "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.Person\"," +
               "\"@odata.id\":\"BASE_ADDRESS/odata/People(5)\"," +
               "\"@odata.editLink\":\"People(5)\"" +
            "}," +
            "{" +
               "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.VipPerson\"," +
               "\"@odata.id\":\"BASE_ADDRESS/odata/People(6)\"," +
               "\"@odata.editLink\":\"People(6)/Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.VipPerson\"," +
               "\"Bonus\":99" +
            "}" +
          "]}";

        // Act & Assert
        ExecuteAndVerifyQueryRequest(requestUri, equals);
    }

    [Theory]
    [InlineData("HomeLocation/Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation/Latitude,HomeLocation/Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeometryLocation/Latitude")]
    [InlineData("HomeLocation($select=Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation/Latitude,Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeometryLocation/Latitude)")]
    public void QueryEntitySetWithSelectOnSameNameDerivedPropertyOfComplexPropertyWithTypeCast(string select)
    {
        // Arrange
        string requestUri = PeopleBaseUrl + "?$select=" + select;

        string value = "\"value\":[" +
            "{" +
                "\"HomeLocation\":{}" +
            "}," +
            "{" +
                "\"HomeLocation\":{\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation\",\"Latitude\":\"12.211\"}" +
            "}," +
            "{" +
                "\"HomeLocation\":{}" +
            "}," +
            "{" +
                "\"HomeLocation\":{\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation\",\"Latitude\":\"12.8\"}" +
            "}," +
            "{" +
                "\"HomeLocation\":{}" +
            "}," +
            "{" +
                "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.VipPerson\"," +
                "\"HomeLocation\":{\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeometryLocation\",\"Latitude\":\"101.1\"}" +
            "}" +
         "]";

        string equals = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation/Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeoLocation/Latitude,HomeLocation/Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType.GeometryLocation/Latitude)\"," + value + "}";

        // Act & Assert
        ExecuteAndVerifyQueryRequest(requestUri, equals);
    }

    private string ExecuteAndVerifyQueryRequest(string requestUri, string equals)
    {
        // Arrange
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

        // Act
        HttpResponseMessage response = this.Client.SendAsync(request).Result;

        // Assert
        string result = response.Content.ReadAsStringAsync().Result;

        // replace the real address using "BASE_ADDRESS"
        string odataContext = "\"@odata.context\":\"";
        int start = result.IndexOf(odataContext) + odataContext.Length;
        int end = result.IndexOf("/odata/$metadata");
        string uri = result.Substring(start, end - start);
        result = result.Replace(uri, "BASE_ADDRESS");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(equals, result);

        return result;
    }
}
