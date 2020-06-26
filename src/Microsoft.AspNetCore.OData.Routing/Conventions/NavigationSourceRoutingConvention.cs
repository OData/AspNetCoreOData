// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// The base class for (entitySet/singleton) routing convention.
    /// </summary>
    public abstract class NavigationSourceRoutingConvention : IODataControllerActionConvention
    {
        /// <summary>
        /// 
        /// </summary>
        public abstract int Order { get; }

        /// <summary>
        /// used for cache
        /// </summary>
        internal IEdmNavigationSource NavigationSource { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual bool AppliesToController(ODataControllerActionContext context)
        {
            //if (model == null)
            //{
            //    throw new ArgumentNullException(nameof(model));
            //}

            //if (controller == null)
            //{
            //    throw new ArgumentNullException(nameof(controller));
            //}

            //string controllerName = controller.ControllerName;
            //NavigationSource = model.EntityContainer?.FindEntitySet(controllerName);

            //// Cached the singleton, because we call this method first, then AppliesToAction
            //// FindSingleton maybe time consuming.
            //return NavigationSource != null;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public abstract bool AppliesToAction(ODataControllerActionContext context);
    }
}
