// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Batch
{
    /// <summary>
    /// Represents an OData batch response.
    /// </summary>
    public abstract class ODataBatchResponseItem
    {
        /// <summary>
        /// Writes a single OData batch response.
        /// </summary>
        /// <param name="writer">The <see cref="ODataBatchWriter"/>.</param>
        /// <param name="context">The message context.</param>
        public static async Task WriteMessageAsync(ODataBatchWriter writer, HttpContext context)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string contentId = (context.Request != null) ? context.Request.GetODataContentId() : String.Empty;

            ODataBatchOperationResponseMessage batchResponse = await writer.CreateOperationResponseMessageAsync(contentId).ConfigureAwait(false);

            batchResponse.StatusCode = context.Response.StatusCode;

            foreach (KeyValuePair<string, StringValues> header in context.Response.Headers)
            {
                batchResponse.SetHeader(header.Key, String.Join(",", header.Value.ToArray()));
            }

            if (context.Response.Body != null && context.Response.Body.Length != 0)
            {
                using (Stream stream = await batchResponse.GetStreamAsync().ConfigureAwait(false))
                {
                    context.RequestAborted.ThrowIfCancellationRequested();
                    context.Response.Body.Seek(0L, SeekOrigin.Begin);
                    await context.Response.Body.CopyToAsync(stream).ConfigureAwait(false);

                    // Close and release the stream for the individual response
                    ODataBatchStream batchStream = context.Response.Body as ODataBatchStream;
                    if (batchStream != null)
                    {
                        await batchStream.InternalDisposeAsync().ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// Writes the response.
        /// </summary>
        /// <param name="writer">The <see cref="ODataBatchWriter"/>.</param>
        public abstract Task WriteResponseAsync(ODataBatchWriter writer);

        /// <summary>
        /// Gets a value that indicates if the responses in this item are successful.
        /// </summary>
        internal virtual bool IsResponseSuccessful()
        {
            return false;
        }
    }
}
