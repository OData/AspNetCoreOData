// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// The interface for all OData convention routing.
    /// </summary>
    public interface IODataControllerActionConvention
    {
        /// <summary>
        /// Gets the order value for determining the order of execution of conventions.
        /// Conventions execute in ascending numeric value of the <see cref="Order"/> property.
        /// </summary>
        /// <para>
        /// If two conventions have the same numeric value of <see cref="Order"/>, then their relative execution order
        /// is undefined.
        /// </para>
        public int Order { get; }

        /// <summary>
        /// Applies the convention on controller
        /// </summary>
        /// <param name="context">The controller action context.</param>
        /// <returns>
        /// True: yes, applies the convention on the actions of this controller.
        /// False: no, please skip this convention on the actions of this controller.
        /// </returns>
        bool AppliesToController(ODataControllerActionContext context);

        /// <summary>
        /// Applies the convention on action of this controller.
        /// </summary>
        /// <param name="context">The controller action context.</param>
        /// <returns>
        /// True: yes, applied the convention, please stop to execute the remaining conventions.
        /// False: no, please continue to execute the remaining conventions.
        /// </returns>
        /// <remarks>
        /// The OData action convention should not put limitation on the action parameters.
        /// That's, if an action has extra parameter that's no required for a certain convention,
        /// We consider this action is valid for this convention.
        /// For example, Entity convention requires the key(s) parameters, doesn't care about other parameters.
        /// </remarks>
        bool AppliesToAction(ODataControllerActionContext context);

        /*
        /// <summary>
        /// Maybe to seperate the query and apply into two parts?
        /// </summary>
        void Apply(...)
        */
    }
}
