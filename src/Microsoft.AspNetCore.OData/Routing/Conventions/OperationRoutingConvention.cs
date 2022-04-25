//-----------------------------------------------------------------------------
// <copyright file="OperationRoutingConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// Conventions for <see cref="IEdmAction"/> and <see cref="IEdmFunction"/>.
    /// Get ~/entityset|singleton/function,  ~/entityset|singleton/cast/function
    /// Get ~/entityset|singleton/key/function, ~/entityset|singleton/key/cast/function
    /// Post ~/entityset|singleton/action,  ~/entityset|singleton/cast/action
    /// Post ~/entityset|singleton/key/action,  ~/entityset|singleton/key/cast/action
    /// </summary>
    public abstract class OperationRoutingConvention : IODataControllerActionConvention
    {
        /// <inheritdoc />
        public abstract int Order { get; }

        /// <inheritdoc />
        public virtual bool AppliesToController(ODataControllerActionContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            // bound operation supports for entity set and singleton
            return context.NavigationSource != null;
        }

        /// <inheritdoc />
        public abstract bool AppliesToAction(ODataControllerActionContext context);

        /// <summary>
        /// Process the operation candidates using the information.
        /// </summary>
        /// <param name="context">The controller and action context.</param>
        /// <param name="entityType">The Edm entity type.</param>
        /// <param name="navigationSource">The Edm navigation source.</param>
        protected void ProcessOperations(ODataControllerActionContext context, IEdmEntityType entityType,  IEdmNavigationSource navigationSource)
        {
            Contract.Assert(context != null);
            Contract.Assert(entityType != null);
            Contract.Assert(navigationSource != null);

            string actionName = context.Action.ActionName;

            bool hasKeyParameter = context.Action.HasODataKeyParameter(entityType, context.Options?.RouteOptions?.EnablePropertyNameCaseInsensitive ?? false);
            if (context.Singleton != null && hasKeyParameter)
            {
                // Singleton doesn't allow to call action with key.
                return;
            }

            bool isOnCollection = false;
            IEdmEntityType castTypeFromActionName = null;

            IEdmOperation[] candidates = FindCandidates(context, actionName);
            if (candidates.Length == 0)
            {
                // If we can't find any Edm operation using the action name directly,
                // Let's split the action name and use part of it to search again.
                candidates = FindCandidates(context, entityType, actionName, out castTypeFromActionName, out isOnCollection);
            }

            if (candidates == null || candidates.Length == 0)
            {
                return;
            }

            foreach (IEdmOperation edmOperation in candidates)
            {
                IEdmOperationParameter bindingParameter = edmOperation.Parameters.FirstOrDefault();
                if (bindingParameter == null)
                {
                    // bound operation at least has one parameter which type is the binding type.
                    continue;
                }

                IEdmTypeReference bindingType = bindingParameter.Type;
                bool bindToCollection = bindingType.TypeKind() == EdmTypeKind.Collection;
                if (bindToCollection)
                {
                    // if binding to collection the action has key parameter or a singleton, skip
                    if (context.Singleton != null || hasKeyParameter)
                    {
                        continue;
                    }
                }
                else
                {
                    // if binding to non-collection and the action hasn't key parameter, skip
                    if (isOnCollection || (context.EntitySet != null && !hasKeyParameter))
                    {
                        continue;
                    }
                }

                // We only allow the binding type is entity type or collection of entity type.
                if (!bindingType.Definition.IsEntityOrEntityCollectionType(out IEdmEntityType bindingEntityType))
                {
                    continue;
                }

                IEdmEntityType castType = null;
                if (castTypeFromActionName == null)
                {
                    if (entityType.IsOrInheritsFrom(bindingEntityType))
                    {
                        // True if and only if the thisType is equivalent to or inherits from otherType.
                        castType = null;
                    }
                    else if (bindingEntityType.InheritsFrom(entityType))
                    {
                        // True if and only if the type inherits from the potential base type.
                        castType = bindingEntityType;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    if (isOnCollection && !bindToCollection)
                    {
                        continue;
                    }

                    if (bindingEntityType != castTypeFromActionName)
                    {
                        continue;
                    }

                    if (castTypeFromActionName != entityType)
                    {
                        castType = castTypeFromActionName;
                    }
                }

                // TODO: need discussion about:
                // 1) Do we need to match the whole parameter count?
                // 2) Do we need to select the best match? So far, i don't think and let it go.
                if (!IsOperationParameterMatched(edmOperation, context.Action))
                {
                    continue;
                }

                AddSelector(context, edmOperation, hasKeyParameter, entityType, navigationSource, castType);
            }
        }

        private static IEdmOperation[] FindCandidates(ODataControllerActionContext context, string operationName)
        {
            // TODO: refactor here
            // If we have multiple same function defined, we should match the best one?

            StringComparison actionNameComparison = context.Options?.RouteOptions?.EnableActionNameCaseInsensitive == true ?
                StringComparison.OrdinalIgnoreCase :
                StringComparison.Ordinal;

            return context.Model.SchemaElements
                .OfType<IEdmOperation>()
                .Where(f => f.IsBound && f.Name.Equals(operationName, actionNameComparison))
                .ToArray();
        }

        private static IEdmOperation[] FindCandidates(ODataControllerActionContext context, IEdmEntityType entityType, string actionName,
            out IEdmEntityType castTypeFromActionName, out bool isOnCollection)
        {
            // OperationNameOnCollectionOfEntityType
            string operationName = SplitActionName(actionName, out string cast, out isOnCollection);

            castTypeFromActionName = null;
            if (cast != null)
            {
                if (cast.Length == 0)
                {
                    // Early return for the following cases:
                    // - {OperationName}On
                    // - {OperationName}OnCollectionOf
                    return null;
                }

                castTypeFromActionName = entityType.FindTypeInInheritance(context.Model, cast, context.Options?.RouteOptions?.EnableActionNameCaseInsensitive == true) as IEdmEntityType;
                if (castTypeFromActionName == null)
                {
                    return null;
                }
            }

            return FindCandidates(context, operationName);
        }

        /// <summary>
        /// Split the action based on supporting pattern.
        /// </summary>
        /// <param name="actionName">The input action name.</param>
        /// <param name="cast">The out of cast type name.</param>
        /// <param name="isOnCollection">The out of collection binding flag.</param>
        /// <returns>The operation name.</returns>
        internal static string SplitActionName(string actionName, out string cast, out bool isOnCollection)
        {
            Contract.Assert(actionName != null);

            // We support the following function/action name pattern:
            // OperationNameOnCollectionOfEntityType
            // OperationNameOnEntityType
            // OperationName
            cast = null;
            isOnCollection = false;
            string operation;
            int index = actionName.LastIndexOf("OnCollectionOf", StringComparison.Ordinal);
            if (index > 0)
            {
                operation = actionName.Substring(0, index);
                cast = actionName.Substring(index + "OnCollectionOf".Length);
                isOnCollection = true;
                return operation;
            }

            index = actionName.LastIndexOf("On", StringComparison.Ordinal);
            if (index > 0)
            {
                operation = actionName.Substring(0, index);
                cast = actionName.Substring(index + "On".Length);
                return operation;
            }

            return actionName;
        }

        /// <summary>
        /// Verify the parameter of the Edm operation matches the parameter defined in action.
        /// </summary>
        /// <param name="operation">The Edm operation.</param>
        /// <param name="action">The action model.</param>
        /// <returns>true if the parameter of the Edm operation matches the parameter defined in the action; otherwise, false.</returns>
        protected abstract bool IsOperationParameterMatched(IEdmOperation operation, ActionModel action);

        /// <summary>
        /// Add the template to the action
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="edmOperation">The Edm operation.</param>
        /// <param name="hasKeyParameter">Has key parameter or not.</param>
        /// <param name="entityType">The entity type.</param>
        /// <param name="navigationSource">The navigation source.</param>
        /// <param name="castType">The type cast.</param>
        protected static void AddSelector(ODataControllerActionContext context,
            IEdmOperation edmOperation,
            bool hasKeyParameter,
            IEdmEntityType entityType,
            IEdmNavigationSource navigationSource,
            IEdmEntityType castType)
        {
            Contract.Assert(context != null);
            Contract.Assert(entityType != null);
            Contract.Assert(navigationSource != null);
            Contract.Assert(edmOperation != null);

            // Now, let's add the selector model.
            IList<ODataSegmentTemplate> segments = new List<ODataSegmentTemplate>();
            if (context.EntitySet != null)
            {
                segments.Add(new EntitySetSegmentTemplate(context.EntitySet));
                if (hasKeyParameter)
                {
                    segments.Add(KeySegmentTemplate.CreateKeySegment(entityType, navigationSource));
                }
            }
            else
            {
                segments.Add(new SingletonSegmentTemplate(context.Singleton));
            }

            if (castType != null)
            {
                if (context.Singleton != null || !hasKeyParameter)
                {
                    segments.Add(new CastSegmentTemplate(castType, entityType, navigationSource));
                }
                else
                {
                    segments.Add(new CastSegmentTemplate(new EdmCollectionType(castType.ToEdmTypeReference(false)),
                        new EdmCollectionType(entityType.ToEdmTypeReference(false)), navigationSource));
                }
            }

            IEdmNavigationSource targetEntitySet = null;
            if (edmOperation.ReturnType != null)
            {
                targetEntitySet = edmOperation.GetTargetEntitySet(navigationSource, context.Model);
            }

            string httpMethod;
            if (edmOperation.IsAction())
            {
                segments.Add(new ActionSegmentTemplate((IEdmAction)edmOperation, targetEntitySet));
                httpMethod = "Post";
            }
            else
            {
                IDictionary<string, string> required = GetRequiredFunctionParamters(edmOperation, context.Action);
                segments.Add(new FunctionSegmentTemplate(required, (IEdmFunction)edmOperation, targetEntitySet));
                httpMethod = "Get";
            }

            ODataPathTemplate template = new ODataPathTemplate(segments);
            context.Action.AddSelector(httpMethod, context.Prefix, context.Model, template, context.Options?.RouteOptions);
        }

        private static IDictionary<string, string> GetRequiredFunctionParamters(IEdmOperation operation, ActionModel action)
        {
            Contract.Assert(operation != null);
            Contract.Assert(operation.IsFunction());
            Contract.Assert(action != null);

            IDictionary<string, string> requiredParameters = new Dictionary<string, string>();
            // we can allow the action has other parameters except the function parameters.
            foreach (var parameter in operation.Parameters.Skip(1))
            {
                if (action.Parameters.Any(p => p.ParameterInfo.Name == parameter.Name))
                {
                    requiredParameters[parameter.Name] = $"{{{parameter.Name}}}";
                }
            }

            return requiredParameters;
        }
    }
}
