//-----------------------------------------------------------------------------
// <copyright file="ChangeSetResponseItem.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Batch;

/// <summary>
/// Represents a ChangeSet response.
/// </summary>
public class ChangeSetResponseItem : ODataBatchResponseItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeSetResponseItem"/> class.
    /// </summary>
    /// <param name="contexts">The response contexts for the ChangeSet requests.</param>
    public ChangeSetResponseItem(IEnumerable<HttpContext> contexts)
    {
        Contexts = contexts ?? throw new ArgumentNullException(nameof(contexts));
    }

    /// <summary>
    /// Gets the response contexts for the ChangeSet.
    /// </summary>
    public IEnumerable<HttpContext> Contexts { get; }

    /// <summary>
    /// Writes the responses as a ChangeSet.
    /// </summary>
    /// <param name="writer">The <see cref="ODataBatchWriter"/>.</param>
    public override async Task WriteResponseAsync(ODataBatchWriter writer)
    {
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        await writer.WriteStartChangesetAsync().ConfigureAwait(false);

        foreach (HttpContext context in Contexts)
        {
            await WriteMessageAsync(writer, context).ConfigureAwait(false);
        }

        await writer.WriteEndChangesetAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a value that indicates if the responses in this item are successful.
    /// </summary>
    internal override bool IsResponseSuccessful()
    {
        return Contexts.All(c => c.Response.IsSuccessStatusCode());
    }
}
