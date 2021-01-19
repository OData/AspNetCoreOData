// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// The convention for <see cref="IEdmAction"/>.
    /// Post ~/entity|singleton/action,  ~/entity|singleton/cast/action
    /// Post ~/entity|singleton/key/action,  ~/entity|singleton/key/cast/action
    /// </summary>
    public class ActionRoutingConvention : OperationRoutingConvention
    {
        /// <inheritdoc />
        public override int Order => 700;

        /// <inheritdoc />
        public override bool AppliesToAction(ODataControllerActionContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            IEdmNavigationSource navigationSource = context.EntitySet == null ?
                (IEdmNavigationSource)context.Singleton :
                (IEdmNavigationSource)context.EntitySet;

            IEdmEntityType entityType = navigationSource.EntityType();

            // action should have the [HttpPost]
            if (!context.Action.Attributes.Any(a => a is HttpPostAttribute))
            {
                return false;
            }

            // action overload on binding type, only one action overload on the same binding type.
            // however, it supports the bound action on derived type.
            ProcessOperations(context, entityType, navigationSource);

            // in OData operationImport routing convention, all action are processed by default
            // even it's not a really edm operation import call.
            return false;
        }

        /// <inheritdoc />
        protected override bool IsOperationParameterMeet(IEdmOperation operation, ActionModel action)
        {
            Contract.Assert(operation != null);
            Contract.Assert(operation.IsAction());
            Contract.Assert(action != null);

            // So far, we use the "ODataActionParameters" and "ODataUntypedActionParameters" to hold the action parameter values.
            // TODO: consider to use [FromODataBody] to seperate the parameters to each corresponding 
            if (operation.Parameters.Count() > 1)
            {
                if (!action.Parameters.Any(p => p.ParameterType == typeof(ODataActionParameters) || p.ParameterType == typeof(ODataUntypedActionParameters)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
