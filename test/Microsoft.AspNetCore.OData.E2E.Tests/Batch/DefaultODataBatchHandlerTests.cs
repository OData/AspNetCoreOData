//-----------------------------------------------------------------------------
// <copyright file="DefaultODataBatchHandlerTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.E2E.Tests.Commons;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Batch;

public class DefaultBatchHandlerCUDBatchTests : WebApiTestBase<DefaultBatchHandlerCUDBatchTests>
{
    private static IEdmModel edmModel;

    public DefaultBatchHandlerCUDBatchTests(WebApiTestFixture<DefaultBatchHandlerCUDBatchTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        services.ConfigureControllers(typeof(DefaultBatchCustomersController), typeof(DefaultBatchOrdersController));

        edmModel = GetEdmModel();
        services.AddControllers().AddOData(opt =>
        {
            opt.EnableQueryFeatures();
            opt.EnableContinueOnErrorHeader = true;
            opt.AddRouteComponents("DefaultBatch", edmModel, new DefaultODataBatchHandler());
        });
    }

    protected static void UpdateConfigure(IApplicationBuilder app)
    {
        app.UseODataBatching();
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }

    protected static IEdmModel GetEdmModel()
    {
        ODataModelBuilder builder = new ODataConventionModelBuilder();
        EntitySetConfiguration<DefaultBatchCustomer> customers = builder.EntitySet<DefaultBatchCustomer>("DefaultBatchCustomers");
        builder.EntitySet<DefaultBatchOrder>("DefaultBatchOrders");
        customers.EntityType.Collection.Action("OddCustomers").ReturnsCollectionFromEntitySet<DefaultBatchCustomer>("DefaultBatchCustomers");
        builder.MaxDataServiceVersion = builder.DataServiceVersion;
        builder.Namespace = typeof(DefaultBatchCustomer).Namespace;
        return builder.GetEdmModel();
    }

    [Fact]
    public async Task CanHandleAbsoluteAndRelativeUrls()
    {
        // Arrange
        HttpClient client = CreateClient();
        string requestUri = "DefaultBatch/$batch";

        string host = client.BaseAddress.Host;
        string relativeToServiceRootUri = "DefaultBatchCustomers";
        string relativeToHostUri = "/DefaultBatch/DefaultBatchCustomers";
        string absoluteUri = "http://localhost/DefaultBatch/DefaultBatchCustomers";

        // Act
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("multipart/mixed"));
        HttpContent content = new StringContent(@"
--batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0
Content-Type: multipart/mixed; boundary=changeset_6c67825c-8938-4f11-af6b-a25861ee53cc

--changeset_6c67825c-8938-4f11-af6b-a25861ee53cc
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 2

POST " + relativeToServiceRootUri + @" HTTP/1.1
OData-Version: 4.0;NetFx
OData-MaxVersion: 4.0;NetFx
Content-Type: application/json;odata.metadata=minimal
Accept: application/json;odata.metadata=minimal
Accept-Charset: UTF-8

{'Id':11,'Name':'MyName11'}
--changeset_6c67825c-8938-4f11-af6b-a25861ee53cc
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 3

POST " + relativeToHostUri + @" HTTP/1.1
Host: " + host + @"
OData-Version: 4.0;NetFx
OData-MaxVersion: 4.0;NetFx
Accept: application/json;odata.metadata=minimal
Accept-Charset: UTF-8
Content-Type: application/json;odata.metadata=minimal

{'Id':12,'Name':'MyName12'}
--changeset_6c67825c-8938-4f11-af6b-a25861ee53cc
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 4

POST " + absoluteUri + @" HTTP/1.1
OData-Version: 4.0;NetFx
OData-MaxVersion: 4.0;NetFx
Accept: application/json;odata.metadata=minimal
Accept-Charset: UTF-8
Content-Type: application/json;odata.metadata=minimal

{'Id':13,'Name':'MyName13'}
--changeset_6c67825c-8938-4f11-af6b-a25861ee53cc--
--batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0--
");
        // string b = await content.ReadAsStringAsync();
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/mixed; boundary=batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0");
        request.Content = content;
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        //string a = await response.Content.ReadAsStringAsync();

        var stream = await response.Content.ReadAsStreamAsync();
        IODataResponseMessage odataResponseMessage = new ODataMessageWrapper(stream, response.Content.Headers);
        int subResponseCount = 0;
        using (var messageReader = new ODataMessageReader(odataResponseMessage, new ODataMessageReaderSettings(), edmModel))
        {
            var batchReader = messageReader.CreateODataBatchReader();
            while (batchReader.Read())
            {
                switch (batchReader.State)
                {
                    case ODataBatchReaderState.Operation:
                        var operationMessage = batchReader.CreateOperationResponseMessage();
                        subResponseCount++;
                        Assert.Equal(201, operationMessage.StatusCode);
                        break;
                }
            }
        }

        Assert.Equal(3, subResponseCount);
    }

