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
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
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

            services.ConfigureControllers(typeof(PeopleController));

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
    }
}
