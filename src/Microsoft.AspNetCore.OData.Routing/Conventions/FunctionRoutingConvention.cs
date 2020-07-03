// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// 
    /// </summary>
    public class FunctionRoutingConvention : IODataControllerActionConvention
    {
        /// <inheritdoc />
        public int Order => 700;

        /// <inheritdoc />
        public virtual bool AppliesToController(ODataControllerActionContext context)
        {
            // bound operation supports for entity set and singleton
            return context?.EntitySet != null || context?.Singleton != null;
        }

        /// <inheritdoc />
        public bool AppliesToAction(ODataControllerActionContext context)
        {

            return false;
        }
    }
}
