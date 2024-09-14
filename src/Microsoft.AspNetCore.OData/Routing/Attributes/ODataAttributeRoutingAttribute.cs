//-----------------------------------------------------------------------------
// <copyright file="ODataAttributeRoutingAttribute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.Routing.Attributes;


/// <summary>
/// When used to decorate a <see cref="Controller"/> or Controller method, automatically opts that item into 
/// OData routing conventions. 
/// </summary>
/// <remarks>
/// <para>
/// NOTE: If your Controller inherits from <see cref="ODataController"/>, this attribute is NOT required.
/// </para>
/// <para>
/// To allow individual methods to opt out of the OData routing conventions, add the [<see cref="ODataIgnoredAttribute">ODataIgnored</see>] attribute 
/// to that method.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class ODataAttributeRoutingAttribute : Attribute
{
    // The design:
    // ODataController has this attribute, all controllers derived from ODataController will be considered as OData attribute routing.

    // If any other controller decorated with this attribute, the route template of all actions will be considered as OData attribute routing.
    // If any action decorated with this attribute, the route template of this action will be considered as OData attribute routing.

    // If you want to mix asp.net core attribute and odata attribute routing, consider to create two methods in the controller.
    // If you want to opt one action out OData attribute routing, using [NonODataAction] attribute.
}

