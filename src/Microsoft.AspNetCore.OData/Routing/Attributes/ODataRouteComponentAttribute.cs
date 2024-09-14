//-----------------------------------------------------------------------------
// <copyright file="ODataRouteComponentAttribute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.Routing.Attributes;


/// <summary>
/// When used to decorate a <see cref="Controller"/> or Controller method (including <see cref="ODataController"/>),
/// instructs OData which set of <see cref="ODataOptions.RouteComponents"/> to use.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class ODataRouteComponentAttribute : Attribute
{

    /// <summary>
    /// Instructs OData to use the default <see cref="ODataOptions.RouteComponents"/> RoutePrefix, which equals 
    /// <see cref="string.Empty"/>, for the decorated Controller or Controller method.
    /// </summary>
    public ODataRouteComponentAttribute()
        : this(string.Empty)
    {
    }

    /// <summary>
    /// Instructs OData to use the specified <paramref name="routePrefix"/> for the decorated Controller or Controller method.
    /// </summary>
    /// <param name="routePrefix">The key in <see cref="ODataOptions.RouteComponents"/> to use .</param>
    /// <remarks>
    /// Ensure this model is registered with OData by calling 
    /// services.AddOData(options => options.AddRouteComponent()).
    /// </remarks>
    public ODataRouteComponentAttribute(string routePrefix)
    {
        RoutePrefix = routePrefix ?? throw Error.ArgumentNull(nameof(routePrefix));
    }

    /// <summary>
    /// Gets the specified model name.
    /// </summary>
    public string RoutePrefix { get; }

}

