//-----------------------------------------------------------------------------
// <copyright file="DollarFilterTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
            typeof(BasicTypesController));

        services.AddControllers().AddOData(opt =>
            opt.Filter().Select().AddRouteComponents("odata", model));
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
}
