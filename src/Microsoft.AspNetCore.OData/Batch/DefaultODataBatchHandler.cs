//-----------------------------------------------------------------------------
// <copyright file="DefaultODataBatchHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Batch;

/// <summary>
/// Default implementation of <see cref="ODataBatchHandler"/> for handling OData batch request.
/// By default, it buffers the request content stream.
/// </summary>
public class DefaultODataBatchHandler : ODataBatchHandler
{
    /// <inheritdoc/>
    public override async Task ProcessBatchAsync(HttpContext context, RequestDelegate nextHandler)
    {
        if (context == null)
        {
            throw Error.ArgumentNull(nameof(context));
        }

        if (nextHandler == null)
        {
            throw Error.ArgumentNull(nameof(nextHandler));
        }

        if (!await ValidateRequest(context.Request).ConfigureAwait(false))
        {
            return;
        }

        SetMinimalApi(context);

        IList<ODataBatchRequestItem> subRequests = await ParseBatchRequestsAsync(context).ConfigureAwait(false);

        bool enableContinueOnErrorHeader = false;
        if (MiniMetadata != null)
        {
            enableContinueOnErrorHeader = MiniMetadata.Options.EnableContinueOnErrorHeader;
        }
        else
        {
            ODataOptions options = context.RequestServices.GetRequiredService<IOptions<ODataOptions>>().Value;
            enableContinueOnErrorHeader = (options != null) ? options.EnableContinueOnErrorHeader : false;
        }

        SetContinueOnError(context.Request.Headers, enableContinueOnErrorHeader);

        IList<ODataBatchResponseItem> responses = await ExecuteRequestMessagesAsync(subRequests, nextHandler).ConfigureAwait(false);

        await CreateResponseMessageAsync(responses, context.Request).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the OData batch requests.
    /// </summary>
    /// <param name="requests">The collection of OData batch requests.</param>
    /// <param name="handler">The handler for processing a message.</param>
    /// <returns>A collection of <see cref="ODataBatchResponseItem"/> for the batch requests.</returns>
    public virtual async Task<IList<ODataBatchResponseItem>> ExecuteRequestMessagesAsync(IEnumerable<ODataBatchRequestItem> requests, RequestDelegate handler)
    {
        if (requests == null)
        {
            throw Error.ArgumentNull(nameof(requests));
        }

        if (handler == null)
        {
            throw Error.ArgumentNull(nameof(handler));
        }

        IList<ODataBatchResponseItem> responses = new List<ODataBatchResponseItem>();

        foreach (ODataBatchRequestItem request in requests)
        {
            ODataBatchResponseItem responseItem = await request.SendRequestAsync(handler).ConfigureAwait(false);
            responses.Add(responseItem);

            if (responseItem != null && responseItem.IsResponseSuccessful() == false && ContinueOnError == false)
            {
                break;
            }
        }

        return responses;
    }

    /// <summary>
    /// Converts the incoming OData batch request into a collection of request messages.
    /// </summary>
    /// <param name="context">The context containing the batch request messages.</param>
    /// <returns>A collection of <see cref="ODataBatchRequestItem"/>.</returns>
    public virtual async Task<IList<ODataBatchRequestItem>> ParseBatchRequestsAsync(HttpContext context)
    {
        if (context == null)
        {
            throw Error.ArgumentNull(nameof(context));
        }

        HttpRequest request = context.Request;

        IServiceProvider requestContainer = null;
        if (MiniMetadata != null)
        {
            requestContainer = MiniMetadata.ServiceProvider;

            // We should dispose the scope after the request is processed?
            // In the case of batch request, we can leave the GC to clean up the scope?
            IServiceScope scope = requestContainer.GetRequiredService<IServiceScopeFactory>().CreateScope();
            request.ODataFeature().Services = scope.ServiceProvider;
            request.ODataFeature().RequestScope = scope;
        }
        else
        {
            requestContainer = request.CreateRouteServices(PrefixName);
        }

        requestContainer.GetRequiredService<ODataMessageReaderSettings>().BaseUri = GetBaseUri(request);

        using (ODataMessageReader reader = request.GetODataMessageReader(requestContainer))
        {
            CancellationToken cancellationToken = context.RequestAborted;
            List<ODataBatchRequestItem> requests = new List<ODataBatchRequestItem>();
            ODataBatchReader batchReader = await reader.CreateODataBatchReaderAsync().ConfigureAwait(false);
            Guid batchId = Guid.NewGuid();
            Dictionary<string, string> contentToLocationMapping = new Dictionary<string, string>();

            while (await batchReader.ReadAsync().ConfigureAwait(false))
            {
                if (batchReader.State == ODataBatchReaderState.ChangesetStart)
                {
                    IList<HttpContext> changeSetContexts = await batchReader.ReadChangeSetRequestAsync(context, batchId, cancellationToken).ConfigureAwait(false);
                    foreach (HttpContext changeSetContext in changeSetContexts)
                    {
                        changeSetContext.Request.ClearRouteServices();
                    }

                    ChangeSetRequestItem requestItem = new ChangeSetRequestItem(changeSetContexts);
                    requestItem.ContentIdToLocationMapping = contentToLocationMapping;
                    requests.Add(requestItem);
                }
                else if (batchReader.State == ODataBatchReaderState.Operation)
                {
                    HttpContext operationContext = await batchReader.ReadOperationRequestAsync(context, batchId, cancellationToken).ConfigureAwait(false);
                    operationContext.Request.ClearRouteServices();
                    OperationRequestItem requestItem = new OperationRequestItem(operationContext);
                    requestItem.ContentIdToLocationMapping = contentToLocationMapping;
                    requests.Add(requestItem);
                }
            }

            return requests;
        }
    }
}
