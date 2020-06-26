// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// 
    /// </summary>
    public interface IODataControllerActionConvention
    {
        /// <summary>
        /// 
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        bool AppliesToController(ODataControllerActionContext context);

        /*
        /// <summary>
        /// Maybe to seperate the query and apply into two parts?
        /// </summary>
        void Apply(...)
        */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        bool AppliesToAction(ODataControllerActionContext context);
    }
}
