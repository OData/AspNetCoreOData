// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// A context object for <see cref="IODataControllerActionConvention"/>.
    /// </summary>
    public class ODataControllerActionContext
    {
        /// <summary>
        /// 
        /// </summary>
        public ODataControllerActionContext()
        {

        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="ODataControllerActionContext" /> class.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="model">The Edm model.</param>
        /// <param name="controller">The controller model.</param>
        public ODataControllerActionContext(string prefix, IEdmModel model, ControllerModel controller)
        {
            Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
            Model = model ?? throw new ArgumentNullException(nameof(model));
            Controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="ODataControllerActionContext" /> class.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="model">The Edm model.</param>
        /// <param name="controller">The controller model.</param>
        /// <param name="entitySet">The associated entity set for this controller.</param>
        public ODataControllerActionContext(string prefix, IEdmModel model, ControllerModel controller, IEdmEntitySet entitySet)
            : this(prefix, model, controller)
        {
            EntitySet = entitySet ?? throw new ArgumentNullException(nameof(entitySet));
            EntityType = entitySet.EntityType();
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="ODataControllerActionContext" /> class.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="model">The Edm model.</param>
        /// <param name="controller">The controller model.</param>
        /// <param name="singleton">The associated singleton for this controller.</param>
        public ODataControllerActionContext(string prefix, IEdmModel model, ControllerModel controller, IEdmSingleton singleton)
            : this(prefix, model, controller)
        {
            Singleton = singleton ?? throw new ArgumentNullException(nameof(singleton));
            EntityType = singleton.EntityType();
        }

        /// <summary>
        /// Gets the associated model name for this model, it's also used as the routing prefix.
        /// </summary>
        public string Prefix { get; internal set; }

        /// <summary>
        /// Gets the Edm model.
        /// </summary>
        public IEdmModel Model { get; internal set; }

        /// <summary>
        /// Gets the associated <see cref="IEdmEntitySet"/> for this controller.
        /// It might be null.
        /// </summary>
        public IEdmEntitySet EntitySet { get; internal set; }

        /// <summary>
        /// Gets the associated <see cref="IEdmEntityType"/>.
        /// It might be null.
        /// </summary>
        public IEdmEntityType EntityType { get; internal set; }

        /// <summary>
        /// Gets the associated <see cref=" IEdmSingleton"/> for this controller.
        /// It might be null.
        /// </summary>
        public IEdmSingleton Singleton { get; internal set; }

        /// <summary>
        /// Gets the related controller model in this context. This property should never be "null".
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
        internal IEnumerable<string> RoutePrefixes { get; set; }
    }
}
