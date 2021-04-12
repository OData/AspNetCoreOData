// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.OData.Routing.Attributes
{
    /// <summary>
    /// Represents an attribute that can be placed on Controller or action to specify that's OData controller or OData action.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ODataRoutingAttribute : Attribute
    {
        // The design:
        // ODataController has this attribute, all controllers derived from ODataController will be considered as OData attribute routing.

        // If any other controller decorated with this attribute, the route template of all actions will be considered as OData attribute routing.
        // If any action decorated with this attribute, the route template of this action will be considered as OData attribute routing.

        // If you want to mix asp.net core attribute and odata attribute routing, consider to create two methods in the controller.
        // If you want to opt one action out OData attribute routing, using [NonODataAction] attribute.
    }
}
