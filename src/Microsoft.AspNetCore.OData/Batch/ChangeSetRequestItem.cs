//-----------------------------------------------------------------------------
// <copyright file="ChangeSetRequestItem.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;

namespace Microsoft.AspNetCore.OData.Batch
{
    /// <summary>
    /// Represents a ChangeSet request.
    /// </summary>
    public class ChangeSetRequestItem : ODataBatchRequestItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeSetRequestItem"/> class.
        /// </summary>
        /// <param name="contexts">The request contexts in the ChangeSet.</param>
        public ChangeSetRequestItem(IEnumerable<HttpContext> contexts)
        {
            Contexts = contexts ?? throw new ArgumentNullException(nameof(contexts));
        }

        /// <summary>
        /// Gets the request contexts in the ChangeSet.
        /// </summary>
        public IEnumerable<HttpContext> Contexts { get; }

        /// <summary>
        /// Sends the ChangeSet request to the request delegate.
        /// </summary>
        /// <param name="handler">The handler for processing a message.</param>
        /// <returns>A <see cref="ChangeSetResponseItem"/>.</returns>
        public override async Task<ODataBatchResponseItem> SendRequestAsync(RequestDelegate handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            Dictionary<string, string> contentIdToLocationMapping = new Dictionary<string, string>();
            List<HttpContext> responseContexts = new List<HttpContext>();

            foreach (HttpContext context in Contexts)
            {
                await SendRequestAsync(handler, context, contentIdToLocationMapping).ConfigureAwait(false);

                HttpResponse response = context.Response;
                if (response.IsSuccessStatusCode())
                {
                    responseContexts.Add(context);
                }
                else
                {
                    responseContexts.Clear();
                    responseContexts.Add(context);
                    return new ChangeSetResponseItem(responseContexts);
                }
            }

            return new ChangeSetResponseItem(responseContexts);
        }
    }
}
