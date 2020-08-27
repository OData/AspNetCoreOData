// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.OData.Batch
{
    /// <summary>
    /// Represents an Operation request.
    /// </summary>
    public class OperationRequestItem : ODataBatchRequestItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationRequestItem"/> class.
        /// </summary>
        /// <param name="context">The Operation http context.</param>
        public OperationRequestItem(HttpContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Gets the Operation request context.
        /// </summary>
        public HttpContext Context { get; }

        /// <summary>
        /// Sends the Operation request.
        /// </summary>
        /// <param name="handler">The handler for processing a Http request.</param>
        /// <returns>A <see cref="OperationResponseItem"/>.</returns>
        public override async Task<ODataBatchResponseItem> SendRequestAsync(RequestDelegate handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            await SendRequestAsync(handler, Context, null).ConfigureAwait(false);

            return new OperationResponseItem(Context);
        }
    }
}