// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Commons;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// 
    /// </summary>
    public class OperationRoutingConvention : IODataControllerActionConvention
    {
        /// <summary>
        /// 
        /// </summary>
        public virtual int Order => 600;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual bool AppliesToController(ODataControllerActionContext context)
        {
            // bound operation supports for entity set and singleton
            return context?.EntitySet != null || context?.Singleton != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public virtual bool AppliesToAction(ODataControllerActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ActionModel action = context.Action;

            if (context.EntitySet == null && context.Singleton == null)
            {
                return false;
            }
            IEdmNavigationSource navigationSource = context.EntitySet == null ?
                (IEdmNavigationSource)context.Singleton :
                (IEdmNavigationSource)context.EntitySet;

            IEdmModel model = context.Model;
            string prefix = context.Prefix;
            IEdmEntityType entityType = navigationSource.EntityType();
            bool hasKeyParameter = HasKeyParameter(entityType, action);

            // found
            int keyNumber = entityType.Key().Count();
            IEdmType bindType = entityType;
            if (!hasKeyParameter)
            {
                // bond to collection
                bindType = new EdmCollectionType(new EdmEntityTypeReference(entityType, true));
                keyNumber = 0;
            }

            string actionName = action.ActionMethod.Name;
            var operations = model.FindBoundOperations(bindType).Where(p => p.Name == actionName);

            var actions = operations.OfType<IEdmAction>().ToList();
            if (actions.Count == 1) // action overload on binding type, only one action overload on the same binding type
            {
                if (action.Parameters.Any(p => p.ParameterType == typeof(ODataActionParameters)))
                {
                    // we find a action route
                    IList<ODataSegmentTemplate> segments = new List<ODataSegmentTemplate>();

                    if (context.EntitySet != null)
                    {
                        segments.Add(new EntitySetSegmentTemplate(context.EntitySet));
                    }
                    else
                    {
                        segments.Add(new SingletonSegmentTemplate(context.Singleton));
                    }


                    if (hasKeyParameter)
                    {
                        segments.Add(new KeySegmentTemplate(entityType));
                    }
                    segments.Add(new ActionSegmentTemplate(actions[0], false));

                    ODataPathTemplate template = new ODataPathTemplate(segments);

                    action.AddSelector(prefix, model, template);
                    return true;
                }
            }

            var functions = operations.OfType<IEdmFunction>().ToList();
            IEdmFunction function = FindMatchFunction(keyNumber, functions, action);
            if (function != null)
            {
                IList<ODataSegmentTemplate> segments = new List<ODataSegmentTemplate>();

                if (context.EntitySet != null)
                {
                    segments.Add(new EntitySetSegmentTemplate(context.EntitySet));
                }
                else
                {
                    segments.Add(new SingletonSegmentTemplate(context.Singleton));
                }

                if (hasKeyParameter)
                {
                    segments.Add(new KeySegmentTemplate(entityType));
                }
                segments.Add(new FunctionSegmentTemplate(function, false));

                ODataPathTemplate template = new ODataPathTemplate(segments);

                action.AddSelector(prefix, model, template);
                return true;
            }

            // in OData operationImport routing convention, all action are processed by default
            // even it's not a really edm operation import call.
            return false;
        }

        private static bool HasKeyParameter(IEdmEntityType entityType, ActionModel action)
        {
            var keys = entityType.Key().ToArray();
            if (keys.Length == 1)
            {
                return action.Parameters.Any(p => p.ParameterInfo.Name == "key");
            }
            else
            {
                foreach (var key in keys)
                {
                    string keyName = $"key{key.Name}";
                    if (!action.Parameters.Any(p => p.ParameterInfo.Name == keyName))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private static IEdmFunction FindMatchFunction(int keyNumber, IEnumerable<IEdmFunction> functions, ActionModel action)
        {
            // if it's action
            int actionParameterNumber = action.Parameters.Count - keyNumber + 1; // +1 means to include the binding type
            foreach (var function in functions)
            {
                if (function.Parameters.Count() != actionParameterNumber)
                {
                    // maybe we can allow other parameters
                    continue;
                }

                bool matched = true;
                foreach (var parameter in function.Parameters.Skip(1)) // skip 1 because bound
                {
                    if (!action.Parameters.Any(p => p.ParameterInfo.Name == parameter.Name))
                    {
                        matched = false;
                        break;
                    }
                }

                if (matched)
                {
                    return function;
                }
            }

            return null;
        }
    }
}
