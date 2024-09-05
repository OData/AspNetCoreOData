//-----------------------------------------------------------------------------
// <copyright file="ODataBatchFeature.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.Abstracts;

/// <summary>
/// Provide the interface for the details of a given OData batch request.
/// </summary>
public class ODataBatchFeature : IODataBatchFeature
{
    /// <summary>
    /// Gets or sets the batch id.
    /// </summary>
    public Guid? BatchId { get; set; }

    /// <summary>
    /// Gets or sets the change set id.
    /// </summary>
    public Guid? ChangeSetId { get; set; }

    /// <summary>
    /// Gets or sets the content id.
    /// </summary>
    public string ContentId { get; set; }

    /// <summary>
    /// Gets or sets the content id mapping.
    /// </summary>
    public IDictionary<string, string> ContentIdMapping { get; } = new Dictionary<string, string>();
}
