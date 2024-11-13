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
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter
{
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
                typeof(BadVendorsController));

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
    }
}
