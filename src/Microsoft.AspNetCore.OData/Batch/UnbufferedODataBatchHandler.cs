//-----------------------------------------------------------------------------
// <copyright file="UnbufferedODataBatchHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Batch;

/// <summary>
/// An implementation of <see cref="ODataBatchHandler"/> that doesn't buffer the request content stream.
/// </summary>
public class UnbufferedODataBatchHandler : ODataBatchHandler
{
    /// <inheritdoc/>
    public override async Task ProcessBatchAsync(HttpContext context, RequestDelegate nextHandler)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (nextHandler == null)
        {
            throw new ArgumentNullException(nameof(nextHandler));
        }

        if (!await ValidateRequest(context.Request).ConfigureAwait(false))
        {
            return;
        }

        // This container is for the overall batch request.
        HttpRequest request = context.Request;
        IServiceProvider requestContainer = request.CreateRouteServices(PrefixName);
        requestContainer.GetRequiredService<ODataMessageReaderSettings>().BaseUri = GetBaseUri(request);
        List<ODataBatchResponseItem> responses = new List<ODataBatchResponseItem>();
            
        using (ODataMessageReader reader = request.GetODataMessageReader(requestContainer))
        {
            ODataBatchReader batchReader = await reader.CreateODataBatchReaderAsync().ConfigureAwait(false);
            Guid batchId = Guid.NewGuid();

            ODataOptions options = context.RequestServices.GetRequiredService<IOptions<ODataOptions>>().Value;
            bool enableContinueOnErrorHeader = (options != null)
                ? options.EnableContinueOnErrorHeader
                : false;

            SetContinueOnError(request.Headers, enableContinueOnErrorHeader);

            while (await batchReader.ReadAsync().ConfigureAwait(false))
            {
                ODataBatchResponseItem responseItem = null;
                if (batchReader.State == ODataBatchReaderState.ChangesetStart)
                {
                    responseItem = await ExecuteChangeSetAsync(batchReader, batchId, request, nextHandler).ConfigureAwait(false);
                }
                else if (batchReader.State == ODataBatchReaderState.Operation)
                {
                    responseItem = await ExecuteOperationAsync(batchReader, batchId, request, nextHandler).ConfigureAwait(false);
                }
                if (responseItem != null)
                {
                    responses.Add(responseItem);
                    if (responseItem.IsResponseSuccessful() == false && ContinueOnError == false)
                    {
                        break;
                    }
                }
            }
        }

        await CreateResponseMessageAsync(responses, request).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the operation.
    /// </summary>
    /// <param name="batchReader">The batch reader.</param>
    /// <param name="batchId">The batch id.</param>
    /// <param name="originalRequest">The original request containing all the batch requests.</param>
    /// <param name="handler">The handler for processing a message.</param>
    /// <returns>The response for the operation.</returns>
    public virtual async Task<ODataBatchResponseItem> ExecuteOperationAsync(ODataBatchReader batchReader, Guid batchId, HttpRequest originalRequest, RequestDelegate handler)
    {
        if (batchReader == null)
        {
            throw new ArgumentNullException(nameof(batchReader));
        }

        if (originalRequest == null)
        {
            throw new ArgumentNullException(nameof(originalRequest));
        }

        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        CancellationToken cancellationToken = originalRequest.HttpContext.RequestAborted;
        cancellationToken.ThrowIfCancellationRequested();
        HttpContext operationContext = await batchReader.ReadOperationRequestAsync(originalRequest.HttpContext, batchId, cancellationToken).ConfigureAwait(false);

        operationContext.Request.ClearRouteServices();
        OperationRequestItem operation = new OperationRequestItem(operationContext);
        operation.ContentIdToLocationMapping = originalRequest.HttpContext.ODataBatchFeature().ContentIdMapping;

        ODataBatchResponseItem responseItem = await operation.SendRequestAsync(handler).ConfigureAwait(false);

        return responseItem;
    }

    /// <summary>
    /// Executes the ChangeSet.
    /// </summary>
    /// <param name="batchReader">The batch reader.</param>
    /// <param name="batchId">The batch id.</param>
    /// <param name="originalRequest">The original request containing all the batch requests.</param>
    /// <param name="handler">The handler for processing a message.</param>
    /// <returns>The response for the ChangeSet.</returns>
    public virtual async Task<ODataBatchResponseItem> ExecuteChangeSetAsync(ODataBatchReader batchReader, Guid batchId, HttpRequest originalRequest, RequestDelegate handler)
    {
        if (batchReader == null)
        {
            throw new ArgumentNullException(nameof(batchReader));
        }

        if (originalRequest == null)
        {
            throw new ArgumentNullException(nameof(originalRequest));
        }

        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        Guid changeSetId = Guid.NewGuid();
        List<HttpContext> changeSetResponse = new List<HttpContext>();

        IDictionary<string, string> contentIdToLocationMapping = originalRequest.HttpContext.ODataBatchFeature().ContentIdMapping;
        while (await batchReader.ReadAsync().ConfigureAwait(false) && batchReader.State != ODataBatchReaderState.ChangesetEnd)
        {
            if (batchReader.State == ODataBatchReaderState.Operation)
            {
                CancellationToken cancellationToken = originalRequest.HttpContext.RequestAborted;
                HttpContext changeSetOperationContext = await batchReader.ReadChangeSetOperationRequestAsync(originalRequest.HttpContext, batchId, changeSetId, cancellationToken).ConfigureAwait(false);

                await ODataBatchRequestItem.SendRequestAsync(handler, changeSetOperationContext, contentIdToLocationMapping).ConfigureAwait(false);
                if (changeSetOperationContext.Response.IsSuccessStatusCode())
                {
                    changeSetResponse.Add(changeSetOperationContext);
                }
                else
                {
                    changeSetResponse.Clear();
                    changeSetResponse.Add(changeSetOperationContext);
                    return new ChangeSetResponseItem(changeSetResponse);
                }
            }
        }

        return new ChangeSetResponseItem(changeSetResponse);
    }
}
