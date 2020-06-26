// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// 
    /// </summary>
    public class OperationImportRoutingConvention : IODataControllerActionConvention
    {
        /// <summary>
        /// 
        /// </summary>
        public int Order => 700;

        /// <summary>
        /// used for cache
        /// </summary>
        internal IEdmOperationImport OperationImport { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool AppliesToController(ODataControllerActionContext context)
        {
            return context?.Controller?.ControllerName == "ODataOperationImport";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public bool AppliesToAction(ODataControllerActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ActionModel action = context.Action;
            if (action.Controller.ControllerName != "ODataOperationImport")
            {
                return false;
            }

            IEdmModel model = context.Model;

            // By convention, we use the operation name as the action name in the controller
            string actionMethodName = action.ActionMethod.Name;
            var edmOperationImports = model.EntityContainer.FindOperationImports(actionMethodName);

            foreach (var edmOperationImport in edmOperationImports)
            {
                IEdmEntitySetBase targetSet = null;
                edmOperationImport.TryGetStaticEntitySet(model, out targetSet);

                if (edmOperationImport.IsActionImport())
                {
                    ODataPathTemplate template = new ODataPathTemplate(new ActionImportSegmentTemplate((IEdmActionImport)edmOperationImport));
                    action.AddSelector(context.Prefix, context.Model, template);
                }
                else
                {
                    IEdmFunctionImport functionImport = (IEdmFunctionImport)edmOperationImport;
                    ODataPathTemplate template = new ODataPathTemplate(new FunctionImportSegmentTemplate(functionImport));
                    action.AddSelector(context.Prefix, context.Model, template);
                }
            }

            // in OData operationImport routing convention, all action are processed by default
            // even it's not a really edm operation import call.
            return true;
        }
    }
}
