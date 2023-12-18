//-----------------------------------------------------------------------------
// <copyright file="MediaTypesTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.MediaTypes
{
    public class MediaTypesTests : WebApiTestBase<MediaTypesTests>
    {
        public MediaTypesTests(WebApiTestFixture<MediaTypesTests> fixture)
            : base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            var model = MediaTypesEdmModel.GetEdmModel();

            services.ConfigureControllers(typeof(OrdersController));

            services.AddControllers().AddOData(
                options => options.EnableQueryFeatures()
                .AddRouteComponents(model));
        }

        public static IEnumerable<object[]> GetMediaTypeTestData()
        {
            return new List<object[]>
            {
                new object[]
                {
                    "application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=false",
                    "{\"@odata.context\":\"http://localhost/$metadata#Orders/$entity\",\"Id\":1,\"Amount\":130,\"TrackingNumber\":9223372036854775807}"
                },
                new object[]
                {
                    "application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=true",
                    "{\"@odata.context\":\"http://localhost/$metadata#Orders/$entity\",\"Id\":1,\"Amount\":\"130\",\"TrackingNumber\":\"9223372036854775807\"}"
                },
                new object[]
                {
                    "application/json;odata.metadata=minimal;odata.streaming=false;IEEE754Compatible=false",
                    "{\"@odata.context\":\"http://localhost/$metadata#Orders/$entity\",\"Id\":1,\"Amount\":130,\"TrackingNumber\":9223372036854775807}"
                },
                new object[]
                {
                    "application/json;odata.metadata=minimal;odata.streaming=false;IEEE754Compatible=true",
                    "{\"@odata.context\":\"http://localhost/$metadata#Orders/$entity\",\"Id\":1,\"Amount\":\"130\",\"TrackingNumber\":\"9223372036854775807\"}"
                },
                new object[]
                {
                    "application/json;odata.metadata=minimal;IEEE754Compatible=false",
                    "{\"@odata.context\":\"http://localhost/$metadata#Orders/$entity\",\"Id\":1,\"Amount\":130,\"TrackingNumber\":9223372036854775807}"
                },
                new object[]
                {
                    "application/json;odata.metadata=minimal;IEEE754Compatible=true",
                    "{\"@odata.context\":\"http://localhost/$metadata#Orders/$entity\",\"Id\":1,\"Amount\":\"130\",\"TrackingNumber\":\"9223372036854775807\"}"
                },
                new object[]
                {
                    "application/json;odata.metadata=full;odata.streaming=true;IEEE754Compatible=false",
                    $"{{\"@odata.context\":\"http://localhost/$metadata#Orders/$entity\",\"@odata.type\":\"#{typeof(Order).FullName}\",\"@odata.id\":\"http://localhost/Orders(1)\",\"@odata.editLink\":\"Orders(1)\",\"Id\":1,\"Amount@odata.type\":\"#Decimal\",\"Amount\":130,\"TrackingNumber@odata.type\":\"#Int64\",\"TrackingNumber\":9223372036854775807}}"
                },
                new object[]
                {
                    "application/json;odata.metadata=full;odata.streaming=true;IEEE754Compatible=true",
                    $"{{\"@odata.context\":\"http://localhost/$metadata#Orders/$entity\",\"@odata.type\":\"#{typeof(Order).FullName}\",\"@odata.id\":\"http://localhost/Orders(1)\",\"@odata.editLink\":\"Orders(1)\",\"Id\":1,\"Amount@odata.type\":\"#Decimal\",\"Amount\":\"130\",\"TrackingNumber@odata.type\":\"#Int64\",\"TrackingNumber\":\"9223372036854775807\"}}"
                },
                new object[]
                {
                    "application/json;odata.metadata=full;odata.streaming=false;IEEE754Compatible=false",
                    $"{{\"@odata.context\":\"http://localhost/$metadata#Orders/$entity\",\"@odata.type\":\"#{typeof(Order).FullName}\",\"@odata.id\":\"http://localhost/Orders(1)\",\"@odata.editLink\":\"Orders(1)\",\"Id\":1,\"Amount@odata.type\":\"#Decimal\",\"Amount\":130,\"TrackingNumber@odata.type\":\"#Int64\",\"TrackingNumber\":9223372036854775807}}"
                },
                new object[]
                {
                    "application/json;odata.metadata=full;odata.streaming=false;IEEE754Compatible=true",
                    $"{{\"@odata.context\":\"http://localhost/$metadata#Orders/$entity\",\"@odata.type\":\"#{typeof(Order).FullName}\",\"@odata.id\":\"http://localhost/Orders(1)\",\"@odata.editLink\":\"Orders(1)\",\"Id\":1,\"Amount@odata.type\":\"#Decimal\",\"Amount\":\"130\",\"TrackingNumber@odata.type\":\"#Int64\",\"TrackingNumber\":\"9223372036854775807\"}}"
                },
                new object[]
                {
                    "application/json;odata.metadata=full;IEEE754Compatible=false",
                    $"{{\"@odata.context\":\"http://localhost/$metadata#Orders/$entity\",\"@odata.type\":\"#{typeof(Order).FullName}\",\"@odata.id\":\"http://localhost/Orders(1)\",\"@odata.editLink\":\"Orders(1)\",\"Id\":1,\"Amount@odata.type\":\"#Decimal\",\"Amount\":130,\"TrackingNumber@odata.type\":\"#Int64\",\"TrackingNumber\":9223372036854775807}}"
                },
                new object[]
                {
                    "application/json;odata.metadata=full;IEEE754Compatible=true",
                    $"{{\"@odata.context\":\"http://localhost/$metadata#Orders/$entity\",\"@odata.type\":\"#{typeof(Order).FullName}\",\"@odata.id\":\"http://localhost/Orders(1)\",\"@odata.editLink\":\"Orders(1)\",\"Id\":1,\"Amount@odata.type\":\"#Decimal\",\"Amount\":\"130\",\"TrackingNumber@odata.type\":\"#Int64\",\"TrackingNumber\":\"9223372036854775807\"}}"
                },
                new object[]
                {
                    "application/json;odata.metadata=none;odata.streaming=true;IEEE754Compatible=false",
                    "{\"Id\":1,\"Amount\":130,\"TrackingNumber\":9223372036854775807}"
                },
                new object[]
                {
                    "application/json;odata.metadata=none;odata.streaming=true;IEEE754Compatible=true",
                    "{\"Id\":1,\"Amount\":\"130\",\"TrackingNumber\":\"9223372036854775807\"}"
                },
                new object[]
                {
                    "application/json;odata.metadata=none;odata.streaming=false;IEEE754Compatible=false",
                    "{\"Id\":1,\"Amount\":130,\"TrackingNumber\":9223372036854775807}"
                },
                new object[]
                {
                    "application/json;odata.metadata=none;odata.streaming=false;IEEE754Compatible=true",
                    "{\"Id\":1,\"Amount\":\"130\",\"TrackingNumber\":\"9223372036854775807\"}"
                },
                new object[]
                {
                    "application/json;odata.metadata=none;IEEE754Compatible=false",
                    "{\"Id\":1,\"Amount\":130,\"TrackingNumber\":9223372036854775807}"
                },
                new object[]
                {
                    "application/json;odata.metadata=none;IEEE754Compatible=true",
                    "{\"Id\":1,\"Amount\":\"130\",\"TrackingNumber\":\"9223372036854775807\"}"
                },
                new object[]
                {
                    "application/json;odata.streaming=false;IEEE754Compatible=false",
                    $"{{\"@odata.context\":\"http://localhost/$metadata#Orders/$entity\",\"Id\":1,\"Amount\":130,\"TrackingNumber\":9223372036854775807}}"
                },
                new object[]
                {
                    "application/json;odata.streaming=false;IEEE754Compatible=true",
                    $"{{\"@odata.context\":\"http://localhost/$metadata#Orders/$entity\",\"Id\":1,\"Amount\":\"130\",\"TrackingNumber\":\"9223372036854775807\"}}"
                },
                new object[]
                {
                    "application/json;odata.streaming=true;IEEE754Compatible=false",
                    $"{{\"@odata.context\":\"http://localhost/$metadata#Orders/$entity\",\"Id\":1,\"Amount\":130,\"TrackingNumber\":9223372036854775807}}"
                },
                new object[]
                {
                    "application/json;odata.streaming=true;IEEE754Compatible=true",
                    $"{{\"@odata.context\":\"http://localhost/$metadata#Orders/$entity\",\"Id\":1,\"Amount\":\"130\",\"TrackingNumber\":\"9223372036854775807\"}}"
                },
                new object[]
                {
                    "application/json;IEEE754Compatible=false",
                    "{\"@odata.context\":\"http://localhost/$metadata#Orders/$entity\",\"Id\":1,\"Amount\":130,\"TrackingNumber\":9223372036854775807}"
                },
                new object[]
                {
                    "application/json;IEEE754Compatible=true",
                    "{\"@odata.context\":\"http://localhost/$metadata#Orders/$entity\",\"Id\":1,\"Amount\":\"130\",\"TrackingNumber\":\"9223372036854775807\"}"
                },
            };
        }    

        [Theory]
        [MemberData(nameof(GetMediaTypeTestData))]
        public async Task VerifyResultForMediaTypeInAcceptHeader(string mediaType, string expected)
        {
            // Arrange
            var requestUri = "Orders(1)";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(mediaType));
            var client = CreateClient();

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();

            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(GetMediaTypeTestData))]
        public async Task VerifyResultForMediaTypeInFormatQueryOption(string mediaType, string expected)
        {
            // Arrange
            var requestUri = $"Orders(1)?$format={mediaType}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var client = CreateClient();

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=true")]
        [InlineData("application/json;odata.metadata=minimal;IEEE754Compatible=true;odata.streaming=true")]
        [InlineData("application/json;IEEE754Compatible=true;odata.metadata=minimal;odata.streaming=true")]
        public async Task VerifyPositionOfIEEE754CompatibleParameterInMediaTypeShouldNotMatter(string mediaType)
        {
            // Arrange
            var requestUri = "Orders(1)";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(mediaType));
            var client = CreateClient();

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();

            Assert.Equal("{\"@odata.context\":\"http://localhost/$metadata#Orders/$entity\",\"Id\":1,\"Amount\":\"130\",\"TrackingNumber\":\"9223372036854775807\"}", result);
        }
    }
}
