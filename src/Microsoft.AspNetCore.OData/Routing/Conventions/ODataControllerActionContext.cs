//-----------------------------------------------------------------------------
// <copyright file="ODataControllerActionContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
        /// Gets the associated model name for this model, it's also used as the routing prefix.
        /// </summary>
        public string Prefix { get; internal set; }

        /// <summary>
        /// Gets the Edm model.
        /// </summary>
        public IEdmModel Model { get; internal set; }

        /// <summary>
        /// Gets the related controller model in this context. This property should never be "null".
        /// </summary>
        public ControllerModel Controller { get; internal set; }

        /// <summary>
        /// Gets/sets the navigation source associated with controller.
        /// </summary>
        public IEdmNavigationSource NavigationSource { get; set; }

        /// <summary>
        /// Gets/sets the related action model in this context.
        /// </summary>
        public ActionModel Action { get; set; }

        /// <summary>
        /// Gets the associated <see cref="IEdmEntitySet"/> for this controller.
        /// It might be null.
        /// </summary>
        public IEdmEntitySet EntitySet => NavigationSource as IEdmEntitySet;

        /// <summary>
        /// Gets the associated <see cref="IEdmEntityType"/>.
        /// It might be null.
        /// </summary>
        public IEdmEntityType EntityType => NavigationSource?.EntityType;

        /// <summary>
        /// Gets the associated <see cref=" IEdmSingleton"/> for this controller.
        /// It might be null.
        /// </summary>
        public IEdmSingleton Singleton => NavigationSource as IEdmSingleton;

        /// <summary>
        /// Gets/sets the OData Options.
        /// </summary>
        public ODataOptions Options { get; set; } = new ODataOptions();
    }
}
