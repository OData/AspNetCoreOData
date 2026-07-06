//-----------------------------------------------------------------------------
// <copyright file="DollarFilterTests.cs" company=".NET Foundation">
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
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter;

public class DollarFilterTests : WebApiTestBase<DollarFilterTests>
{
    public DollarFilterTests(WebApiTestFixture<DollarFilterTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        IEdmModel model = DollarFilterEdmModel.GetEdmModel();

        services.ConfigureControllers(
            typeof(PeopleController),
            typeof(VendorsController),
            typeof(BadVendorsController),
            typeof(CustomersController),
            typeof(BadCustomersController),
            typeof(ProductsController),
            typeof(BasketsController),
            typeof(BasicTypesController),
            typeof(CatalogsController));

        services.AddControllers().AddOData(opt =>
            opt.Filter().OrderBy().Select().AddRouteComponents("odata", model));
    }

    [Theory]
    [InlineData("('a''bc')", "[{\"Id\":1,\"SSN\":\"a'bc\"}]")]
    [InlineData("('''def')", "[{\"Id\":2,\"SSN\":\"'def\"}]")]
    [InlineData("('xyz''')", "[{\"Id\":3,\"SSN\":\"xyz'\"}]")]
    [InlineData("('''pqr''')", "[{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('a''bc','''def')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"}]")]
    [InlineData("('a''bc','xyz''')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":3,\"SSN\":\"xyz'\"}]")]
    [InlineData("('a''bc','''pqr''')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''def','a''bc')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"}]")]
    [InlineData("('''def','xyz''')", "[{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"}]")]
    [InlineData("('''def','''pqr''')", "[{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('xyz''','a''bc')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":3,\"SSN\":\"xyz'\"}]")]
    [InlineData("('xyz''','''def')", "[{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"}]")]
    [InlineData("('xyz''','''pqr''')", "[{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''pqr''','a''bc')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''pqr''','''def')", "[{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''pqr''','xyz''')", "[{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('a''bc','''def','xyz''')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"}]")]
    [InlineData("('a''bc','''def','''pqr''')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('a''bc','xyz''','''def')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"}]")]
    [InlineData("('a''bc','xyz''','''pqr''')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('a''bc','''pqr''','''def')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('a''bc','''pqr''','xyz''')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''def','a''bc','xyz''')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"}]")]
    [InlineData("('''def','a''bc','''pqr''')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''def','xyz''','a''bc')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"}]")]
    [InlineData("('''def','xyz''','''pqr''')", "[{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''def','''pqr''','a''bc')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''def','''pqr''','xyz''')", "[{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('xyz''','a''bc','''def')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"}]")]
    [InlineData("('xyz''','a''bc','''pqr''')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('xyz''','''def','''pqr''')", "[{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('xyz''','''def','a''bc')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"}]")]
    [InlineData("('xyz''','''pqr''','a''bc')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('xyz''','''pqr''','''def')", "[{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''pqr''','a''bc','''def')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''pqr''','a''bc','xyz''')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''pqr''','''def','a''bc')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''pqr''','''def','xyz''')", "[{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''pqr''','xyz''','a''bc')", "[{\"Id\":1,\"SSN\":\"a'bc\"},{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    [InlineData("('''pqr''','xyz''','''def')", "[{\"Id\":2,\"SSN\":\"'def\"},{\"Id\":3,\"SSN\":\"xyz'\"},{\"Id\":4,\"SSN\":\"'pqr'\"}]")]
    public async Task TestSingleQuotesOnInExpression(string inExpr, string partialResult)
    {
        // Arrange
        var queryUrl = $"odata/People?$filter=SSN in {inExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();

        Assert.EndsWith($"$metadata#People\",\"value\":{partialResult}}}", result);
    }

    [Theory]
    [InlineData("Id eq 2")]
    [InlineData("DeclaredPrimitiveProperty eq 13")]
    [InlineData("DeclaredSingleValuedProperty/Street eq 'Ocean Drive'")]
    [InlineData("DeclaredSingleValuedProperty/ZipCode eq '73857'")]
    [InlineData("DeclaredSingleValuedProperty/City/Name eq 'Miami'")]
    [InlineData("DeclaredSingleValuedProperty/City/State eq 'Florida'")]
    [InlineData("DynamicPrimitiveProperty eq 13")]
    [InlineData("DynamicSingleValuedProperty/Street eq 'Ocean Drive'")]
    [InlineData("DynamicSingleValuedProperty/ZipCode eq '73857'")]
    [InlineData("DynamicSingleValuedProperty/City/Name eq 'Miami'")]
    [InlineData("DynamicSingleValuedProperty/City/State eq 'Florida'")]
    public async Task TestDeclaredAndDynamicPropertiesInFilterExpressions(string filterExpr)
    {
        // Arrange
        var queryUrl = $"odata/Vendors?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();

        Assert.EndsWith(
            "$metadata#Vendors\"," +
            "\"value\":[" +
            "{\"Id\":2," +
            "\"DeclaredPrimitiveProperty\":13," +
            "\"DynamicPrimitiveProperty\":13," +
            "\"DeclaredSingleValuedProperty\":{" +
            "\"Street\":\"Ocean Drive\"," +
            "\"ZipCode\":\"73857\",\"City\":{\"Name\":\"Miami\",\"State\":\"Florida\"}}," +
            "\"DynamicSingleValuedProperty\":{" +
            "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.VendorAddress\"," +
            "\"Street\":\"Ocean Drive\"," +
            "\"ZipCode\":\"73857\"," +
            "\"City\":{\"Name\":\"Miami\",\"State\":\"Florida\"}}}]}",
            result);
    }

    [Theory]
    [InlineData("Id in (1,3)")]
    [InlineData("DeclaredPrimitiveProperty in (19,17)")]
    [InlineData("DeclaredSingleValuedProperty/Street in ('Bourbon Street','Canal Street')")]
    [InlineData("DeclaredSingleValuedProperty/ZipCode in ('25810','11065')")]
    [InlineData("DeclaredSingleValuedProperty/City/Name eq 'New Orleans'")]
    [InlineData("DeclaredSingleValuedProperty/City/State eq 'Louisiana'")]
    [InlineData("DynamicPrimitiveProperty eq 19 or DynamicPrimitiveProperty eq 17")]
    [InlineData("DynamicSingleValuedProperty/Street in ('Bourbon Street','Canal Street')")]
    [InlineData("DynamicSingleValuedProperty/ZipCode in ('25810','11065')")]
    [InlineData("DynamicSingleValuedProperty/City/Name eq 'New Orleans'")]
    [InlineData("DynamicSingleValuedProperty/City/State eq 'Louisiana'")]
    public async Task TestDeclaredAndDynamicPropertiesInFilterExpressionsReturningMultipleResources(string filterExpr)
    {
        // Arrange
        var queryUrl = $"odata/Vendors?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();

        Assert.EndsWith(
            "$metadata#Vendors\"," +
            "\"value\":[" +
            "{\"Id\":1," +
            "\"DeclaredPrimitiveProperty\":19," +
            "\"DynamicPrimitiveProperty\":19," +
            "\"DeclaredSingleValuedProperty\":{" +
            "\"Street\":\"Bourbon Street\"," +
            "\"ZipCode\":\"25810\"," +
            "\"City\":{\"Name\":\"New Orleans\",\"State\":\"Louisiana\"}}," +
            "\"DynamicSingleValuedProperty\":{" +
            "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.VendorAddress\"," +
            "\"Street\":\"Bourbon Street\"," +
            "\"ZipCode\":\"25810\"," +
            "\"City\":{\"Name\":\"New Orleans\",\"State\":\"Louisiana\"}}}," +
            "{\"Id\":3," +
            "\"DeclaredPrimitiveProperty\":17," +
            "\"DynamicPrimitiveProperty\":17," +
            "\"DeclaredSingleValuedProperty\":{" +
            "\"Street\":\"Canal Street\"," +
            "\"ZipCode\":\"11065\"," +
            "\"City\":{\"Name\":\"New Orleans\",\"State\":\"Louisiana\"}}," +
            "\"DynamicSingleValuedProperty\":{" +
            "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.VendorAddress\"," +
            "\"Street\":\"Canal Street\"," +
            "\"ZipCode\":\"11065\"," +
            "\"City\":{\"Name\":\"New Orleans\",\"State\":\"Louisiana\"}}}]}",
            result);
    }

    [Fact]
    public async Task TestDynamicPropertySegmentAfterNonOpenDynamicProperty()
    {
        // Arrange
        var queryUrl = $"odata/BadVendors?$filter=WarehouseAddress/City eq 'Mexico City'";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await client.SendAsync(request);
        });

        Assert.NotNull(exception);
        var odataException = exception.InnerException?.InnerException;
        Assert.NotNull(odataException);
        Assert.Equal(string.Format(SRResources.TypeMustBeOpenType, typeof(NonOpenVendorAddress).FullName),
            odataException.Message);
    }

    [Fact]
    public async Task TestDynamicPropertySegmentAfterPrimitiveDynamicProperty()
    {
        // Arrange
        var queryUrl = $"odata/BadVendors?$filter=Foo/City eq 'Mexico City'";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await client.SendAsync(request);
        });

        Assert.NotNull(exception);
        var odataException = exception.InnerException?.InnerException;
        Assert.NotNull(odataException);
        Assert.Equal(string.Format(SRResources.QueryNodeBindingNotSupported, QueryNodeKind.SingleValueOpenPropertyAccess, typeof(QueryBinder).Name),
            odataException.Message);
    }

    [Fact]
    public async Task TestDynamicPropertySegmentOnResourceTypeNotInEdmModel()
    {
        // Arrange
        var queryUrl = $"odata/BadVendors?$filter=NotInModelAddress/City eq 'Mexico City'";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await client.SendAsync(request);
        });

        Assert.NotNull(exception);
        var odataException = exception.InnerException?.InnerException;
        Assert.NotNull(odataException);
        Assert.Equal(string.Format(SRResources.ResourceTypeNotInModel, typeof(NotInModelVendorAddress).FullName),
            odataException.Message);
    }

    [Theory]
    [InlineData("ContainerPropertyNullAddress/City eq 'Mexico City'")]
    [InlineData("NonExistentDynamicProperty eq 404")]
    public async Task TestNullPropagationForNullDynamicContainerProperty(string filterExpr)
    {
        // Arrange
        var queryUrl = $"odata/BadVendors?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal(
            "{\"@odata.context\":\"http://localhost/odata/$metadata#BadVendors\",\"value\":[]}",
            result);
    }

    [Theory]
    [InlineData("Id eq 1")]
    [InlineData("DeclaredContactInfo/DeclaredEmails/any(d:d eq 'temp1a@test.com')")]
    [InlineData("DeclaredContactInfo/DynamicEmails/any(d:d eq 'temp1a@test.com')")]
    [InlineData("DeclaredContactInfo/DeclaredAddresses/any(d:d/DeclaredStreet eq 'Wujiang Road')")]
    [InlineData("DeclaredContactInfo/DeclaredAddresses/any(d:d/DynamicStreet eq 'Wujiang Road')")]
    [InlineData("DeclaredContactInfo/DeclaredAddresses/any(d:d/DeclaredFloors/any(e:e/DeclaredNumber eq 2))")]
    [InlineData("DeclaredContactInfo/DeclaredAddresses/any(d:d/DeclaredFloors/any(e:e/DynamicNumber eq 2))")]
    [InlineData("DeclaredContactInfo/DeclaredAddresses/any(d:d/DynamicFloors/any(e:e/DeclaredNumber eq 2))")]
    [InlineData("DeclaredContactInfo/DeclaredAddresses/any(d:d/DynamicFloors/any(e:e/DynamicNumber eq 2))")]
    [InlineData("DeclaredContactInfo/DynamicAddresses/any(d:d/DeclaredStreet eq 'Wujiang Road')")]
    [InlineData("DeclaredContactInfo/DynamicAddresses/any(d:d/DynamicStreet eq 'Wujiang Road')")]
    [InlineData("DeclaredContactInfo/DynamicAddresses/any(d:d/DeclaredFloors/any(e:e/DeclaredNumber eq 2))")]
    [InlineData("DeclaredContactInfo/DynamicAddresses/any(d:d/DeclaredFloors/any(e:e/DynamicNumber eq 2))")]
    [InlineData("DeclaredContactInfo/DynamicAddresses/any(d:d/DynamicFloors/any(e:e/DeclaredNumber eq 2))")]
    [InlineData("DeclaredContactInfo/DynamicAddresses/any(d:d/DynamicFloors/any(e:e/DynamicNumber eq 2))")]
    [InlineData("DynamicContactInfo/DeclaredEmails/any(d:d eq 'temp1a@test.com')")]
    [InlineData("DynamicContactInfo/DynamicEmails/any(d:d eq 'temp1a@test.com')")]
    [InlineData("DynamicContactInfo/DeclaredAddresses/any(d:d/DeclaredStreet eq 'Wujiang Road')")]
    [InlineData("DynamicContactInfo/DeclaredAddresses/any(d:d/DynamicStreet eq 'Wujiang Road')")]
    [InlineData("DynamicContactInfo/DeclaredAddresses/any(d:d/DeclaredFloors/any(e:e/DeclaredNumber eq 2))")]
    [InlineData("DynamicContactInfo/DeclaredAddresses/any(d:d/DeclaredFloors/any(e:e/DynamicNumber eq 2))")]
    [InlineData("DynamicContactInfo/DeclaredAddresses/any(d:d/DynamicFloors/any(e:e/DeclaredNumber eq 2))")]
    [InlineData("DynamicContactInfo/DeclaredAddresses/any(d:d/DynamicFloors/any(e:e/DynamicNumber eq 2))")]
    [InlineData("DynamicContactInfo/DynamicAddresses/any(d:d/DeclaredStreet eq 'Wujiang Road')")]
    [InlineData("DynamicContactInfo/DynamicAddresses/any(d:d/DynamicStreet eq 'Wujiang Road')")]
    [InlineData("DynamicContactInfo/DynamicAddresses/any(d:d/DeclaredFloors/any(e:e/DeclaredNumber eq 2))")]
    [InlineData("DynamicContactInfo/DynamicAddresses/any(d:d/DeclaredFloors/any(e:e/DynamicNumber eq 2))")]
    [InlineData("DynamicContactInfo/DynamicAddresses/any(d:d/DynamicFloors/any(e:e/DeclaredNumber eq 2))")]
    [InlineData("DynamicContactInfo/DynamicAddresses/any(d:d/DynamicFloors/any(e:e/DynamicNumber eq 2))")]
    public async Task TestAnyAsync(string filterExpr)
    {
        // Arrange
        var queryUrl = $"odata/Customers?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();
        Assert.EndsWith("/$metadata#Customers\"," +
            "\"value\":[" +
            "{\"Id\":1," +
            "\"DeclaredContactInfo\":{" +
            "\"DeclaredEmails\":[\"temp1a@test.com\",\"temp1b@test.com\"]," +
            "\"DynamicEmails@odata.type\":\"#Collection(String)\"," +
            "\"DynamicEmails\":[\"temp1a@test.com\",\"temp1b@test.com\"]," +
            "\"DeclaredAddresses\":[" +
            "{\"DeclaredStreet\":\"Temple Street\"," +
            "\"DynamicStreet\":\"Temple Street\"," +
            "\"DeclaredFloors\":[{\"DeclaredNumber\":2,\"DynamicNumber\":2},{\"DeclaredNumber\":3,\"DynamicNumber\":3}]," +
            "\"DynamicFloors@odata.type\":\"#Collection(Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Floor)\"," +
            "\"DynamicFloors\":[{\"DeclaredNumber\":2,\"DynamicNumber\":2},{\"DeclaredNumber\":3,\"DynamicNumber\":3}]}," +
            "{\"DeclaredStreet\":\"Wujiang Road\"," +
            "\"DynamicStreet\":\"Wujiang Road\"," +
            "\"DeclaredFloors\":[{\"DeclaredNumber\":4,\"DynamicNumber\":4},{\"DeclaredNumber\":5,\"DynamicNumber\":5}]," +
            "\"DynamicFloors@odata.type\":\"#Collection(Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Floor)\"," +
            "\"DynamicFloors\":[{\"DeclaredNumber\":4,\"DynamicNumber\":4},{\"DeclaredNumber\":5,\"DynamicNumber\":5}]}]," +
            "\"DynamicAddresses@odata.type\":\"#Collection(Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Address)\"," +
            "\"DynamicAddresses\":[" +
            "{\"DeclaredStreet\":\"Temple Street\"," +
            "\"DynamicStreet\":\"Temple Street\"," +
            "\"DeclaredFloors\":[{\"DeclaredNumber\":2,\"DynamicNumber\":2},{\"DeclaredNumber\":3,\"DynamicNumber\":3}]," +
            "\"DynamicFloors@odata.type\":\"#Collection(Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Floor)\"," +
            "\"DynamicFloors\":[{\"DeclaredNumber\":2,\"DynamicNumber\":2},{\"DeclaredNumber\":3,\"DynamicNumber\":3}]}," +
            "{\"DeclaredStreet\":\"Wujiang Road\"," +
            "\"DynamicStreet\":\"Wujiang Road\"," +
            "\"DeclaredFloors\":[{\"DeclaredNumber\":4,\"DynamicNumber\":4},{\"DeclaredNumber\":5,\"DynamicNumber\":5}]," +
            "\"DynamicFloors@odata.type\":\"#Collection(Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Floor)\"," +
            "\"DynamicFloors\":[{\"DeclaredNumber\":4,\"DynamicNumber\":4},{\"DeclaredNumber\":5,\"DynamicNumber\":5}]}]}," +
            "\"DynamicContactInfo\":{" +
            "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.ContactInfo\"," +
            "\"DeclaredEmails\":[\"temp1a@test.com\",\"temp1b@test.com\"]," +
            "\"DynamicEmails@odata.type\":\"#Collection(String)\"," +
            "\"DynamicEmails\":[\"temp1a@test.com\",\"temp1b@test.com\"]," +
            "\"DeclaredAddresses\":[" +
            "{\"DeclaredStreet\":\"Temple Street\"," +
            "\"DynamicStreet\":\"Temple Street\"," +
            "\"DeclaredFloors\":[{\"DeclaredNumber\":2,\"DynamicNumber\":2},{\"DeclaredNumber\":3,\"DynamicNumber\":3}]," +
            "\"DynamicFloors@odata.type\":\"#Collection(Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Floor)\"," +
            "\"DynamicFloors\":[{\"DeclaredNumber\":2,\"DynamicNumber\":2},{\"DeclaredNumber\":3,\"DynamicNumber\":3}]}," +
            "{\"DeclaredStreet\":\"Wujiang Road\"," +
            "\"DynamicStreet\":\"Wujiang Road\"," +
            "\"DeclaredFloors\":[{\"DeclaredNumber\":4,\"DynamicNumber\":4},{\"DeclaredNumber\":5,\"DynamicNumber\":5}]," +
            "\"DynamicFloors@odata.type\":\"#Collection(Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Floor)\"," +
            "\"DynamicFloors\":[{\"DeclaredNumber\":4,\"DynamicNumber\":4},{\"DeclaredNumber\":5,\"DynamicNumber\":5}]}]," +
            "\"DynamicAddresses@odata.type\":\"#Collection(Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Address)\"," +
            "\"DynamicAddresses\":[" +
            "{\"DeclaredStreet\":\"Temple Street\"," +
            "\"DynamicStreet\":\"Temple Street\"," +
            "\"DeclaredFloors\":[{\"DeclaredNumber\":2,\"DynamicNumber\":2},{\"DeclaredNumber\":3,\"DynamicNumber\":3}]," +
            "\"DynamicFloors@odata.type\":\"#Collection(Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Floor)\"," +
            "\"DynamicFloors\":[{\"DeclaredNumber\":2,\"DynamicNumber\":2},{\"DeclaredNumber\":3,\"DynamicNumber\":3}]}," +
            "{\"DeclaredStreet\":\"Wujiang Road\"," +
            "\"DynamicStreet\":\"Wujiang Road\"," +
            "\"DeclaredFloors\":[{\"DeclaredNumber\":4,\"DynamicNumber\":4},{\"DeclaredNumber\":5,\"DynamicNumber\":5}]," +
            "\"DynamicFloors@odata.type\":\"#Collection(Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Floor)\"," +
            "\"DynamicFloors\":[{\"DeclaredNumber\":4,\"DynamicNumber\":4},{\"DeclaredNumber\":5,\"DynamicNumber\":5}]}]}}]}",
            result);
    }

    [Theory]
    [InlineData("Id eq 2")]
    [InlineData("DeclaredContactInfo/DeclaredEmails/all(d:d ne 'temp1b@test.com')")]
    [InlineData("DeclaredContactInfo/DynamicEmails/all(d:d ne 'temp1b@test.com')")]
    [InlineData("DeclaredContactInfo/DeclaredAddresses/all(d:d/DeclaredStreet ne 'Temple Street')")]
    [InlineData("DeclaredContactInfo/DeclaredAddresses/all(d:d/DynamicStreet ne 'Temple Street')")]
    [InlineData("DeclaredContactInfo/DeclaredAddresses/all(d:d/DeclaredFloors/all(e:e/DeclaredNumber gt 4))")]
    [InlineData("DeclaredContactInfo/DeclaredAddresses/all(d:d/DeclaredFloors/all(e:e/DynamicNumber gt 4))")]
    [InlineData("DeclaredContactInfo/DeclaredAddresses/all(d:d/DynamicFloors/all(e:e/DeclaredNumber gt 4))")]
    [InlineData("DeclaredContactInfo/DeclaredAddresses/all(d:d/DynamicFloors/all(e:e/DynamicNumber gt 4))")]
    [InlineData("DeclaredContactInfo/DynamicAddresses/all(d:d/DeclaredStreet ne 'Temple Street')")]
    [InlineData("DeclaredContactInfo/DynamicAddresses/all(d:d/DynamicStreet ne 'Temple Street')")]
    [InlineData("DeclaredContactInfo/DynamicAddresses/all(d:d/DeclaredFloors/all(e:e/DeclaredNumber gt 4))")]
    [InlineData("DeclaredContactInfo/DynamicAddresses/all(d:d/DeclaredFloors/all(e:e/DynamicNumber gt 4))")]
    [InlineData("DeclaredContactInfo/DynamicAddresses/all(d:d/DynamicFloors/all(e:e/DeclaredNumber gt 4))")]
    [InlineData("DeclaredContactInfo/DynamicAddresses/all(d:d/DynamicFloors/all(e:e/DynamicNumber gt 4))")]
    [InlineData("DynamicContactInfo/DeclaredEmails/all(d:d ne 'temp1b@test.com')")]
    [InlineData("DynamicContactInfo/DynamicEmails/all(d:d ne 'temp1b@test.com')")]
    [InlineData("DynamicContactInfo/DeclaredAddresses/all(d:d/DeclaredStreet ne 'Temple Street')")]
    [InlineData("DynamicContactInfo/DeclaredAddresses/all(d:d/DynamicStreet ne 'Temple Street')")]
    [InlineData("DynamicContactInfo/DeclaredAddresses/all(d:d/DeclaredFloors/all(e:e/DeclaredNumber gt 4))")]
    [InlineData("DynamicContactInfo/DeclaredAddresses/all(d:d/DeclaredFloors/all(e:e/DynamicNumber gt 4))")]
    [InlineData("DynamicContactInfo/DeclaredAddresses/all(d:d/DynamicFloors/all(e:e/DeclaredNumber gt 4))")]
    [InlineData("DynamicContactInfo/DeclaredAddresses/all(d:d/DynamicFloors/all(e:e/DynamicNumber gt 4))")]
    [InlineData("DynamicContactInfo/DynamicAddresses/all(d:d/DeclaredStreet ne 'Temple Street')")]
    [InlineData("DynamicContactInfo/DynamicAddresses/all(d:d/DynamicStreet ne 'Temple Street')")]
    [InlineData("DynamicContactInfo/DynamicAddresses/all(d:d/DeclaredFloors/all(e:e/DeclaredNumber gt 4))")]
    [InlineData("DynamicContactInfo/DynamicAddresses/all(d:d/DeclaredFloors/all(e:e/DynamicNumber gt 4))")]
    [InlineData("DynamicContactInfo/DynamicAddresses/all(d:d/DynamicFloors/all(e:e/DeclaredNumber gt 4))")]
    [InlineData("DynamicContactInfo/DynamicAddresses/all(d:d/DynamicFloors/all(e:e/DynamicNumber gt 4))")]
    public async Task TestAllAsync(string filterExpr)
    {
        // Arrange
        var queryUrl = $"odata/Customers?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();
        Assert.EndsWith("/$metadata#Customers\"," +
            "\"value\":[" +
            "{\"Id\":2," +
            "\"DeclaredContactInfo\":{" +
            "\"DeclaredEmails\":[\"temp2a@test.com\",\"temp2b@test.com\"]," +
            "\"DynamicEmails@odata.type\":\"#Collection(String)\"," +
            "\"DynamicEmails\":[\"temp2a@test.com\",\"temp2b@test.com\"]," +
            "\"DeclaredAddresses\":[" +
            "{\"DeclaredStreet\":\"Buchanan Street\"," +
            "\"DynamicStreet\":\"Buchanan Street\"," +
            "\"DeclaredFloors\":[{\"DeclaredNumber\":5,\"DynamicNumber\":5},{\"DeclaredNumber\":6,\"DynamicNumber\":6}]," +
            "\"DynamicFloors@odata.type\":\"#Collection(Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Floor)\"," +
            "\"DynamicFloors\":[{\"DeclaredNumber\":5,\"DynamicNumber\":5},{\"DeclaredNumber\":6,\"DynamicNumber\":6}]}," +
            "{\"DeclaredStreet\":\"Victoria Street\"," +
            "\"DynamicStreet\":\"Victoria Street\"," +
            "\"DeclaredFloors\":[{\"DeclaredNumber\":7,\"DynamicNumber\":7},{\"DeclaredNumber\":8,\"DynamicNumber\":8}]," +
            "\"DynamicFloors@odata.type\":\"#Collection(Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Floor)\"," +
            "\"DynamicFloors\":[{\"DeclaredNumber\":7,\"DynamicNumber\":7},{\"DeclaredNumber\":8,\"DynamicNumber\":8}]}]," +
            "\"DynamicAddresses@odata.type\":\"#Collection(Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Address)\"," +
            "\"DynamicAddresses\":[" +
            "{\"DeclaredStreet\":\"Buchanan Street\"," +
            "\"DynamicStreet\":\"Buchanan Street\"," +
            "\"DeclaredFloors\":[{\"DeclaredNumber\":5,\"DynamicNumber\":5},{\"DeclaredNumber\":6,\"DynamicNumber\":6}]," +
            "\"DynamicFloors@odata.type\":\"#Collection(Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Floor)\"," +
            "\"DynamicFloors\":[{\"DeclaredNumber\":5,\"DynamicNumber\":5},{\"DeclaredNumber\":6,\"DynamicNumber\":6}]}," +
            "{\"DeclaredStreet\":\"Victoria Street\"," +
            "\"DynamicStreet\":\"Victoria Street\"," +
            "\"DeclaredFloors\":[{\"DeclaredNumber\":7,\"DynamicNumber\":7},{\"DeclaredNumber\":8,\"DynamicNumber\":8}]," +
            "\"DynamicFloors@odata.type\":\"#Collection(Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Floor)\"," +
            "\"DynamicFloors\":[{\"DeclaredNumber\":7,\"DynamicNumber\":7},{\"DeclaredNumber\":8,\"DynamicNumber\":8}]}]}," +
            "\"DynamicContactInfo\":{" +
            "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.ContactInfo\"," +
            "\"DeclaredEmails\":[\"temp2a@test.com\",\"temp2b@test.com\"]," +
            "\"DynamicEmails@odata.type\":\"#Collection(String)\"," +
            "\"DynamicEmails\":[\"temp2a@test.com\",\"temp2b@test.com\"]," +
            "\"DeclaredAddresses\":[" +
            "{\"DeclaredStreet\":\"Buchanan Street\"," +
            "\"DynamicStreet\":\"Buchanan Street\"," +
            "\"DeclaredFloors\":[{\"DeclaredNumber\":5,\"DynamicNumber\":5},{\"DeclaredNumber\":6,\"DynamicNumber\":6}]," +
            "\"DynamicFloors@odata.type\":\"#Collection(Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Floor)\"," +
            "\"DynamicFloors\":[{\"DeclaredNumber\":5,\"DynamicNumber\":5},{\"DeclaredNumber\":6,\"DynamicNumber\":6}]}," +
            "{\"DeclaredStreet\":\"Victoria Street\"," +
            "\"DynamicStreet\":\"Victoria Street\"," +
            "\"DeclaredFloors\":[{\"DeclaredNumber\":7,\"DynamicNumber\":7},{\"DeclaredNumber\":8,\"DynamicNumber\":8}]," +
            "\"DynamicFloors@odata.type\":\"#Collection(Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Floor)\"," +
            "\"DynamicFloors\":[{\"DeclaredNumber\":7,\"DynamicNumber\":7},{\"DeclaredNumber\":8,\"DynamicNumber\":8}]}]," +
            "\"DynamicAddresses@odata.type\":\"#Collection(Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Address)\"," +
            "\"DynamicAddresses\":[" +
            "{\"DeclaredStreet\":\"Buchanan Street\"," +
            "\"DynamicStreet\":\"Buchanan Street\"," +
            "\"DeclaredFloors\":[{\"DeclaredNumber\":5,\"DynamicNumber\":5},{\"DeclaredNumber\":6,\"DynamicNumber\":6}]," +
            "\"DynamicFloors@odata.type\":\"#Collection(Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Floor)\"," +
            "\"DynamicFloors\":[{\"DeclaredNumber\":5,\"DynamicNumber\":5},{\"DeclaredNumber\":6,\"DynamicNumber\":6}]}," +
            "{\"DeclaredStreet\":\"Victoria Street\"," +
            "\"DynamicStreet\":\"Victoria Street\"," +
            "\"DeclaredFloors\":[{\"DeclaredNumber\":7,\"DynamicNumber\":7},{\"DeclaredNumber\":8,\"DynamicNumber\":8}]," +
            "\"DynamicFloors@odata.type\":\"#Collection(Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Floor)\"," +
            "\"DynamicFloors\":[{\"DeclaredNumber\":7,\"DynamicNumber\":7},{\"DeclaredNumber\":8,\"DynamicNumber\":8}]}]}}]}",
            result);
    }

    [Theory]
    [InlineData("Id in (1,2)")]
    [InlineData("DeclaredContactInfo/DeclaredAddresses/any(d:d/DeclaredFloors/any(e:e/DeclaredNumber eq 5))")]
    [InlineData("DeclaredContactInfo/DeclaredAddresses/any(d:d/DeclaredFloors/any(e:e/DynamicNumber eq 5))")]
    [InlineData("DeclaredContactInfo/DeclaredAddresses/any(d:d/DynamicFloors/any(e:e/DeclaredNumber eq 5))")]
    [InlineData("DeclaredContactInfo/DeclaredAddresses/any(d:d/DynamicFloors/any(e:e/DynamicNumber eq 5))")]
    [InlineData("DeclaredContactInfo/DynamicAddresses/any(d:d/DeclaredFloors/any(e:e/DeclaredNumber eq 5))")]
    [InlineData("DeclaredContactInfo/DynamicAddresses/any(d:d/DeclaredFloors/any(e:e/DynamicNumber eq 5))")]
    [InlineData("DeclaredContactInfo/DynamicAddresses/any(d:d/DynamicFloors/any(e:e/DeclaredNumber eq 5))")]
    [InlineData("DeclaredContactInfo/DynamicAddresses/any(d:d/DynamicFloors/any(e:e/DynamicNumber eq 5))")]
    [InlineData("DynamicContactInfo/DeclaredAddresses/any(d:d/DeclaredFloors/any(e:e/DeclaredNumber eq 5))")]
    [InlineData("DynamicContactInfo/DeclaredAddresses/any(d:d/DeclaredFloors/any(e:e/DynamicNumber eq 5))")]
    [InlineData("DynamicContactInfo/DeclaredAddresses/any(d:d/DynamicFloors/any(e:e/DeclaredNumber eq 5))")]
    [InlineData("DynamicContactInfo/DeclaredAddresses/any(d:d/DynamicFloors/any(e:e/DynamicNumber eq 5))")]
    [InlineData("DynamicContactInfo/DynamicAddresses/any(d:d/DeclaredFloors/any(e:e/DeclaredNumber eq 5))")]
    [InlineData("DynamicContactInfo/DynamicAddresses/any(d:d/DeclaredFloors/any(e:e/DynamicNumber eq 5))")]
    [InlineData("DynamicContactInfo/DynamicAddresses/any(d:d/DynamicFloors/any(e:e/DeclaredNumber eq 5))")]
    [InlineData("DynamicContactInfo/DynamicAddresses/any(d:d/DynamicFloors/any(e:e/DynamicNumber eq 5))")]
    public async Task TestAnyReturningMultipleTopLevelResourcesAsync(string filterExpr)
    {
        // Arrange
        var queryUrl = $"odata/Customers?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = (await response.Content.ReadAsObject<JObject>())["value"] as JArray;
        Assert.Equal(2, result.Count); // 2 customers have floor #5
        Assert.Equal(1, (result[0] as JObject)["Id"]);
        Assert.Equal(2, (result[1] as JObject)["Id"]);
    }

    [Theory]
    [InlineData("Id in (1,2)")]
    [InlineData("DeclaredContactInfo/DeclaredAddresses/all(d:d/DeclaredFloors/all(e:e/DeclaredNumber ge 2 and e/DeclaredNumber le 8))")]
    [InlineData("DeclaredContactInfo/DeclaredAddresses/all(d:d/DeclaredFloors/all(e:e/DynamicNumber ge 2 and e/DynamicNumber le 8))")]
    [InlineData("DeclaredContactInfo/DeclaredAddresses/all(d:d/DynamicFloors/all(e:e/DeclaredNumber ge 2 and e/DeclaredNumber le 8))")]
    [InlineData("DeclaredContactInfo/DeclaredAddresses/all(d:d/DynamicFloors/all(e:e/DynamicNumber ge 2 and e/DynamicNumber le 8))")]
    [InlineData("DeclaredContactInfo/DynamicAddresses/all(d:d/DeclaredFloors/all(e:e/DeclaredNumber ge 2 and e/DeclaredNumber le 8))")]
    [InlineData("DeclaredContactInfo/DynamicAddresses/all(d:d/DeclaredFloors/all(e:e/DynamicNumber ge 2 and e/DynamicNumber le 8))")]
    [InlineData("DeclaredContactInfo/DynamicAddresses/all(d:d/DynamicFloors/all(e:e/DeclaredNumber ge 2 and e/DeclaredNumber le 8))")]
    [InlineData("DeclaredContactInfo/DynamicAddresses/all(d:d/DynamicFloors/all(e:e/DynamicNumber ge 2 and e/DynamicNumber le 8))")]
    [InlineData("DynamicContactInfo/DeclaredAddresses/all(d:d/DeclaredFloors/all(e:e/DeclaredNumber ge 2 and e/DeclaredNumber le 8))")]
    [InlineData("DynamicContactInfo/DeclaredAddresses/all(d:d/DeclaredFloors/all(e:e/DynamicNumber ge 2 and e/DynamicNumber le 8))")]
    [InlineData("DynamicContactInfo/DeclaredAddresses/all(d:d/DynamicFloors/all(e:e/DeclaredNumber ge 2 and e/DeclaredNumber le 8))")]
    [InlineData("DynamicContactInfo/DeclaredAddresses/all(d:d/DynamicFloors/all(e:e/DynamicNumber ge 2 and e/DynamicNumber le 8))")]
    [InlineData("DynamicContactInfo/DynamicAddresses/all(d:d/DeclaredFloors/all(e:e/DeclaredNumber ge 2 and e/DeclaredNumber le 8))")]
    [InlineData("DynamicContactInfo/DynamicAddresses/all(d:d/DeclaredFloors/all(e:e/DynamicNumber ge 2 and e/DynamicNumber le 8))")]
    [InlineData("DynamicContactInfo/DynamicAddresses/all(d:d/DynamicFloors/all(e:e/DeclaredNumber ge 2 and e/DeclaredNumber le 8))")]
    [InlineData("DynamicContactInfo/DynamicAddresses/all(d:d/DynamicFloors/all(e:e/DynamicNumber ge 2 and e/DynamicNumber le 8))")]
    public async Task TestAllReturningMultipleTopLevelResourcesAsync(string filterExpr)
    {
        // Arrange
        var queryUrl = $"odata/Customers?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = (await response.Content.ReadAsObject<JObject>())["value"] as JArray;
        Assert.Equal(2, result.Count);
        Assert.Equal(1, (result[0] as JObject)["Id"]);
        Assert.Equal(2, (result[1] as JObject)["Id"]);
    }

    [Theory]
    [InlineData("DeclaredContactInfo/DynamicNonOpenAddresses/any(d:d/DynamicStreet eq 'No Way')")]
    [InlineData("DynamicContactInfo/DynamicNonOpenAddresses/any(d:d/DynamicStreet eq 'No Way')")]
    [InlineData("DeclaredContactInfo/DynamicNonOpenAddresses/all(d:d/DynamicStreet eq 'No Way')")]
    [InlineData("DynamicContactInfo/DynamicNonOpenAddresses/all(d:d/DynamicStreet eq 'No Way')")]
    public async Task TestAnyAndAllOnDynamicCollectionPropertyOfNonOpenTypeAsync(string filterExpr)
    {
        // Arrange
        var queryUrl = $"odata/BadCustomers?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await client.SendAsync(request);
        });

        // Assert
        Assert.NotNull(exception);
        var odataException = exception.InnerException?.InnerException;
        Assert.NotNull(odataException);
        Assert.Equal(string.Format(SRResources.TypeMustBeOpenType, typeof(NonOpenAddress).FullName),
            odataException.Message);
    }

    [Theory]
    [InlineData("DeclaredContactInfo/DynamicNotInModelAddresses/any(d:d/DynamicStreet eq 'No Way')")]
    [InlineData("DynamicContactInfo/DynamicNotInModelAddresses/any(d:d/DynamicStreet eq 'No Way')")]
    [InlineData("DeclaredContactInfo/DynamicNotInModelAddresses/all(d:d/DynamicStreet eq 'No Way')")]
    [InlineData("DynamicContactInfo/DynamicNotInModelAddresses/all(d:d/DynamicStreet eq 'No Way')")]
    public async Task TestAnyAndAllOnDynamicCollectionPropertyOfTypeNotInModelAsync(string filterExpr)
    {
        // Arrange
        var queryUrl = $"odata/BadCustomers?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await client.SendAsync(request);
        });

        // Assert
        Assert.NotNull(exception);
        var odataException = exception.InnerException?.InnerException;
        Assert.NotNull(odataException);
        Assert.Equal(string.Format(SRResources.ResourceTypeNotInModel, typeof(NotInModelAddress).FullName),
            odataException.Message);
    }

    [Theory]
    [InlineData("PropertyIsNotCollectionContactInfo/DeclaredAddress/any(e:e eq 'Wujiang Road')", "DeclaredAddress", "PropertyIsNotCollectionContactInfo")]
    [InlineData("PropertyIsNotCollectionContactInfo/DynamicAddress/any(e:e eq 'Wujiang Road')", "DynamicAddress", "PropertyIsNotCollectionContactInfo")]
    [InlineData("PropertyIsNotCollectionContactInfo/DeclaredAddress/all(e:e eq 'Wujiang Road')", "DeclaredAddress", "PropertyIsNotCollectionContactInfo")]
    [InlineData("PropertyIsNotCollectionContactInfo/DynamicAddress/all(e:e eq 'Wujiang Road')", "DynamicAddress", "PropertyIsNotCollectionContactInfo")]
    [InlineData("DeclaredContactInfo/DynamicAddress/any(e:e eq 'Wujiang Road')", "DynamicAddress", "ContactInfo")]
    [InlineData("DeclaredContactInfo/DynamicAddress/all(e:e eq 'Wujiang Road')", "DynamicAddress", "ContactInfo")]
    public async Task TestAnyAndAllOnSingleValuedDynamicPropertyAsync(string filterExpr, string propertyName, string typeName)
    {
        var queryUrl = $"odata/BadCustomers?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await client.SendAsync(request);
        });

        // Assert
        Assert.NotNull(exception);
        var odataException = exception.InnerException?.InnerException;
        Assert.NotNull(odataException);
        Assert.Equal(
            string.Format(
                SRResources.PropertyIsNotCollection,
                typeof(Address).FullName,
                propertyName,
                $"Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.{typeName}"),
            odataException.Message);
    }

    [Theory]
    [InlineData("DynamicSingleValuedProperty eq 'a'")]
    [InlineData("DeclaredSingleValuedProperty eq 'a'")]
    public async Task TestEqualOperatorOnSingleValuedPropertyAsync(string filterExpr)
    {
        // Arrange
        var queryUrl = $"odata/Products?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();
        Assert.EndsWith(
            "/$metadata#Products\"," +
            "\"value\":[" +
            "{\"Id\":\"accdx34g-3d16-473e-9251-378c68de859e\"," +
            "\"DeclaredSingleValuedProperty\":\"a\"," +
            "\"DeclaredCollectionValuedProperty\":[1,2,3]," +
            "\"DynamicSingleValuedProperty\":\"a\"," +
            "\"DynamicCollectionValuedProperty@odata.type\":\"#Collection(Int32)\"," +
            "\"DynamicCollectionValuedProperty\":[1,2,3]}]}",
            result);
    }

    [Theory]
    [InlineData("DynamicSingleValuedProperty ne 'a'")]
    [InlineData("DeclaredSingleValuedProperty ne 'a'")]
    public async Task TestNotEqualOperatorOnSingleValuedPropertyAsync(string filterExpr)
    {
        // Arrange
        var queryUrl = $"odata/Products?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();
        Assert.EndsWith(
            "/$metadata#Products\"," +
            "\"value\":[" +
            "{\"Id\":\"abc8fh64-3d16-473e-9251-378c68de859f\"," +
            "\"DeclaredSingleValuedProperty\":\"b\"," +
            "\"DeclaredCollectionValuedProperty\":[2,3,4]," +
            "\"DynamicSingleValuedProperty\":\"b\"," +
            "\"DynamicCollectionValuedProperty@odata.type\":\"#Collection(Int32)\"," +
            "\"DynamicCollectionValuedProperty\":[2,3,4]}," +
            "{\"Id\":\"abc8fh64-3d16-473e-9251-378c68de859g\"," +
            "\"DeclaredSingleValuedProperty\":\"c\"," +
            "\"DeclaredCollectionValuedProperty\":[3,4,5]," +
            "\"DynamicSingleValuedProperty\":\"c\"," +
            "\"DynamicCollectionValuedProperty@odata.type\":\"#Collection(Int32)\"," +
            "\"DynamicCollectionValuedProperty\":[3,4,5]}]}",
            result);
    }

    [Theory]
    [InlineData("DynamicSingleValuedProperty in ('b','c')")]
    [InlineData("DeclaredSingleValuedProperty in ('b','c')")]
    public async Task TestInOperatorOnSingleValuedPropertyAsync(string filterExpr)
    {
        // Arrange
        var queryUrl = $"odata/Products?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();
        Assert.EndsWith(
            "/$metadata#Products\"," +
            "\"value\":[" +
            "{\"Id\":\"abc8fh64-3d16-473e-9251-378c68de859f\"," +
            "\"DeclaredSingleValuedProperty\":\"b\"," +
            "\"DeclaredCollectionValuedProperty\":[2,3,4]," +
            "\"DynamicSingleValuedProperty\":\"b\"," +
            "\"DynamicCollectionValuedProperty@odata.type\":\"#Collection(Int32)\"," +
            "\"DynamicCollectionValuedProperty\":[2,3,4]}," +
            "{\"Id\":\"abc8fh64-3d16-473e-9251-378c68de859g\"," +
            "\"DeclaredSingleValuedProperty\":\"c\"," +
            "\"DeclaredCollectionValuedProperty\":[3,4,5]," +
            "\"DynamicSingleValuedProperty\":\"c\"," +
            "\"DynamicCollectionValuedProperty@odata.type\":\"#Collection(Int32)\"," +
            "\"DynamicCollectionValuedProperty\":[3,4,5]}]}",
            result);
    }

    [Theory]
    [InlineData("DynamicCollectionValuedProperty/any(d:d eq 4)")]
    [InlineData("DeclaredCollectionValuedProperty/any(d:d eq 4)")]
    public async Task TestAnyOperatorOnCollectionValuedPropertyAsync(string filterExpr)
    {
        // Arrange
        var queryUrl = $"odata/Products?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();
        Assert.EndsWith(
            "/$metadata#Products\"," +
            "\"value\":[" +
            "{\"Id\":\"abc8fh64-3d16-473e-9251-378c68de859f\"," +
            "\"DeclaredSingleValuedProperty\":\"b\"," +
            "\"DeclaredCollectionValuedProperty\":[2,3,4]," +
            "\"DynamicSingleValuedProperty\":\"b\"," +
            "\"DynamicCollectionValuedProperty@odata.type\":\"#Collection(Int32)\"," +
            "\"DynamicCollectionValuedProperty\":[2,3,4]}," +
            "{\"Id\":\"abc8fh64-3d16-473e-9251-378c68de859g\"," +
            "\"DeclaredSingleValuedProperty\":\"c\"," +
            "\"DeclaredCollectionValuedProperty\":[3,4,5]," +
            "\"DynamicSingleValuedProperty\":\"c\"," +
            "\"DynamicCollectionValuedProperty@odata.type\":\"#Collection(Int32)\"," +
            "\"DynamicCollectionValuedProperty\":[3,4,5]}]}",
            result);
    }

    [Theory]
    [InlineData("DynamicCollectionValuedProperty/all(d:d gt 2)")]
    [InlineData("DeclaredCollectionValuedProperty/all(d:d gt 2)")]
    public async Task TestAllOperatorOnCollectionValuedPropertyAsync(string filterExpr)
    {
        // Arrange
        var queryUrl = $"odata/Products?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();
        Assert.EndsWith(
            "/$metadata#Products\"," +
            "\"value\":[" +
            "{\"Id\":\"abc8fh64-3d16-473e-9251-378c68de859g\"," +
            "\"DeclaredSingleValuedProperty\":\"c\"," +
            "\"DeclaredCollectionValuedProperty\":[3,4,5]," +
            "\"DynamicSingleValuedProperty\":\"c\"," +
            "\"DynamicCollectionValuedProperty@odata.type\":\"#Collection(Int32)\"," +
            "\"DynamicCollectionValuedProperty\":[3,4,5]}]}",
            result);
    }

    [Theory]
    [InlineData("DeclaredCollectionValuedProperty/any(fruit:fruit/Name eq 'Dragon Fruit')")]
    [InlineData("DeclaredCollectionValuedProperty/any(fruit:fruit/Family eq 'Cactaceae')")]
    [InlineData("DynamicCollectionValuedProperty/any(fruit:fruit/Name eq 'Dragon Fruit')")]
    [InlineData("DynamicCollectionValuedProperty/any(fruit:fruit/Family eq 'Cactaceae')")]
    public async Task TestAnyOperatorOnCollectionValuedComplexPropertyAsync(string filterExpr)
    {
        // Arrange
        var queryUrl = $"odata/Baskets?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();
        Assert.EndsWith("/$metadata#Baskets\"," +
            "\"value\":[" +
            "{\"Id\":1," +
            "\"DeclaredCollectionValuedProperty\":[{\"Name\":\"Apple\",\"Family\":\"Rosaceae\"},{\"Name\":\"Dragon Fruit\",\"Family\":\"Cactaceae\"}]," +
            "\"DynamicCollectionValuedProperty@odata.type\":\"#Collection(Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Fruit)\"," +
            "\"DynamicCollectionValuedProperty\":[{\"Name\":\"Apple\",\"Family\":\"Rosaceae\"},{\"Name\":\"Dragon Fruit\",\"Family\":\"Cactaceae\"}]}]}",
            result);
    }

    [Theory]
    [InlineData("DeclaredLiteralInfo/DeclaredBooleanProperty eq true")]
    [InlineData("DeclaredLiteralInfo/DeclaredByteProperty eq 1")]
    [InlineData("DeclaredLiteralInfo/DeclaredSignedByteProperty eq 9")]
    [InlineData("DeclaredLiteralInfo/DeclaredInt16Property eq 7")]
    [InlineData("DeclaredLiteralInfo/DeclaredInt32Property eq 13")]
    [InlineData("DeclaredLiteralInfo/DeclaredInt64Property eq 6078747774547")]
    [InlineData("DeclaredLiteralInfo/DeclaredSingleProperty eq 3.142")]
    [InlineData("DeclaredLiteralInfo/DeclaredDoubleProperty eq 3.14159265359")]
    [InlineData("DeclaredLiteralInfo/DeclaredDecimalProperty eq 7654321")]
    [InlineData("DeclaredLiteralInfo/DeclaredGuidProperty eq 00000017-003b-003b-0001-020304050607")]
    [InlineData("DeclaredLiteralInfo/DeclaredStringProperty eq 'Foo'")]
    [InlineData("DeclaredLiteralInfo/DeclaredTimeSpanProperty eq Duration'PT23H59M59S'")]
    [InlineData("DeclaredLiteralInfo/DeclaredTimeOfDayProperty eq 23:59:59.0000000")]
    [InlineData("DeclaredLiteralInfo/DeclaredDateProperty eq 1970-01-01")]
    [InlineData("DeclaredLiteralInfo/DeclaredDateTimeOffsetProperty eq 1970-12-31T23:59:59Z")]
    [InlineData("DeclaredLiteralInfo/DeclaredEnumProperty eq Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Color'Black'")]
    [InlineData("DeclaredLiteralInfo/DeclaredByteArrayProperty eq Binary'AQIDBAUGBwgJAA=='")]
    [InlineData("DeclaredLiteralInfo/DynamicBooleanProperty eq true")]
    [InlineData("DeclaredLiteralInfo/DynamicByteProperty eq 1)", Skip = "Disambiguation for byte not supported")]
    [InlineData("DeclaredLiteralInfo/DynamicSignedByteProperty eq 9)", Skip = "Disambiguation for sbyte not supported")]
    [InlineData("DeclaredLiteralInfo/DynamicInt16Property eq 7)", Skip = "Disambiguation for short not supported")]
    [InlineData("DeclaredLiteralInfo/DynamicInt32Property eq 13")]
    [InlineData("DeclaredLiteralInfo/DynamicInt64Property eq 6078747774547")]
    [InlineData("DeclaredLiteralInfo/DynamicSingleProperty eq 3.142")]
    [InlineData("DeclaredLiteralInfo/DynamicDoubleProperty eq 3.14159265359")]
    [InlineData("DeclaredLiteralInfo/DynamicDecimalProperty eq 7654321m")] // Suffix makes this work
    [InlineData("DeclaredLiteralInfo/DynamicGuidProperty eq 00000017-003b-003b-0001-020304050607")]
    [InlineData("DeclaredLiteralInfo/DynamicStringProperty eq 'Foo'")]
    [InlineData("DeclaredLiteralInfo/DynamicTimeSpanProperty eq Duration'PT23H59M59S'")]
    [InlineData("DeclaredLiteralInfo/DynamicTimeOfDayProperty eq 23:59:59.0000000")]
    [InlineData("DeclaredLiteralInfo/DynamicDateProperty eq 1970-01-01")]
    [InlineData("DeclaredLiteralInfo/DynamicDateTimeOffsetProperty eq 1970-12-31T23:59:59Z")]
    [InlineData("DeclaredLiteralInfo/DynamicEnumProperty eq Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Color'Black'")]
    [InlineData("DeclaredLiteralInfo/DynamicByteArrayProperty eq Binary'AQIDBAUGBwgJAA=='")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DeclaredBooleanProperty eq true)")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DeclaredByteProperty eq 1)")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DeclaredSignedByteProperty eq 9)")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DeclaredInt16Property eq 7)")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DeclaredInt32Property eq 13)")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DeclaredInt64Property eq 6078747774547)")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DeclaredSingleProperty eq 3.142)")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DeclaredDoubleProperty eq 3.14159265359)")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DeclaredDecimalProperty eq 7654321)")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DeclaredGuidProperty eq 00000017-003b-003b-0001-020304050607)")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DeclaredStringProperty eq 'Foo')")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DeclaredTimeSpanProperty eq Duration'PT23H59M59S')")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DeclaredTimeOfDayProperty eq 23:59:59.0000000)")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DeclaredDateProperty eq 1970-01-01)")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DeclaredDateTimeOffsetProperty eq 1970-12-31T23:59:59Z)")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DeclaredEnumProperty eq Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Color'Black')")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DeclaredByteArrayProperty eq Binary'AQIDBAUGBwgJAA==')")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DynamicBooleanProperty eq true)")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DynamicByteProperty eq 1)", Skip = "Disambiguation for byte not supported")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DynamicSignedByteProperty eq 9)", Skip = "Disambiguation for sbyte not supported")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DynamicInt16Property eq 7)", Skip = "Disambiguation for short not supported)")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DynamicInt32Property eq 13)")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DynamicInt64Property eq 6078747774547)")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DynamicSingleProperty eq 3.142)")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DynamicDoubleProperty eq 3.14159265359)")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DynamicDecimalProperty eq 7654321m)")] // Suffix makes this work
    [InlineData("DeclaredLiteralInfos/any(d:d/DynamicGuidProperty eq 00000017-003b-003b-0001-020304050607)")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DynamicStringProperty eq 'Foo')")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DynamicTimeSpanProperty eq Duration'PT23H59M59S')")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DynamicTimeOfDayProperty eq 23:59:59.0000000)")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DynamicDateProperty eq 1970-01-01)")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DynamicDateTimeOffsetProperty eq 1970-12-31T23:59:59Z)")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DynamicEnumProperty eq Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Color'Black')")]
    [InlineData("DeclaredLiteralInfos/any(d:d/DynamicByteArrayProperty eq Binary'AQIDBAUGBwgJAA==')")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DeclaredBooleanProperty eq true)")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DeclaredByteProperty eq 1)")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DeclaredSignedByteProperty eq 9)")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DeclaredInt16Property eq 7)")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DeclaredInt32Property eq 13)")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DeclaredInt64Property eq 6078747774547)")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DeclaredSingleProperty eq 3.142)")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DeclaredDoubleProperty eq 3.14159265359)")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DeclaredDecimalProperty eq 7654321)")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DeclaredGuidProperty eq 00000017-003b-003b-0001-020304050607)")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DeclaredStringProperty eq 'Foo')")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DeclaredTimeSpanProperty eq Duration'PT23H59M59S')")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DeclaredTimeOfDayProperty eq 23:59:59.0000000)")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DeclaredDateProperty eq 1970-01-01)")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DeclaredDateTimeOffsetProperty eq 1970-12-31T23:59:59Z)")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DeclaredEnumProperty eq Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Color'Black')")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DeclaredByteArrayProperty eq Binary'AQIDBAUGBwgJAA==')")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DynamicBooleanProperty eq true)")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DynamicByteProperty eq 1)", Skip = "Disambiguation for byte not supported)")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DynamicSignedByteProperty eq 9)", Skip = "Disambiguation for sbyte not supported")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DynamicInt16Property eq 7)", Skip = "Disambiguation for short not supported")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DynamicInt32Property eq 13)")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DynamicInt64Property eq 6078747774547)")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DynamicSingleProperty eq 3.142)")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DynamicDoubleProperty eq 3.14159265359)")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DynamicDecimalProperty eq 7654321m)")] // Suffix makes this work
    [InlineData("DeclaredLiteralInfos/all(d:d/DynamicGuidProperty eq 00000017-003b-003b-0001-020304050607)")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DynamicStringProperty eq 'Foo')")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DynamicTimeSpanProperty eq Duration'PT23H59M59S')")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DynamicTimeOfDayProperty eq 23:59:59.0000000)")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DynamicDateProperty eq 1970-01-01)")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DynamicDateTimeOffsetProperty eq 1970-12-31T23:59:59Z)")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DynamicEnumProperty eq Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Color'Black')")]
    [InlineData("DeclaredLiteralInfos/all(d:d/DynamicByteArrayProperty eq Binary'AQIDBAUGBwgJAA==')")]
    [InlineData("DynamicLiteralInfo/DeclaredBooleanProperty eq true")]
    [InlineData("DynamicLiteralInfo/DeclaredByteProperty eq 1)", Skip = "Disambiguation for byte not supported")]
    [InlineData("DynamicLiteralInfo/DeclaredSignedByteProperty eq 9)", Skip = "Disambiguation for sbyte not supported")]
    [InlineData("DynamicLiteralInfo/DeclaredInt16Property eq 7)", Skip = "Disambiguation for short not supported")]
    [InlineData("DynamicLiteralInfo/DeclaredInt32Property eq 13")]
    [InlineData("DynamicLiteralInfo/DeclaredInt64Property eq 6078747774547")]
    [InlineData("DynamicLiteralInfo/DeclaredSingleProperty eq 3.142")]
    [InlineData("DynamicLiteralInfo/DeclaredDoubleProperty eq 3.14159265359")]
    [InlineData("DynamicLiteralInfo/DeclaredDecimalProperty eq 7654321m")] // Suffix makes this work
    [InlineData("DynamicLiteralInfo/DeclaredGuidProperty eq 00000017-003b-003b-0001-020304050607")]
    [InlineData("DynamicLiteralInfo/DeclaredStringProperty eq 'Foo'")]
    [InlineData("DynamicLiteralInfo/DeclaredTimeSpanProperty eq Duration'PT23H59M59S'")]
    [InlineData("DynamicLiteralInfo/DeclaredTimeOfDayProperty eq 23:59:59.0000000")]
    [InlineData("DynamicLiteralInfo/DeclaredDateProperty eq 1970-01-01")]
    [InlineData("DynamicLiteralInfo/DeclaredDateTimeOffsetProperty eq 1970-12-31T23:59:59Z")]
    [InlineData("DynamicLiteralInfo/DeclaredEnumProperty eq Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Color'Black'")]
    [InlineData("DynamicLiteralInfo/DeclaredByteArrayProperty eq Binary'AQIDBAUGBwgJAA=='")]
    [InlineData("DynamicLiteralInfo/DynamicBooleanProperty eq true")]
    [InlineData("DynamicLiteralInfo/DynamicByteProperty eq 1)", Skip = "Disambiguation for byte not supported")]
    [InlineData("DynamicLiteralInfo/DynamicSignedByteProperty eq 9)", Skip = "Disambiguation for sbyte not supported")]
    [InlineData("DynamicLiteralInfo/DynamicInt16Property eq 7)", Skip = "Disambiguation for short not supported")]
    [InlineData("DynamicLiteralInfo/DynamicInt32Property eq 13")]
    [InlineData("DynamicLiteralInfo/DynamicInt64Property eq 6078747774547")]
    [InlineData("DynamicLiteralInfo/DynamicSingleProperty eq 3.142")]
    [InlineData("DynamicLiteralInfo/DynamicDoubleProperty eq 3.14159265359")]
    [InlineData("DynamicLiteralInfo/DynamicDecimalProperty eq 7654321m")] // Suffix makes this work
    [InlineData("DynamicLiteralInfo/DynamicGuidProperty eq 00000017-003b-003b-0001-020304050607")]
    [InlineData("DynamicLiteralInfo/DynamicStringProperty eq 'Foo'")]
    [InlineData("DynamicLiteralInfo/DynamicTimeSpanProperty eq Duration'PT23H59M59S'")]
    [InlineData("DynamicLiteralInfo/DynamicTimeOfDayProperty eq 23:59:59.0000000")]
    [InlineData("DynamicLiteralInfo/DynamicDateProperty eq 1970-01-01")]
    [InlineData("DynamicLiteralInfo/DynamicDateTimeOffsetProperty eq 1970-12-31T23:59:59Z")]
    [InlineData("DynamicLiteralInfo/DynamicEnumProperty eq Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Color'Black'")]
    [InlineData("DynamicLiteralInfo/DynamicByteArrayProperty eq Binary'AQIDBAUGBwgJAA=='")]
    [InlineData("DynamicLiteralInfos/any(d:d/DeclaredBooleanProperty eq true)")]
    [InlineData("DynamicLiteralInfos/any(d:d/DeclaredByteProperty eq 1)", Skip = "Disambiguation for byte not supported")]
    [InlineData("DynamicLiteralInfos/any(d:d/DeclaredSignedByteProperty eq 9)", Skip = "Disambiguation for sbyte not supported")]
    [InlineData("DynamicLiteralInfos/any(d:d/DeclaredInt16Property eq 7)", Skip = "Disambiguation for short not supported")]
    [InlineData("DynamicLiteralInfos/any(d:d/DeclaredInt32Property eq 13)")]
    [InlineData("DynamicLiteralInfos/any(d:d/DeclaredInt64Property eq 6078747774547)")]
    [InlineData("DynamicLiteralInfos/any(d:d/DeclaredSingleProperty eq 3.142)")]
    [InlineData("DynamicLiteralInfos/any(d:d/DeclaredDoubleProperty eq 3.14159265359)")]
    [InlineData("DynamicLiteralInfos/any(d:d/DeclaredDecimalProperty eq 7654321m)")] // Suffix makes this work
    [InlineData("DynamicLiteralInfos/any(d:d/DeclaredGuidProperty eq 00000017-003b-003b-0001-020304050607)")]
    [InlineData("DynamicLiteralInfos/any(d:d/DeclaredStringProperty eq 'Foo')")]
    [InlineData("DynamicLiteralInfos/any(d:d/DeclaredTimeSpanProperty eq Duration'PT23H59M59S')")]
    [InlineData("DynamicLiteralInfos/any(d:d/DeclaredTimeOfDayProperty eq 23:59:59.0000000)")]
    [InlineData("DynamicLiteralInfos/any(d:d/DeclaredDateProperty eq 1970-01-01)")]
    [InlineData("DynamicLiteralInfos/any(d:d/DeclaredDateTimeOffsetProperty eq 1970-12-31T23:59:59Z)")]
    [InlineData("DynamicLiteralInfos/any(d:d/DeclaredEnumProperty eq Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Color'Black')")]
    [InlineData("DynamicLiteralInfos/any(d:d/DeclaredByteArrayProperty eq Binary'AQIDBAUGBwgJAA==')")]
    [InlineData("DynamicLiteralInfos/any(d:d/DynamicBooleanProperty eq true)")]
    [InlineData("DynamicLiteralInfos/any(d:d/DynamicByteProperty eq 1)", Skip = "Disambiguation for byte not supported")]
    [InlineData("DynamicLiteralInfos/any(d:d/DynamicSignedByteProperty eq 9)", Skip = "Disambiguation for sbyte not supported")]
    [InlineData("DynamicLiteralInfos/any(d:d/DynamicInt16Property eq 7)", Skip = "Disambiguation for short not supported")]
    [InlineData("DynamicLiteralInfos/any(d:d/DynamicInt32Property eq 13)")]
    [InlineData("DynamicLiteralInfos/any(d:d/DynamicInt64Property eq 6078747774547)")]
    [InlineData("DynamicLiteralInfos/any(d:d/DynamicSingleProperty eq 3.142)")]
    [InlineData("DynamicLiteralInfos/any(d:d/DynamicDoubleProperty eq 3.14159265359)")]
    [InlineData("DynamicLiteralInfos/any(d:d/DynamicDecimalProperty eq 7654321m)")] // Suffix makes this work
    [InlineData("DynamicLiteralInfos/any(d:d/DynamicGuidProperty eq 00000017-003b-003b-0001-020304050607)")]
    [InlineData("DynamicLiteralInfos/any(d:d/DynamicStringProperty eq 'Foo')")]
    [InlineData("DynamicLiteralInfos/any(d:d/DynamicTimeSpanProperty eq Duration'PT23H59M59S')")]
    [InlineData("DynamicLiteralInfos/any(d:d/DynamicTimeOfDayProperty eq 23:59:59.0000000)")]
    [InlineData("DynamicLiteralInfos/any(d:d/DynamicDateProperty eq 1970-01-01)")]
    [InlineData("DynamicLiteralInfos/any(d:d/DynamicDateTimeOffsetProperty eq 1970-12-31T23:59:59Z)")]
    [InlineData("DynamicLiteralInfos/any(d:d/DynamicEnumProperty eq Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Color'Black')")]
    [InlineData("DynamicLiteralInfos/any(d:d/DynamicByteArrayProperty eq Binary'AQIDBAUGBwgJAA==')")]
    [InlineData("DynamicLiteralInfos/all(d:d/DeclaredBooleanProperty eq true)")]
    [InlineData("DynamicLiteralInfos/all(d:d/DeclaredByteProperty eq 1)", Skip = "Disambiguation for byte not supported")]
    [InlineData("DynamicLiteralInfos/all(d:d/DeclaredSignedByteProperty eq 9)", Skip = "Disambiguation for sbyte not supported")]
    [InlineData("DynamicLiteralInfos/all(d:d/DeclaredInt16Property eq 7)", Skip = "Disambiguation for short not supported")]
    [InlineData("DynamicLiteralInfos/all(d:d/DeclaredInt32Property eq 13)")]
    [InlineData("DynamicLiteralInfos/all(d:d/DeclaredInt64Property eq 6078747774547)")]
    [InlineData("DynamicLiteralInfos/all(d:d/DeclaredSingleProperty eq 3.142)")]
    [InlineData("DynamicLiteralInfos/all(d:d/DeclaredDoubleProperty eq 3.14159265359)")]
    [InlineData("DynamicLiteralInfos/all(d:d/DeclaredDecimalProperty eq 7654321m)")] // Suffix makes this work
    [InlineData("DynamicLiteralInfos/all(d:d/DeclaredGuidProperty eq 00000017-003b-003b-0001-020304050607)")]
    [InlineData("DynamicLiteralInfos/all(d:d/DeclaredStringProperty eq 'Foo')")]
    [InlineData("DynamicLiteralInfos/all(d:d/DeclaredTimeSpanProperty eq Duration'PT23H59M59S')")]
    [InlineData("DynamicLiteralInfos/all(d:d/DeclaredTimeOfDayProperty eq 23:59:59.0000000)")]
    [InlineData("DynamicLiteralInfos/all(d:d/DeclaredDateProperty eq 1970-01-01)")]
    [InlineData("DynamicLiteralInfos/all(d:d/DeclaredDateTimeOffsetProperty eq 1970-12-31T23:59:59Z)")]
    [InlineData("DynamicLiteralInfos/all(d:d/DeclaredEnumProperty eq Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Color'Black')")]
    [InlineData("DynamicLiteralInfos/all(d:d/DeclaredByteArrayProperty eq Binary'AQIDBAUGBwgJAA==')")]
    [InlineData("DynamicLiteralInfos/all(d:d/DynamicBooleanProperty eq true)")]
    [InlineData("DynamicLiteralInfos/all(d:d/DynamicByteProperty eq 1)", Skip = "Disambiguation for byte not supported")]
    [InlineData("DynamicLiteralInfos/all(d:d/DynamicSignedByteProperty eq 9)", Skip = "Disambiguation for sbyte not supported")]
    [InlineData("DynamicLiteralInfos/all(d:d/DynamicInt16Property eq 7)", Skip = "Disambiguation for short not supported")]
    [InlineData("DynamicLiteralInfos/all(d:d/DynamicInt32Property eq 13)")]
    [InlineData("DynamicLiteralInfos/all(d:d/DynamicInt64Property eq 6078747774547)")]
    [InlineData("DynamicLiteralInfos/all(d:d/DynamicSingleProperty eq 3.142)")]
    [InlineData("DynamicLiteralInfos/all(d:d/DynamicDoubleProperty eq 3.14159265359)")]
    [InlineData("DynamicLiteralInfos/all(d:d/DynamicDecimalProperty eq 7654321m)")] // Suffix makes this work
    [InlineData("DynamicLiteralInfos/all(d:d/DynamicGuidProperty eq 00000017-003b-003b-0001-020304050607)")]
    [InlineData("DynamicLiteralInfos/all(d:d/DynamicStringProperty eq 'Foo')")]
    [InlineData("DynamicLiteralInfos/all(d:d/DynamicTimeSpanProperty eq Duration'PT23H59M59S')")]
    [InlineData("DynamicLiteralInfos/all(d:d/DynamicTimeOfDayProperty eq 23:59:59.0000000)")]
    [InlineData("DynamicLiteralInfos/all(d:d/DynamicDateProperty eq 1970-01-01)")]
    [InlineData("DynamicLiteralInfos/all(d:d/DynamicDateTimeOffsetProperty eq 1970-12-31T23:59:59Z)")]
    [InlineData("DynamicLiteralInfos/all(d:d/DynamicEnumProperty eq Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter.Color'Black')")]
    [InlineData("DynamicLiteralInfos/all(d:d/DynamicByteArrayProperty eq Binary'AQIDBAUGBwgJAA==')")]
    public async Task TestDynamicPropertiesOfBasicLiteralTypesSupportedInFilterExpressionAsync(string filterExpr)
    {
        // Arrange
        var queryUrl = $"odata/BasicTypes?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = (await response.Content.ReadAsObject<JObject>())["value"] as JArray;
        var resource = Assert.Single(result) as JObject;
        Assert.Equal(1, resource["Id"]);
    }

    // Verifies that a single-valued segment following an open/dynamic property resolves
    // declared (modeled) and genuinely dynamic siblings, while a CLR property that is not
    // declared in the EDM model (Ignore()'d) is treated as a dynamic name only.
    [Theory]
    [InlineData("DynamicInfo/DeclaredCode eq 'DC1'", 1)] // declared in the model
    [InlineData("DynamicInfo/DeclaredCode eq 'DC2'", 2)] // declared in the model
    [InlineData("DynamicInfo/DynamicCode eq 'DynCode1'", 1)] // genuinely dynamic
    [InlineData("DynamicInfo/DynamicCode eq 'DynCode2'", 2)] // genuinely dynamic
    [InlineData("DynamicInfo/IgnoredCode eq 'BravoDyn1'", 1)] // dynamic value, not the CLR member
    [InlineData("DynamicInfo/IgnoredCode eq 'RealDyn2'", 2)] // dynamic value, not the CLR member
    public async Task TestNestedDynamicSegmentResolvesDeclaredAndDynamicProperties(string filterExpr, int expectedId)
    {
        // Arrange
        var queryUrl = $"odata/Catalogs?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = (await response.Content.ReadAsObject<JObject>())["value"] as JArray;
        var resource = Assert.Single(result) as JObject;
        Assert.Equal(expectedId, resource["Id"]);
    }

    // Verifies that a CLR property excluded from the EDM model is not bound from the CLR
    // instance on a nested single-valued segment. The matching CLR values (BravoDyn1 -> 1,
    // RealDyn2 -> 2 are the dynamic values; ZCode1/ACode2 are the CLR member values) never
    // surface, so filtering on a CLR member value yields no resources.
    [Theory]
    [InlineData("DynamicInfo/IgnoredCode eq 'ZCode1'")]
    [InlineData("DynamicInfo/IgnoredCode eq 'ACode2'")]
    public async Task TestNestedDynamicSegmentDoesNotResolveUnmodeledClrProperty(string filterExpr)
    {
        // Arrange
        var queryUrl = $"odata/Catalogs?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = (await response.Content.ReadAsObject<JObject>())["value"] as JArray;
        Assert.Empty(result);
    }

    // A nested declared property whose EDM name (EdmRenamedCode) differs from its CLR name
    // (RenamedCode) binds to the CLR member value when addressed by its EDM name.
    [Fact]
    public async Task TestNestedDynamicSegmentResolvesDeclaredPropertyWithRenamedEdmName()
    {
        // Arrange - EdmRenamedCode (EDM name) must bind to the RenamedCode CLR member.
        var queryUrl = "odata/Catalogs?$filter=DynamicInfo/EdmRenamedCode eq 'AlphaRenamed'";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = (await response.Content.ReadAsObject<JObject>())["value"] as JArray;
        var resource = Assert.Single(result) as JObject;
        Assert.Equal(1, resource["Id"]);
    }

    // Verifies that a nested CLR-member access on a dynamic property whose runtime value is a
    // primitive resolves against that runtime type rather than being rejected at the model
    // boundary. DynamicName carries the strings "Dyn One"/"Dyn Two" (both length 7) for catalogs
    // 1 and 2, so DynamicName/Length reaches the nested single-valued open-access sink with a
    // string value and string.Length is read, selectively returning exactly those two catalogs.
    [Fact]
    public async Task TestFilterNestedClrMemberAccessOnDynamicPrimitiveValueResolves()
    {
        // Arrange
        var queryUrl = "odata/Catalogs?$filter=DynamicName/Length eq 7";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = (await response.Content.ReadAsObject<JObject>())["value"] as JArray;
        var ids = result.Select(resource => (int)resource["Id"]).OrderBy(id => id).ToArray();
        Assert.Equal(new[] { 1, 2 }, ids);
    }

    // Verifies $orderby over a nested dynamic segment uses the dynamic value (or absence)
    // rather than the unmodeled CLR member. Scoped to the two fully-populated catalogs so the
    // ordering proof is exact: ordering by the unmodeled name uses the dynamic values
    // (BravoDyn1 < RealDyn2 => [1, 2]); ordering by declared/dynamic members descending yields [2, 1].
    [Theory]
    [InlineData("DynamicInfo/IgnoredCode", 1, 2)]
    [InlineData("DynamicInfo/IgnoredCode desc", 2, 1)]
    [InlineData("DynamicInfo/DeclaredCode desc", 2, 1)]
    [InlineData("DynamicInfo/DynamicCode desc", 2, 1)]
    public async Task TestOrderByOnNestedDynamicSegment(string orderByExpr, int firstId, int secondId)
    {
        // Arrange
        var queryUrl = $"odata/Catalogs?$filter=Id le 2&$orderby={orderByExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = (await response.Content.ReadAsObject<JObject>())["value"] as JArray;
        Assert.Equal(2, result.Count);
        Assert.Equal(firstId, (result[0] as JObject)["Id"]);
        Assert.Equal(secondId, (result[1] as JObject)["Id"]);
    }

    // $orderby over the renamed nested property (EDM name EdmRenamedCode) orders by the CLR member
    // value. Scoped to the two seeded catalogs: "AlphaRenamed" < "BetaRenamed" => asc [1, 2], desc [2, 1].
    [Theory]
    [InlineData("DynamicInfo/EdmRenamedCode", 1, 2)]
    [InlineData("DynamicInfo/EdmRenamedCode desc", 2, 1)]
    public async Task TestOrderByOnNestedDynamicSegmentWithRenamedEdmName(string orderByExpr, int firstId, int secondId)
    {
        // Arrange
        var queryUrl = $"odata/Catalogs?$filter=Id le 2&$orderby={orderByExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = (await response.Content.ReadAsObject<JObject>())["value"] as JArray;
        Assert.Equal(2, result.Count);
        Assert.Equal(firstId, (result[0] as JObject)["Id"]);
        Assert.Equal(secondId, (result[1] as JObject)["Id"]);
    }

    // Verifies that a collection segment following an open/dynamic property resolves declared
    // (modeled) and genuinely dynamic collections via any().
    [Theory]
    [InlineData("DynamicInfo/DeclaredTags/any(t:t/DeclaredLabel eq 'DeclaredTag1')", 1)] // declared in the model
    [InlineData("DynamicInfo/DeclaredTags/any(t:t/DeclaredLabel eq 'DeclaredTag2')", 2)] // declared in the model
    [InlineData("DynamicInfo/DynamicTags/any(t:t/DeclaredLabel eq 'DynTag1')", 1)] // genuinely dynamic
    public async Task TestNestedDynamicCollectionSegmentResolvesDeclaredAndDynamicProperties(string filterExpr, int expectedId)
    {
        // Arrange
        var queryUrl = $"odata/Catalogs?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = (await response.Content.ReadAsObject<JObject>())["value"] as JArray;
        var resource = Assert.Single(result) as JObject;
        Assert.Equal(expectedId, resource["Id"]);
    }

    // Verifies that a CLR collection property excluded from the EDM model is not bound from
    // the CLR instance on a nested collection segment; any() over it matches nothing.
    [Theory]
    [InlineData("DynamicInfo/IgnoredTags/any(t:t/DeclaredLabel eq 'HiddenTag1')")]
    [InlineData("DynamicInfo/IgnoredTags/any(t:t/DeclaredLabel eq 'HiddenTag2')")]
    public async Task TestNestedDynamicCollectionSegmentDoesNotResolveUnmodeledClrProperty(string filterExpr)
    {
        // Arrange
        var queryUrl = $"odata/Catalogs?$filter={filterExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = (await response.Content.ReadAsObject<JObject>())["value"] as JArray;
        Assert.Empty(result);
    }

    // Verifies $apply=groupby over a declared property of an open entity projects its values.
    [Fact]
    public async Task TestApplyGroupByResolvesDeclaredProperty()
    {
        // Arrange
        var queryUrl = "odata/Catalogs?$apply=groupby((DeclaredName))";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();
        Assert.Contains("Visible One", result);
        Assert.Contains("Visible Two", result);
    }

    // Verifies $apply=groupby over a genuinely dynamic property of an open entity projects its values.
    [Fact]
    public async Task TestApplyGroupByResolvesDynamicProperty()
    {
        // Arrange
        var queryUrl = "odata/Catalogs?$apply=groupby((DynamicName))";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();
        Assert.Contains("Dyn One", result);
        Assert.Contains("Dyn Two", result);
    }

    // Verifies $apply=groupby over a CLR property excluded from the EDM model does not project
    // the CLR member values; the unmodeled name is treated as a dynamic name only.
    [Fact]
    public async Task TestApplyGroupByDoesNotResolveUnmodeledClrProperty()
    {
        // Arrange
        var queryUrl = "odata/Catalogs?$apply=groupby((IgnoredName))";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Hidden One", result);
        Assert.DoesNotContain("Hidden Two", result);
    }

    // Baseline (no query options): querying the entity set returns declared and dynamic
    // properties, while CLR members excluded from the EDM model (Ignore()'d) are not
    // serialized. A dynamic-container entry that shares a name with an unmodeled CLR member
    // surfaces the dynamic value, consistent with the model.
    [Fact]
    public async Task TestQueryingCatalogsWithoutQueryOptionsReturnsDeclaredAndDynamicProperties()
    {
        // Arrange
        var queryUrl = "odata/Catalogs";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var content = await response.Content.ReadAsStringAsync();
        var value = JObject.Parse(content)["value"] as JArray;

        // Catalog 1: declared and dynamic properties are present.
        var catalog1 = value.Children<JObject>().Single(o => (int)o["Id"] == 1);
        Assert.Equal("Visible One", (string)catalog1["DeclaredName"]);
        Assert.Equal("Dyn One", (string)catalog1["DynamicName"]);

        var info1 = catalog1["DynamicInfo"] as JObject;
        Assert.Equal("DC1", (string)info1["DeclaredCode"]);
        Assert.Equal("DynCode1", (string)info1["DynamicCode"]);
        // The name "IgnoredCode" surfaces the dynamic-container value, not the CLR member value.
        Assert.Equal("BravoDyn1", (string)info1["IgnoredCode"]);
        Assert.Equal("DeclaredTag1", (string)((info1["DeclaredTags"] as JArray)[0] as JObject)["DeclaredLabel"]);
        Assert.Equal("DynTag1", (string)((info1["DynamicTags"] as JArray)[0] as JObject)["DeclaredLabel"]);
        // The Ignore()'d CLR collection is not serialized.
        Assert.Null(info1["IgnoredTags"]);

        // Catalog 2: declared and dynamic properties are present.
        var catalog2 = value.Children<JObject>().Single(o => (int)o["Id"] == 2);
        Assert.Equal("Visible Two", (string)catalog2["DeclaredName"]);
        Assert.Equal("Dyn Two", (string)catalog2["DynamicName"]);

        var info2 = catalog2["DynamicInfo"] as JObject;
        Assert.Equal("DC2", (string)info2["DeclaredCode"]);
        Assert.Equal("DynCode2", (string)info2["DynamicCode"]);
        Assert.Equal("RealDyn2", (string)info2["IgnoredCode"]);

        // CLR members excluded from the model never appear in the payload.
        Assert.DoesNotContain("Hidden One", content);
        Assert.DoesNotContain("Hidden Two", content);
        Assert.DoesNotContain("ZCode1", content);
        Assert.DoesNotContain("ACode2", content);
        Assert.DoesNotContain("HiddenTag1", content);
        Assert.DoesNotContain("HiddenTag2", content);
    }

    // Baseline with rows whose dynamic-property container is null or empty,
    // whose nested dynamic complex is null, or which carry null dynamic values are serialized
    // without error. Declared members (including an empty string) are present, while CLR members
    // excluded from the model never surface.
    [Fact]
    public async Task TestQueryingCatalogsWithNullAndEmptyDynamicContainersIsConsistent()
    {
        // Arrange
        var queryUrl = "odata/Catalogs";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var content = await response.Content.ReadAsStringAsync();
        var value = JObject.Parse(content)["value"] as JArray;

        // Null dynamic-property container: only declared members are serialized.
        var catalog3 = value.Children<JObject>().Single(o => (int)o["Id"] == 3);
        Assert.Equal("Visible Three", (string)catalog3["DeclaredName"]);
        Assert.Null(catalog3["DynamicName"]);
        Assert.Null(catalog3["DynamicInfo"]);

        // Empty dynamic-property container and an empty declared string.
        var catalog4 = value.Children<JObject>().Single(o => (int)o["Id"] == 4);
        Assert.Equal("", (string)catalog4["DeclaredName"]);
        Assert.Null(catalog4["DynamicName"]);
        Assert.Null(catalog4["DynamicInfo"]);

        // Null values within the nested dynamic complex and empty collections: the complex is
        // still present, and the unmodeled CLR IgnoredCode value is never surfaced from it.
        var catalog5 = value.Children<JObject>().Single(o => (int)o["Id"] == 5);
        Assert.Equal("Visible Five", (string)catalog5["DeclaredName"]);
        var info5 = catalog5["DynamicInfo"] as JObject;
        Assert.NotNull(info5);
        Assert.Null(info5["IgnoredCode"]);

        // Null nested dynamic complex value: the declared members are still serialized.
        var catalog6 = value.Children<JObject>().Single(o => (int)o["Id"] == 6);
        Assert.Equal("Visible Six", (string)catalog6["DeclaredName"]);

        // CLR members excluded from the model never appear in the payload.
        Assert.DoesNotContain("Hidden Three", content);
        Assert.DoesNotContain("Hidden Four", content);
        Assert.DoesNotContain("Hidden Six", content);
        Assert.DoesNotContain("ZCode5", content);
    }

    // Verifies $orderby over a nested dynamic segment remains stable when many rows have a null or
    // absent dynamic value (null container, null nested complex, or a null dynamic entry): the
    // request succeeds and the two rows that carry dynamic values keep their relative order
    // (BravoDyn1 < RealDyn2 => catalog 1 before catalog 2).
    [Fact]
    public async Task TestOrderByOnNestedDynamicSegmentWithNullValuesIsStable()
    {
        // Arrange
        var queryUrl = "odata/Catalogs?$orderby=DynamicInfo/IgnoredCode";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = (await response.Content.ReadAsObject<JObject>())["value"] as JArray;
        var ids = result.Select(t => (int)t["Id"]).ToList();
        Assert.Contains(1, ids);
        Assert.Contains(2, ids);
        Assert.True(ids.IndexOf(1) < ids.IndexOf(2));
    }

    // A dynamic property whose runtime value is a NON-OPEN modeled complex type: addressing a CLR
    // member excluded from the model via [NotMapped] must not bind (and expose) the CLR member.
    // Consistent with a dynamic segment on any non-open modeled type, it is rejected with
    // TypeMustBeOpenType rather than leaking the SecretSummaryCode value ("TopSecret7").
    [Fact]
    public async Task TestNestedDynamicSegmentOnNonOpenModeledTypeDoesNotExposeNotMappedClrProperty()
    {
        // Arrange
        var queryUrl = "odata/Catalogs?$filter=DynamicSummary/SecretSummaryCode eq 'TopSecret7'";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await client.SendAsync(request);
        });

        // Assert
        Assert.NotNull(exception);
        var odataException = exception.InnerException?.InnerException;
        Assert.NotNull(odataException);
        Assert.Equal(string.Format(SRResources.TypeMustBeOpenType, typeof(CatalogSummary).FullName),
            odataException.Message);
    }

    // Positive control for the model-boundary rejection above: the DECLARED member on the same
    // NON-OPEN modeled complex type binds by its EDM name and returns the catalog, proving the
    // rejection is specific to the excluded [NotMapped] member, not the non-open type as a whole.
    [Fact]
    public async Task TestNestedDynamicSegmentOnNonOpenModeledTypeResolvesDeclaredClrProperty()
    {
        // Arrange
        var queryUrl = "odata/Catalogs?$filter=DynamicSummary/DeclaredSummaryCode eq 'SumCode7'";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = (await response.Content.ReadAsObject<JObject>())["value"] as JArray;
        var resource = Assert.Single(result) as JObject;
        Assert.Equal(7, resource["Id"]);
    }

    // The [NotMapped] member is excluded from the model, so a plain projection of the catalog whose
    // dynamic container holds the non-open complex serializes the declared member ("SumCode7") but
    // never the SecretSummaryCode name or its value ("TopSecret7").
    [Fact]
    public async Task TestQueryingCatalogWithNonOpenDynamicComplexDoesNotSerializeNotMappedMember()
    {
        // Arrange
        var queryUrl = "odata/Catalogs?$filter=Id eq 7";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("SumCode7", content);
        Assert.DoesNotContain("TopSecret7", content);
        Assert.DoesNotContain("SecretSummaryCode", content);
    }

    // $orderby over a nested dynamic segment whose runtime value is a NON-OPEN modeled type must not
    // bind the excluded [NotMapped] member; like $filter, the segment is rejected at the model
    // boundary with TypeMustBeOpenType rather than exposing (ordering by) the CLR value.
    [Fact]
    public async Task TestOrderByNestedDynamicSegmentOnNonOpenModeledTypeDoesNotExposeNotMappedClrProperty()
    {
        // Arrange
        var queryUrl = "odata/Catalogs?$filter=Id eq 7&$orderby=DynamicSummary/SecretSummaryCode";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await client.SendAsync(request);
        });

        // Assert
        Assert.NotNull(exception);
        var odataException = exception.InnerException?.InnerException;
        Assert.NotNull(odataException);
        Assert.Equal(string.Format(SRResources.TypeMustBeOpenType, typeof(CatalogSummary).FullName),
            odataException.Message);
    }

    // Positive control for the $orderby rejection above: ordering by the DECLARED member of the
    // non-open modeled type uses the CLR value. Scoped to the two seeded catalogs so the ordering is
    // exact: "SumCode7" < "SumCode8" => asc [7, 8], desc [8, 7].
    [Theory]
    [InlineData("DynamicSummary/DeclaredSummaryCode", 7, 8)]
    [InlineData("DynamicSummary/DeclaredSummaryCode desc", 8, 7)]
    public async Task TestOrderByNestedDynamicSegmentOnNonOpenModeledTypeResolvesDeclaredClrProperty(string orderByExpr, int firstId, int secondId)
    {
        // Arrange
        var queryUrl = $"odata/Catalogs?$filter=Id ge 7&$orderby={orderByExpr}";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = (await response.Content.ReadAsObject<JObject>())["value"] as JArray;
        Assert.Equal(2, result.Count);
        Assert.Equal(firstId, (result[0] as JObject)["Id"]);
        Assert.Equal(secondId, (result[1] as JObject)["Id"]);
    }

    // $apply=filter reuses the filter binder: an excluded [NotMapped] member on a nested non-open
    // modeled type is rejected with TypeMustBeOpenType rather than exposed.
    [Fact]
    public async Task TestApplyFilterNestedDynamicSegmentOnNonOpenModeledTypeDoesNotExposeNotMappedClrProperty()
    {
        // Arrange
        var queryUrl = "odata/Catalogs?$apply=filter(DynamicSummary/SecretSummaryCode eq 'TopSecret7')";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await client.SendAsync(request);
        });

        // Assert
        Assert.NotNull(exception);
        var odataException = exception.InnerException?.InnerException;
        Assert.NotNull(odataException);
        Assert.Equal(string.Format(SRResources.TypeMustBeOpenType, typeof(CatalogSummary).FullName),
            odataException.Message);
    }

    // Positive control for the $apply=filter rejection above: filtering by the DECLARED member of the
    // non-open modeled type resolves and returns the catalog.
    [Fact]
    public async Task TestApplyFilterNestedDynamicSegmentOnNonOpenModeledTypeResolvesDeclaredClrProperty()
    {
        // Arrange
        var queryUrl = "odata/Catalogs?$apply=filter(DynamicSummary/DeclaredSummaryCode eq 'SumCode7')";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = (await response.Content.ReadAsObject<JObject>())["value"] as JArray;
        var resource = Assert.Single(result) as JObject;
        Assert.Equal(7, resource["Id"]);
    }
}
