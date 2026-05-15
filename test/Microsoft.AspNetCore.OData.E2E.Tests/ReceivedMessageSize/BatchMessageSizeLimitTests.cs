//-----------------------------------------------------------------------------
// <copyright file="BatchMessageSizeLimitTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ReceivedMessageSize;

/// <summary>
/// Verifies that the batch handler enforces a configured MaxReceivedMessageSize (4 KB).
/// Both the batch handler quota and ODataOptions are aligned to test end-to-end enforcement.
/// </summary>
public class BatchMessageSizeLimitConfiguredTests : WebApiTestBase<BatchMessageSizeLimitConfiguredTests>
{
    private const int ConfiguredMaxMessageSize = 4096; // 4 KB

    public BatchMessageSizeLimitConfiguredTests(WebApiTestFixture<BatchMessageSizeLimitConfiguredTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        services.ConfigureControllers(typeof(MessageSizeItemsController));

        IEdmModel edmModel = GetEdmModel();

        var batchHandler = new DefaultODataBatchHandler();
        batchHandler.MessageQuotas.MaxReceivedMessageSize = ConfiguredMaxMessageSize;

        services.AddControllers().AddOData(opt =>
        {
            // Both the batch handler quota AND the reader settings must be aligned
            // for stream-level enforcement to work end-to-end.
            opt.MaxReceivedMessageSize = ConfiguredMaxMessageSize;
            opt.AddRouteComponents("odata", edmModel, batchHandler);
        });
    }

    protected static void UpdateConfigure(IApplicationBuilder app)
    {
        // UseODataBatching must come before UseRouting for batch to work.
        app.UseODataBatching();
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }

    private static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<MessageSizeItem>("MessageSizeItems");
        return builder.GetEdmModel();
    }

    [Fact]
    public async Task Batch_PayloadUnderConfiguredLimit_Succeeds()
    {
        // Arrange — a tiny payload keeps the total batch envelope well under 4 KB.
        var id = 1;
        var payload = "A";

        var batchBoundary = "batch_36522ad7-fc75-4b56-8c71-56071383e77b";
        var changesetBoundary = "changeset_36522ad7-fc75-4b56-8c71-56071383e77b";

        var batchBody =
$@"--{batchBoundary}
Content-Type: multipart/mixed; boundary={changesetBoundary}

--{changesetBoundary}
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 1

POST MessageSizeItems HTTP/1.1
Content-Type: application/json;odata.metadata=minimal
Accept: application/json;odata.metadata=minimal
Accept-Charset: UTF-8
OData-Version: 4.0

{{""{nameof(MessageSizeItem.Id)}"":{id},""{nameof(MessageSizeItem.Payload)}"":""{payload}""}}
--{changesetBoundary}--
--{batchBoundary}--
";
        var request = new HttpRequestMessage(HttpMethod.Post, $"odata/$batch");

        var content = new StringContent(batchBody);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse($"multipart/mixed; boundary={batchBoundary}");
        request.Content = content;

        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode,
            $"Expected success for a small batch payload but got {response.StatusCode}: {responseBody}");
        Assert.Contains("HTTP/1.1 201 Created", responseBody, StringComparison.Ordinal);
        Assert.Contains($@"""{nameof(MessageSizeItem.Id)}"":{id}", responseBody, StringComparison.Ordinal);
        Assert.Contains($@"""{nameof(MessageSizeItem.Payload)}"":""{payload}""", responseBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Batch_PayloadExceedingConfiguredLimit_IsRejected()
    {
        // Arrange — 8 KB payload exceeds the 4 KB batch handler limit.
        var id = 2;
        var largePayload = new string('X', 8192);

        var batchBoundary = "batch_36522ad7-fc75-4b56-8c71-56071383e77b";
        var changesetBoundary = "changeset_36522ad7-fc75-4b56-8c71-56071383e77b";

        var batchBody =
$@"--{batchBoundary}
Content-Type: multipart/mixed; boundary={changesetBoundary}

--{changesetBoundary}
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 1

POST MessageSizeItems HTTP/1.1
Content-Type: application/json;odata.metadata=minimal
Accept: application/json;odata.metadata=minimal
Accept-Charset: UTF-8
OData-Version: 4.0

{{""{nameof(MessageSizeItem.Id)}"":{id},""{nameof(MessageSizeItem.Payload)}"":""{largePayload}""}}
--{changesetBoundary}--
--{batchBoundary}--
";
        var request = new HttpRequestMessage(HttpMethod.Post, $"odata/$batch");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("multipart/mixed"));

        var content = new StringContent(batchBody);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse($"multipart/mixed; boundary={batchBoundary}");
        request.Content = content;

        HttpClient client = CreateClient();

        // Act & Assert — when the batch stream exceeds MaxReceivedMessageSize,
        // the OData reader throws ODataException which propagates through the pipeline.
        // Depending on the test host, this surfaces as either an exception from SendAsync
        // or a non-success response with no successful sub-request in the body.
        var exception = await Record.ExceptionAsync(async () => await client.SendAsync(request));
        Assert.NotNull(exception);
        Assert.Contains("maximum number of bytes allowed to be read from the stream has been exceeded", exception.ToString());
    }
}