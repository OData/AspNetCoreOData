// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Batch
{
    /// <summary>
    /// Represents an Operation response.
    /// </summary>
    public class OperationResponseItem : ODataBatchResponseItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationResponseItem"/> class.
        /// </summary>
        /// <param name="context">The response context for the Operation request.</param>
        public OperationResponseItem(HttpContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Gets the response messages for the Operation.
        /// </summary>
        public HttpContext Context { get; private set; }

        /// <summary>
        /// Writes the response as an Operation.
        /// </summary>
        /// <param name="writer">The <see cref="ODataBatchWriter"/>.</param>
        /// <param name="asyncWriter">Whether or not the writer is in async mode. </param>
        public override Task WriteResponseAsync(ODataBatchWriter writer, bool asyncWriter)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            return WriteMessageAsync(writer, Context, asyncWriter);
        }

        /// <summary>
        /// Gets a value that indicates if the responses in this item are successful.
        /// </summary>
        internal override bool IsResponseSuccessful()
        {
            return Context.Response.IsSuccessStatusCode();
        }
    }
}