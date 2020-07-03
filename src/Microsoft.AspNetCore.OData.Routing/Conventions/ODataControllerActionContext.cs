// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// 
    /// </summary>
    public class ODataControllerActionContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="model"></param>
        internal ODataControllerActionContext(string prefix, IEdmModel model)
        {
            Prefix = prefix;
            Model = model;
        }

        internal ODataControllerActionContext(string prefix, IEdmModel model, IEdmEntitySet entitySet)
            : this(prefix, model)
        {
            EntitySet = entitySet;
        }

        internal ODataControllerActionContext(string prefix, IEdmModel model, IEdmSingleton singleton)
            : this(prefix, model)
        {
            Singleton = singleton;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// 
        /// </summary>
        public IEdmModel Model { get; }

        /// <summary>
        /// 
        /// </summary>
        public IEdmEntitySet EntitySet { get; }

        /// <summary>
        /// 
        /// </summary>
        public IEdmSingleton Singleton { get; }

        /// <summary>
        /// Gets the related controller model in this context
        /// We guarentee that this property should never be "null".
        /// </summary>
        public ControllerModel Controller { get; internal set; }

        /// <summary>
        /// Gets the related action model in this context
        /// We guarentee that this property should never be "null" when calling "AppliesToAction"
        /// </summary>
        public ActionModel Action { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsDone { get; }
    }
}
