//-----------------------------------------------------------------------------
// <copyright file="FunctionRoutingConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
    /// Get ~/entityset|singleton/function,  ~/entityset|singleton/cast/function
    /// Get ~/entityset/key/function, ~/entityset/key/cast/function
    /// </summary>
    public class FunctionRoutingConvention : OperationRoutingConvention
    {
        /// <inheritdoc />
        public override int Order => 600;

        /// <inheritdoc />
        public override bool AppliesToAction(ODataControllerActionContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            IEdmNavigationSource navigationSource = context.NavigationSource;
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
        protected override bool IsOperationParameterMatched(IEdmOperation operation, ActionModel action)
        {
            Contract.Assert(operation != null);
            Contract.Assert(action != null);

            if (!operation.IsFunction())
            {
                return false;
            }

            // we can allow the action has other parameters except the function parameters.
            foreach (var parameter in operation.Parameters.Skip(1))
            {
                // It seems we don't need to distinguish the optional parameter here
                // It means whether it's optional parameter or not, the action descriptor should have such parameter defined.
                // Meanwhile, the send request may or may not have such parameter value.
                IEdmOptionalParameter optionalParameter = parameter as IEdmOptionalParameter;
                if (optionalParameter != null)
                {
                    continue;
                }

                if (!action.Parameters.Any(p => p.ParameterInfo.Name == parameter.Name))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
