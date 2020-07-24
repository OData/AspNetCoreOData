// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// The convention for <see cref="IEdmFunction"/>.
    /// Get ~/entity|singleton/function,  ~/entity|singleton/cast/function
    /// Get ~/entity|singleton/key/function, ~/entity|singleton/key/cast/function
    /// </summary>
    public class FunctionRoutingConvention : OperationRoutingConvention
    {
        /// <inheritdoc />
        public override int Order => 700;

        /// <inheritdoc />
        public override bool AppliesToAction(ODataControllerActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            IEdmNavigationSource navigationSource = context.EntitySet == null ?
                (IEdmNavigationSource)context.Singleton :
                (IEdmNavigationSource)context.EntitySet;

            IEdmEntityType entityType = navigationSource.EntityType();

            // function should have the [HttpGet]
            if (!context.Action.Attributes.Any(a => a is HttpGetAttribute))
            {
                return false;
            }

            ProcessOperations(context, entityType, navigationSource);
            return false;
        }

        /// <inheritdoc />
        protected override bool IsOperationParameterMeet(IEdmOperation operation, ActionModel action)
        {
            Contract.Assert(operation != null);
            Contract.Assert(operation.IsFunction());
            Contract.Assert(action != null);

            // we can allow the action has other parameters except the functio parameters.
            foreach (var parameter in operation.Parameters.Skip(1))
            {
                // It seems we don't need to distinguish the optional parameter here
                // It means whether it's optional parameter or not, the action descriptor should have such parameter defined.
                // Meanwhile, the send request may or may not have such parameter value.
                //IEdmOptionalParameter optionalParameter = parameter as IEdmOptionalParameter;
                //if (optionalParameter != null)
                //{
                //    continue;
                //}
                if (!action.Parameters.Any(p => p.ParameterInfo.Name == parameter.Name))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
