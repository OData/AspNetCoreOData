//-----------------------------------------------------------------------------
// <copyright file="ControllerModelHelpers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.OData.Tests.Extensions
{
    internal static class ControllerModelHelpers
    {
        /// <summary>
        /// Build the controller model.
        /// </summary>
        /// <typeparam name="T">The controller type.</typeparam>
        /// <returns>The controller model.</returns>
        public static ControllerModel BuildControllerModelWithAllActions<T>()
        {
            Type controllerType = typeof(T);
            return BuildControllerModelByMethodInfo(controllerType, controllerType.GetMethods());
        }

        /// <summary>
        /// Build the controller model.
        /// </summary>
        /// <param name="controllerType">The controller type.</param>
        /// <returns>The controller model.</returns>
        public static ControllerModel BuildControllerModelWithAllActions(Type controllerType)
        {
            return BuildControllerModelByMethodInfo(controllerType, controllerType.GetMethods());
        }

        /// <summary>
        /// Build the controller model.
        /// </summary>
        /// <typeparam name="T">The controller type.</typeparam>
        /// <param name="actions">The actions.</param>
        /// <returns>The controller model.</returns>
        public static ControllerModel BuildControllerModel<T>(params string[] actions)
        {
            return BuildControllerModel(typeof(T), actions);
        }

        /// <summary>
        /// Build the controller model.
        /// </summary>
        /// <param name="controllerType">The controller type.</param>
        /// <param name="actions">The actions.</param>
        /// <returns>The controller model.</returns>
        public static ControllerModel BuildControllerModel(Type controllerType, params string[] actions)
        {
            return BuildControllerModelByMethodInfo(controllerType, actions.Select(a => controllerType.GetMethod(a)).ToArray());
        }

        /// <summary>
        /// Build the controller model.
        /// </summary>
        /// <typeparam name="T">The controller type.</typeparam>
        /// <param name="methodInfos">The method infos.</param>
        /// <returns>The controller model.</returns>
        public static ControllerModel BuildControllerModelByMethodInfo<T>(params MethodInfo[] methodInfos)
        {
            return BuildControllerModelByMethodInfo(typeof(T), methodInfos);
        }

        /// <summary>
        /// Build the controller model.
        /// </summary>
        /// <param name="controllerType">The controller type.</param>
        /// <param name="methodInfos">The method infos.</param>
        /// <returns>The controller model.</returns>
        public static ControllerModel BuildControllerModelByMethodInfo(Type controllerType, params MethodInfo[] methodInfos)
        {
            if (controllerType == null)
            {
                throw Error.ArgumentNull(nameof(controllerType));
            }

            TypeInfo controllerTypeInfo = controllerType.GetTypeInfo();
            ControllerModel controller = CreateControllerModel(controllerTypeInfo);

            foreach (var methodInfo in methodInfos)
            {
                if (methodInfo == null)
                {
                    continue;
                }

                ActionModel actionModel = CreateActionModel(controllerTypeInfo, methodInfo);
                if (actionModel == null)
                {
                    continue;
                }
                actionModel.Controller = controller;

                foreach (var parameterInfo in actionModel.ActionMethod.GetParameters())
                {
                    var parameterModel = CreateParameterModel(parameterInfo);
                    if (parameterModel != null)
                    {
                        parameterModel.Action = actionModel;
                        actionModel.Parameters.Add(parameterModel);
                    }
                }

                controller.Actions.Add(actionModel);
            }

            return controller;
        }

        /// <summary>
        /// Creates a <see cref="ControllerModel"/> for the given <see cref="TypeInfo"/>.
        /// </summary>
        /// <param name="typeInfo">The <see cref="TypeInfo"/>.</param>
        /// <returns>A <see cref="ControllerModel"/> for the given <see cref="TypeInfo"/>.</returns>
        internal static ControllerModel CreateControllerModel(TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw Error.ArgumentNull(nameof(typeInfo));
            }

            // For attribute routes on a controller, we want to support 'overriding' routes on a derived
            // class. So we need to walk up the hierarchy looking for the first class to define routes.
            //
            // Then we want to 'filter' the set of attributes, so that only the effective routes apply.
            var currentTypeInfo = typeInfo;
            var objectTypeInfo = typeof(object).GetTypeInfo();

            IRouteTemplateProvider[] routeAttributes;

            do
            {
                routeAttributes = currentTypeInfo
                    .GetCustomAttributes(inherit: false)
                    .OfType<IRouteTemplateProvider>()
                    .ToArray();

                if (routeAttributes.Length > 0)
                {
                    // Found 1 or more route attributes.
                    break;
                }

                currentTypeInfo = currentTypeInfo.BaseType.GetTypeInfo();
            }
            while (currentTypeInfo != objectTypeInfo);

            // CoreCLR returns IEnumerable<Attribute> from GetCustomAttributes - the OfType<object>
            // is needed to so that the result of ToArray() is object
            var attributes = typeInfo.GetCustomAttributes(inherit: true);

            // This is fairly complicated so that we maintain referential equality between items in
            // ControllerModel.Attributes and ControllerModel.Attributes[*].Attribute.
            var filteredAttributes = new List<object>();
            foreach (var attribute in attributes)
            {
                if (attribute is IRouteTemplateProvider)
                {
                    // This attribute is a route-attribute, leave it out.
                }
                else
                {
                    filteredAttributes.Add(attribute);
                }
            }

            filteredAttributes.AddRange(routeAttributes);

            attributes = filteredAttributes.ToArray();

            var controllerModel = new ControllerModel(typeInfo, attributes);

            AddRange(controllerModel.Selectors, CreateSelectors(attributes));

            controllerModel.ControllerName =
                typeInfo.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase) ?
                    typeInfo.Name.Substring(0, typeInfo.Name.Length - "Controller".Length) :
                    typeInfo.Name;

            AddRange(controllerModel.Filters, attributes.OfType<IFilterMetadata>());

            foreach (var routeValueProvider in attributes.OfType<IRouteValueProvider>())
            {
                controllerModel.RouteValues.Add(routeValueProvider.RouteKey, routeValueProvider.RouteValue);
            }

            var apiVisibility = attributes.OfType<IApiDescriptionVisibilityProvider>().FirstOrDefault();
            if (apiVisibility != null)
            {
                controllerModel.ApiExplorer.IsVisible = !apiVisibility.IgnoreApi;
            }

            var apiGroupName = attributes.OfType<IApiDescriptionGroupNameProvider>().FirstOrDefault();
            if (apiGroupName != null)
            {
                controllerModel.ApiExplorer.GroupName = apiGroupName.GroupName;
            }

            // Controllers can implement action filter and result filter interfaces. We add
            // a special delegating filter implementation to the pipeline to handle it.
            //
            // This is needed because filters are instantiated before the controller.
            //if (typeof(IAsyncActionFilter).GetTypeInfo().IsAssignableFrom(typeInfo) ||
            //    typeof(IActionFilter).GetTypeInfo().IsAssignableFrom(typeInfo))
            //{
            //    controllerModel.Filters.Add(new ControllerActionFilter());
            //}
            //if (typeof(IAsyncResultFilter).GetTypeInfo().IsAssignableFrom(typeInfo) ||
            //    typeof(IResultFilter).GetTypeInfo().IsAssignableFrom(typeInfo))
            //{
            //    controllerModel.Filters.Add(new ControllerResultFilter());
            //}

            return controllerModel;
        }

        /// <summary>
        /// Creates the <see cref="ActionModel"/> instance for the given action <see cref="MethodInfo"/>.
        /// </summary>
        /// <param name="typeInfo">The controller <see cref="TypeInfo"/>.</param>
        /// <param name="methodInfo">The action <see cref="MethodInfo"/>.</param>
        /// <returns>
        /// An <see cref="ActionModel"/> instance for the given action <see cref="MethodInfo"/> or
        /// <c>null</c> if the <paramref name="methodInfo"/> does not represent an action.
        /// </returns>
        internal static ActionModel CreateActionModel(TypeInfo typeInfo, MethodInfo methodInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }

            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            if (!IsAction(typeInfo, methodInfo))
            {
                return null;
            }

            // CoreCLR returns IEnumerable<Attribute> from GetCustomAttributes - the OfType<object>
            // is needed to so that the result of ToArray() is object
            var attributes = methodInfo.GetCustomAttributes(inherit: true);

            var actionModel = new ActionModel(methodInfo, attributes);

            AddRange(actionModel.Filters, attributes.OfType<IFilterMetadata>());

            var actionName = attributes.OfType<ActionNameAttribute>().FirstOrDefault();
            if (actionName?.Name != null)
            {
                actionModel.ActionName = actionName.Name;
            }
            else
            {
                actionModel.ActionName = CanonicalizeActionName(methodInfo.Name);
            }

            var apiVisibility = attributes.OfType<IApiDescriptionVisibilityProvider>().FirstOrDefault();
            if (apiVisibility != null)
            {
                actionModel.ApiExplorer.IsVisible = !apiVisibility.IgnoreApi;
            }

            var apiGroupName = attributes.OfType<IApiDescriptionGroupNameProvider>().FirstOrDefault();
            if (apiGroupName != null)
            {
                actionModel.ApiExplorer.GroupName = apiGroupName.GroupName;
            }

            foreach (var routeValueProvider in attributes.OfType<IRouteValueProvider>())
            {
                actionModel.RouteValues.Add(routeValueProvider.RouteKey, routeValueProvider.RouteValue);
            }

            // Now we need to determine the action selection info (cross-section of routes and constraints)
            //
            // For attribute routes on a action, we want to support 'overriding' routes on a
            // virtual method, but allow 'overriding'. So we need to walk up the hierarchy looking
            // for the first definition to define routes.
            //
            // Then we want to 'filter' the set of attributes, so that only the effective routes apply.
            var currentMethodInfo = methodInfo;

            IRouteTemplateProvider[] routeAttributes;

            while (true)
            {
                routeAttributes = currentMethodInfo
                    .GetCustomAttributes(inherit: false)
                    .OfType<IRouteTemplateProvider>()
                    .ToArray();

                if (routeAttributes.Length > 0)
                {
                    // Found 1 or more route attributes.
                    break;
                }

                // GetBaseDefinition returns 'this' when it gets to the bottom of the chain.
                var nextMethodInfo = currentMethodInfo.GetBaseDefinition();
                if (currentMethodInfo == nextMethodInfo)
                {
                    break;
                }

                currentMethodInfo = nextMethodInfo;
            }

            // This is fairly complicated so that we maintain referential equality between items in
            // ActionModel.Attributes and ActionModel.Attributes[*].Attribute.
            var applicableAttributes = new List<object>(routeAttributes.Length);
            foreach (var attribute in attributes)
            {
                if (attribute is IRouteTemplateProvider)
                {
                    // This attribute is a route-attribute, leave it out.
                }
                else
                {
                    applicableAttributes.Add(attribute);
                }
            }

            applicableAttributes.AddRange(routeAttributes);
            AddRange(actionModel.Selectors, CreateSelectors(applicableAttributes));

            return actionModel;
        }

        private static IList<SelectorModel> CreateSelectors(IList<object> attributes)
        {
            // Route attributes create multiple selector models, we want to split the set of
            // attributes based on these so each selector only has the attributes that affect it.
            //
            // The set of route attributes are split into those that 'define' a route versus those that are
            // 'silent'.
            //
            // We need to define a selector for each attribute that 'defines' a route, and a single selector
            // for all of the ones that don't (if any exist).
            //
            // If the attribute that 'defines' a route is NOT an IActionHttpMethodProvider, then we'll include with
            // it, any IActionHttpMethodProvider that are 'silent' IRouteTemplateProviders. In this case the 'extra'
            // action for silent route providers isn't needed.
            //
            // Ex:
            // [HttpGet]
            // [AcceptVerbs("POST", "PUT")]
            // [HttpPost("Api/Things")]
            // public void DoThing()
            //
            // This will generate 2 selectors:
            // 1. [HttpPost("Api/Things")]
            // 2. [HttpGet], [AcceptVerbs("POST", "PUT")]
            //
            // Another example of this situation is:
            //
            // [Route("api/Products")]
            // [AcceptVerbs("GET", "HEAD")]
            // [HttpPost("api/Products/new")]
            //
            // This will generate 2 selectors:
            // 1. [AcceptVerbs("GET", "HEAD")]
            // 2. [HttpPost]
            //
            // Note that having a route attribute that doesn't define a route template _might_ be an error. We
            // don't have enough context to really know at this point so we just pass it on.
            var routeProviders = new List<IRouteTemplateProvider>();

            var createSelectorForSilentRouteProviders = false;
            foreach (var attribute in attributes)
            {
                if (attribute is IRouteTemplateProvider routeTemplateProvider)
                {
                    if (IsSilentRouteAttribute(routeTemplateProvider))
                    {
                        createSelectorForSilentRouteProviders = true;
                    }
                    else
                    {
                        routeProviders.Add(routeTemplateProvider);
                    }
                }
            }

            foreach (var routeProvider in routeProviders)
            {
                // If we see an attribute like
                // [Route(...)]
                //
                // Then we want to group any attributes like [HttpGet] with it.
                //
                // Basically...
                //
                // [HttpGet]
                // [HttpPost("Products")]
                // public void Foo() { }
                //
                // Is two selectors. And...
                //
                // [HttpGet]
                // [Route("Products")]
                // public void Foo() { }
                //
                // Is one selector.
                if (!(routeProvider is IActionHttpMethodProvider))
                {
                    createSelectorForSilentRouteProviders = false;
                    break;
                }
            }

            var selectorModels = new List<SelectorModel>();
            if (routeProviders.Count == 0 && !createSelectorForSilentRouteProviders)
            {
                // Simple case, all attributes apply
                selectorModels.Add(CreateSelectorModel(route: null, attributes: attributes));
            }
            else
            {
                // Each of these routeProviders are the ones that actually have routing information on them
                // something like [HttpGet] won't show up here, but [HttpGet("Products")] will.
                foreach (var routeProvider in routeProviders)
                {
                    var filteredAttributes = new List<object>();
                    foreach (var attribute in attributes)
                    {
                        if (ReferenceEquals(attribute, routeProvider))
                        {
                            filteredAttributes.Add(attribute);
                        }
                        else if (InRouteProviders(routeProviders, attribute))
                        {
                            // Exclude other route template providers
                            // Example:
                            // [HttpGet("template")]
                            // [Route("template/{id}")]
                        }
                        else if (
                            routeProvider is IActionHttpMethodProvider &&
                            attribute is IActionHttpMethodProvider)
                        {
                            // Example:
                            // [HttpGet("template")]
                            // [AcceptVerbs("GET", "POST")]
                            //
                            // Exclude other http method providers if this route is an
                            // http method provider.
                        }
                        else
                        {
                            filteredAttributes.Add(attribute);
                        }
                    }

                    selectorModels.Add(CreateSelectorModel(routeProvider, filteredAttributes));
                }

                if (createSelectorForSilentRouteProviders)
                {
                    var filteredAttributes = new List<object>();
                    foreach (var attribute in attributes)
                    {
                        if (!InRouteProviders(routeProviders, attribute))
                        {
                            filteredAttributes.Add(attribute);
                        }
                    }

                    selectorModels.Add(CreateSelectorModel(route: null, attributes: filteredAttributes));
                }
            }

            return selectorModels;
        }

        private static bool InRouteProviders(List<IRouteTemplateProvider> routeProviders, object attribute)
        {
            foreach (var rp in routeProviders)
            {
                if (ReferenceEquals(rp, attribute))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns <c>true</c> if the <paramref name="methodInfo"/> is an action. Otherwise <c>false</c>.
        /// </summary>
        /// <param name="typeInfo">The <see cref="TypeInfo"/>.</param>
        /// <param name="methodInfo">The <see cref="MethodInfo"/>.</param>
        /// <returns><c>true</c> if the <paramref name="methodInfo"/> is an action. Otherwise <c>false</c>.</returns>
        /// <remarks>
        /// Override this method to provide custom logic to determine which methods are considered actions.
        /// </remarks>
        internal static bool IsAction(TypeInfo typeInfo, MethodInfo methodInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }

            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            // The SpecialName bit is set to flag members that are treated in a special way by some compilers
            // (such as property accessors and operator overloading methods).
            if (methodInfo.IsSpecialName)
            {
                return false;
            }

            if (methodInfo.IsDefined(typeof(NonActionAttribute)))
            {
                return false;
            }

            // Overridden methods from Object class, e.g. Equals(Object), GetHashCode(), etc., are not valid.
            if (methodInfo.GetBaseDefinition().DeclaringType == typeof(object))
            {
                return false;
            }

            // Dispose method implemented from IDisposable is not valid
            if (IsIDisposableMethod(methodInfo))
            {
                return false;
            }

            if (methodInfo.IsStatic)
            {
                return false;
            }

            if (methodInfo.IsAbstract)
            {
                return false;
            }

            if (methodInfo.IsConstructor)
            {
                return false;
            }

            if (methodInfo.IsGenericMethod)
            {
                return false;
            }

            return methodInfo.IsPublic;
        }

        private static SelectorModel CreateSelectorModel(IRouteTemplateProvider route, IList<object> attributes)
        {
            var selectorModel = new SelectorModel();
            if (route != null)
            {
                selectorModel.AttributeRouteModel = new AttributeRouteModel(route);
            }

            AddRange(selectorModel.ActionConstraints, attributes.OfType<IActionConstraintMetadata>());
            AddRange(selectorModel.EndpointMetadata, attributes);

            // Simple case, all HTTP method attributes apply
            var httpMethods = attributes
                .OfType<IActionHttpMethodProvider>()
                .SelectMany(a => a.HttpMethods)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (httpMethods.Length > 0)
            {
                selectorModel.ActionConstraints.Add(new HttpMethodActionConstraint(httpMethods));
                selectorModel.EndpointMetadata.Add(new HttpMethodMetadata(httpMethods));
            }

            return selectorModel;
        }

        private static bool IsIDisposableMethod(MethodInfo methodInfo)
        {
            // Ideally we do not want Dispose method to be exposed as an action. However there are some scenarios where a user
            // might want to expose a method with name "Dispose" (even though they might not be really disposing resources)
            // Example: A controller deriving from MVC's Controller type might wish to have a method with name Dispose,
            // in which case they can use the "new" keyword to hide the base controller's declaration.

            // Find where the method was originally declared
            var baseMethodInfo = methodInfo.GetBaseDefinition();
            var declaringTypeInfo = baseMethodInfo.DeclaringType.GetTypeInfo();

            return
                (typeof(IDisposable).GetTypeInfo().IsAssignableFrom(declaringTypeInfo) &&
                 declaringTypeInfo.GetRuntimeInterfaceMap(typeof(IDisposable)).TargetMethods[0] == baseMethodInfo);
        }

        /// <summary>
        /// Creates a <see cref="ParameterModel"/> for the given <see cref="ParameterInfo"/>.
        /// </summary>
        /// <param name="parameterInfo">The <see cref="ParameterInfo"/>.</param>
        /// <returns>A <see cref="ParameterModel"/> for the given <see cref="ParameterInfo"/>.</returns>
        internal static ParameterModel CreateParameterModel(ParameterInfo parameterInfo)
        {
            if (parameterInfo == null)
            {
                throw new ArgumentNullException(nameof(parameterInfo));
            }

            var attributes = parameterInfo.GetCustomAttributes(inherit: true);

            //BindingInfo bindingInfo;
            //if (_modelMetadataProvider is ModelMetadataProvider modelMetadataProviderBase)
            //{
            //    var modelMetadata = modelMetadataProviderBase.GetMetadataForParameter(parameterInfo);
            //    bindingInfo = BindingInfo.GetBindingInfo(attributes, modelMetadata);
            //}
            //else
            //{
            //    // GetMetadataForParameter should only be used if the user has opted in to the 2.1 behavior.
            //    bindingInfo = BindingInfo.GetBindingInfo(attributes);
            //}

            var parameterModel = new ParameterModel(parameterInfo, attributes)
            {
                ParameterName = parameterInfo.Name,
              //  BindingInfo = bindingInfo,
            };

            return parameterModel;
        }

        private static string CanonicalizeActionName(string actionName)
        {
            const string Suffix = "Async";

            if (actionName.EndsWith(Suffix, StringComparison.Ordinal))
            {
                actionName = actionName.Substring(0, actionName.Length - Suffix.Length);
            }

            return actionName;
        }

        private static bool IsSilentRouteAttribute(IRouteTemplateProvider routeTemplateProvider)
        {
            return
                routeTemplateProvider.Template == null &&
                routeTemplateProvider.Order == null &&
                routeTemplateProvider.Name == null;
        }

        private static void AddRange<T>(IList<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                list.Add(item);
            }
        }
    }
}
