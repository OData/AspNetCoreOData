// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// 
    /// </summary>
    public class FunctionRoutingConvention : IODataControllerActionConvention
    {
        /// <summary>
        /// 
        /// </summary>
        public int Order => 700;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual bool AppliesToController(ODataControllerActionContext context)
        {
            // bound operation supports for entity set and singleton
            return context?.EntitySet != null || context?.Singleton != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public bool AppliesToAction(ODataControllerActionContext context)
        {

            return false;
        }
    }
}