    [Fact]
    public async Task CanHandleAutomicityGroupRequestsAndUngroupedRequest_JsonBatch()
    {
        // Arrange
        HttpClient client = CreateClient();

        string requestUri = "DefaultBatch/$batch";
        string absoluteUri = "http://localhost/DefaultBatch/DefaultBatchCustomers";

        // Act
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        HttpContent content = new StringContent(@"
    {
        ""requests"": [{
                ""id"": ""1"",
                ""atomicityGroup"": ""f7de7314-2f3d-4422-b840-ada6d6de0f18"",
                ""method"": ""POST"",
                ""url"": """ + absoluteUri + @""",
                ""headers"": {
                    ""OData-Version"": ""4.0"",
                    ""Content-Type"": ""application/json;odata.metadata=minimal"",
                    ""Accept"": ""application/json;odata.metadata=minimal""
                },
                ""body"": {
                    ""Id"":11,
                    ""Name"":""CreatedByJsonBatch_11""
                }
            }, {
                ""id"": ""2"",
                ""atomicityGroup"": ""f7de7314-2f3d-4422-b840-ada6d6de0f18"",
                ""method"": ""POST"",
                ""url"": """ + absoluteUri + @""",
                ""headers"": {
                    ""OData-Version"": ""4.0"",
                    ""Content-Type"": ""application/json;odata.metadata=minimal"",
                    ""Accept"": ""application/json;odata.metadata=minimal""
                },
                ""body"": {
                    ""Id"":12,
                    ""Name"":""CreatedByJsonBatch_12""
                }
            }, {
                ""id"": ""3"",
                ""method"": ""POST"",
                ""url"": """ + absoluteUri + @""",
                ""headers"": {
                    ""OData-Version"": ""4.0"",
                    ""Content-Type"": ""application/json;odata.metadata=minimal"",
                    ""Accept"": ""application/json;odata.metadata=minimal""
                },
                ""body"": {
                    ""Id"":13,
                    ""Name"":""CreatedByJsonBatch_3""
                }
            }
        ]
    }");
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        request.Content = content;
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var stream = await response.Content.ReadAsStreamAsync();
        IODataResponseMessage odataResponseMessage = new ODataMessageWrapper(stream, response.Content.Headers);
        int subResponseCount = 0;
        using (var messageReader = new ODataMessageReader(odataResponseMessage, new ODataMessageReaderSettings(), edmModel))
        {
            var batchReader = messageReader.CreateODataBatchReader();
            while (batchReader.Read())
            {
                switch (batchReader.State)
                {
                    case ODataBatchReaderState.Operation:
                        var operationMessage = batchReader.CreateOperationResponseMessage();
                        subResponseCount++;
                        Assert.Equal(201, operationMessage.StatusCode);
                        break;
                }
            }
        }

        Assert.Equal(3, subResponseCount);
    }

    [Fact]
    public async Task CanNotContinueOnErrorWhenHeaderNotSet()
    {
        // Arrange
        HttpClient client = CreateClient();
        var requestUri = "DefaultBatch/$batch";
        string absoluteUri = "http://localhost/DefaultBatch/DefaultBatchCustomers";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("multipart/mixed"));
        HttpContent content = new StringContent(
@"--batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0
Content-Type: application/http
Content-Transfer-Encoding: binary

GET " + absoluteUri + @"(0) HTTP/1.1

--batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0
Content-Type: application/http
Content-Transfer-Encoding: binary

GET " + absoluteUri + @"(-1) HTTP/1.1

--batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0
Content-Type: application/http
Content-Transfer-Encoding: binary

GET " + absoluteUri + @"(1) HTTP/1.1

--batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0--
");
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/mixed; boundary=batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0");
        request.Content = content;

        // Act
        HttpResponseMessage response = await client.SendAsync(request);
        var stream = await response.Content.ReadAsStreamAsync();
        IODataResponseMessage odataResponseMessage = new ODataMessageWrapper(stream, response.Content.Headers);
        int subResponseCount = 0;

        // Assert
        using (var messageReader = new ODataMessageReader(odataResponseMessage, new ODataMessageReaderSettings(), edmModel))
        {
            var batchReader = messageReader.CreateODataBatchReader();
            while (batchReader.Read())
            {
                switch (batchReader.State)
                {
                    case ODataBatchReaderState.Operation:
                        var operationMessage = batchReader.CreateOperationResponseMessage();
                        subResponseCount++;
                        if (subResponseCount == 2)
                        {
                            Assert.Equal(400, operationMessage.StatusCode);
                        }
                        else
                        {
                            Assert.Equal(200, operationMessage.StatusCode);
                        }
                        break;
                }
            }
        }
        Assert.Equal(2, subResponseCount);
    }

    [Fact]
    public async Task CanContinueOnErrorWhenHeaderSet()
    {
        // Arrange
        HttpClient client = CreateClient();
        var requestUri = "DefaultBatch/$batch";
        string absoluteUri = "http://localhost/DefaultBatch/DefaultBatchCustomers";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("multipart/mixed"));
        request.Headers.Add("prefer", "odata.continue-on-error");
        HttpContent content = new StringContent(
@"--batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0
Content-Type: application/http
Content-Transfer-Encoding: binary

GET " + absoluteUri + @"(0) HTTP/1.1

--batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0
Content-Type: application/http
Content-Transfer-Encoding: binary

GET " + absoluteUri + @"(-1) HTTP/1.1

--batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0
Content-Type: application/http
Content-Transfer-Encoding: binary

GET " + absoluteUri + @"(1) HTTP/1.1

--batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0--
");
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/mixed; boundary=batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0");
        request.Content = content;

        // Act
        HttpResponseMessage response = await client.SendAsync(request);
        var stream = await response.Content.ReadAsStreamAsync();
        IODataResponseMessage odataResponseMessage = new ODataMessageWrapper(stream, response.Content.Headers);
        int subResponseCount = 0;

        // Assert
        using (var messageReader = new ODataMessageReader(odataResponseMessage, new ODataMessageReaderSettings(), edmModel))
        {
            var batchReader = messageReader.CreateODataBatchReader();
            while (batchReader.Read())
            {
                switch (batchReader.State)
                {
                    case ODataBatchReaderState.Operation:
                        var operationMessage = batchReader.CreateOperationResponseMessage();
                        subResponseCount++;
                        if (subResponseCount == 2)
                        {
                            Assert.Equal(400, operationMessage.StatusCode);
                        }
                        else
                        {
                            Assert.Equal(200, operationMessage.StatusCode);
                        }
                        break;
                }
            }
        }
        Assert.Equal(3, subResponseCount);
    }

    [Fact]
    public async Task CanHandleContentIDInRelativeUrl()
    {
        // Arrange
        HttpClient client = CreateClient();
        string requestUri = "DefaultBatch/$batch";

        string defaultBatchCustomersAbsoluteUri = "http://localhost/DefaultBatch/DefaultBatchCustomers";
        string defaultBatchOrdersAbsoluteUri = "http://localhost/DefaultBatch/DefaultBatchOrders";

        // Act
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("multipart/mixed"));
        HttpContent content = new StringContent(@"
--batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0
Content-Type: multipart/mixed; boundary=changeset_6c67825c-8938-4f11-af6b-a25861ee53cc

--changeset_6c67825c-8938-4f11-af6b-a25861ee53cc
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 1

POST " + defaultBatchCustomersAbsoluteUri + @" HTTP/1.1
OData-Version: 4.0;NetFx
OData-MaxVersion: 4.0;NetFx
Content-Type: application/json;odata.metadata=minimal
Accept: application/json;odata.metadata=minimal
Accept-Charset: UTF-8

{'Id':13,'Name':'Customer 13'}
--changeset_6c67825c-8938-4f11-af6b-a25861ee53cc
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 2

POST " + defaultBatchOrdersAbsoluteUri + @" HTTP/1.1
OData-Version: 4.0;NetFx
OData-MaxVersion: 4.0;NetFx
Accept: application/json;odata.metadata=minimal
Accept-Charset: UTF-8
Content-Type: application/json;odata.metadata=minimal

{'Id':13}
--changeset_6c67825c-8938-4f11-af6b-a25861ee53cc
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 3

POST $1/Orders/$ref HTTP/1.1
OData-Version: 4.0;NetFx
OData-MaxVersion: 4.0;NetFx
Accept: application/json;odata.metadata=minimal
Accept-Charset: UTF-8
Content-Type: application/json;odata.metadata=minimal

{'@odata.id':'$2'}
--changeset_6c67825c-8938-4f11-af6b-a25861ee53cc--
--batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0--
");
        // string b = await content.ReadAsStringAsync();
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/mixed; boundary=batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0");
        request.Content = content;
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var stream = await response.Content.ReadAsStreamAsync();
        IODataResponseMessage odataResponseMessage = new ODataMessageWrapper(stream, response.Content.Headers);
        int subResponseCount = 0;
        using (var messageReader = new ODataMessageReader(odataResponseMessage, new ODataMessageReaderSettings(), edmModel))
        {
            var batchReader = messageReader.CreateODataBatchReader();
            var subResponseStatusCodes = new int[] { 201, 201, 204 /*CreateRef*/ };

            while (batchReader.Read())
            {
                switch (batchReader.State)
                {
                    case ODataBatchReaderState.Operation:
                        var operationMessage = batchReader.CreateOperationResponseMessage();
                        var subResponseStatusCode = subResponseCount < subResponseStatusCodes.Length ? subResponseStatusCodes[subResponseCount] : 201;

                        subResponseCount++;
                        Assert.Equal(subResponseStatusCode, operationMessage.StatusCode);
                        break;
                }
            }
        }

        Assert.Equal(3, subResponseCount);
    }

    [Fact]
    public async Task CanHandleReferencingOfRequestsNotSharingAutomicityGroup()
    {
        // Arrange
        var client = CreateClient();

        var requestUri = "DefaultBatch/$batch";
        var defaultBatchCustomersRelativeUri = "DefaultBatchCustomers";
        var defaultBatchOrdersRelativeUri = "DefaultBatchOrders";

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        HttpContent content = new StringContent(@"
    {
        ""requests"": [{
                ""id"": ""1"",
                ""method"": ""POST"",
                ""url"": """ + defaultBatchCustomersRelativeUri + @""",
                ""headers"": {
                    ""OData-Version"": ""4.0"",
                    ""Content-Type"": ""application/json;odata.metadata=minimal"",
                    ""Accept"": ""application/json;odata.metadata=minimal""
                },
                ""body"": {
                    ""Id"":7,
                    ""Name"":""Customer 7""
                }
            }, {
                ""id"": ""2"",
                ""dependsOn"": [""1""],
                ""method"": ""POST"",
                ""url"": """ + defaultBatchOrdersRelativeUri + @""",
                ""headers"": {
                    ""OData-Version"": ""4.0"",
                    ""Content-Type"": ""application/json;odata.metadata=minimal"",
                    ""Accept"": ""application/json;odata.metadata=minimal""
                },
                ""body"": {
                    ""Id"":7
                }
            }, {
                ""id"": ""3"",
                ""dependsOn"": [""1"", ""2""],
                ""method"": ""POST"",
                ""url"": ""$1/Orders/$ref"",
                ""headers"": {
                    ""OData-Version"": ""4.0"",
                    ""Content-Type"": ""application/json;odata.metadata=minimal"",
                    ""Accept"": ""application/json;odata.metadata=minimal""
                },
                ""body"": {
                    ""@odata.id"":""$2""
                }
            }
        ]
    }");
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        request.Content = content;
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var stream = await response.Content.ReadAsStreamAsync();
        IODataResponseMessage odataResponseMessage = new ODataMessageWrapper(stream, response.Content.Headers);
        int subResponseCount = 0;
        using (var messageReader = new ODataMessageReader(odataResponseMessage, new ODataMessageReaderSettings(), edmModel))
        {
            var batchReader = messageReader.CreateODataBatchReader();
            var subResponseStatusCodes = new int[] { 201, 201, 204 /*CreateRef*/ };
                
            while (batchReader.Read())
            {
                switch (batchReader.State)
                {
                    case ODataBatchReaderState.Operation:
                        var operationMessage = batchReader.CreateOperationResponseMessage();
                        var subResponseStatusCode = subResponseCount < subResponseStatusCodes.Length ? subResponseStatusCodes[subResponseCount] : 201;
                            
                        subResponseCount++;
                        Assert.Equal(subResponseStatusCode, operationMessage.StatusCode);
                        break;
                }
            }
        }

        Assert.Equal(3, subResponseCount);
    }
}
