//-----------------------------------------------------------------------------
// <copyright file="ODataRoutingMetadata.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing;

/// <summary>
/// Represents OData Endpoint metadata used during routing.
/// </summary>
public sealed class ODataRoutingMetadata : IODataRoutingMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ODataRoutingMetadata"/> class.
    /// </summary>
    /// <param name="prefix">The prefix string.</param>
    /// <param name="model">The Edm model.</param>
    /// <param name="template">The Routing path template.</param>
    public ODataRoutingMetadata(string prefix, IEdmModel model, ODataPathTemplate template)
    {
        Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Template = template ?? throw new ArgumentNullException(nameof(template));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataRoutingMetadata"/> class.
    /// For unit test only
    /// </summary>
    internal ODataRoutingMetadata()
    {
    }

    /// <summary>
    /// Gets the prefix string.
    /// </summary>
    public string Prefix { get; }

    /// <summary>
    /// Gets the Edm model.
    /// </summary>
    public IEdmModel Model { get; }

    /// <summary>
    /// Gets the OData path template
    /// </summary>
    public ODataPathTemplate Template { get; }

    /// <summary>
    /// Gets or sets a boolean value indicating from odata conventional routing.
    /// </summary>
    public bool IsConventional { get; set; } = true;
}
