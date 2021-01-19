// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// A context object for <see cref="IODataControllerActionConvention"/>.
    /// </summary>
    /// <remarks>
    /// Why do i design "ControllerActionContext", not "ControllerContext" and "ActionContext".
    /// It's because a controller may have a bound of actions, and i don't want to create an ActionContext for all of these actions.
    /// </remarks>
    public class ODataControllerActionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataControllerActionContext" /> class.
        /// For unit test only
        /// </summary>
        internal ODataControllerActionContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataControllerActionContext" /> class.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="model">The Edm model.</param>
        /// <param name="controller">The controller model.</param>
        public ODataControllerActionContext(string prefix, IEdmModel model, ControllerModel controller)
        {
            Prefix = prefix ?? throw Error.ArgumentNull(nameof(prefix));
            Model = model ?? throw Error.ArgumentNull(nameof(model));
            Controller = controller ?? throw Error.ArgumentNull(nameof(controller));
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
            EntitySet = entitySet ?? throw Error.ArgumentNull(nameof(entitySet));
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
            Singleton = singleton ?? throw Error.ArgumentNull(nameof(singleton));
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
        /// Gets/sets the odata service provider, used for the attribute routing parser or others.
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }
    }
}
