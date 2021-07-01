// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Routing.Conventions;

namespace Microsoft.AspNetCore.OData.E2E.Tests.OpenType
{

    // this convention is used to stop the conventions for a certain route prefix.
    // Here, we stop to apply routing conventions for "attributeRouting" route prefix.
    public class StopODataRoutingConvention : IODataControllerActionConvention
    {

        public int Order => int.MinValue;

        public bool AppliesToAction(ODataControllerActionContext context)
        {
            if (context.Prefix == "attributeRouting")
            {
                return true;
            }

            return false;
        }

        public bool AppliesToController(ODataControllerActionContext context)
        {
            return true;
        }

    }

}