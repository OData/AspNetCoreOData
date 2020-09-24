// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// A <see cref="ModelBinderAttribute"/> to bind parameters of type <see cref="ODataQueryOptions"/> to the OData query from the incoming request.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public sealed class ODataQueryParameterBindingAttribute : ModelBinderAttribute
    {
        /// <summary>
        /// Instantiates a new instance of the <see cref="ODataQueryParameterBindingAttribute"/> class.
        /// </summary>
        public ODataQueryParameterBindingAttribute()
            : base(typeof(ODataQueryParameterBinding))
        {
        }

        internal static Type GetEntityClrTypeFromParameterType(Type parameterType)
        {
            Contract.Assert(parameterType != null);

            if (parameterType.IsGenericType &&
                parameterType.GetGenericTypeDefinition() == typeof(ODataQueryOptions<>))
            {
                return parameterType.GetGenericArguments().Single();
            }

            return null;
        }

        internal class ODataQueryParameterBinding : IModelBinder
        {
            private static MethodInfo _createODataQueryOptions = typeof(ODataQueryParameterBinding).GetMethod("CreateODataQueryOptions");
            private const string CreateODataQueryOptionsCtorKey = "MS_CreateODataQueryOptionsOfT";

            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We don't want to fail in model binding.")]
            public Task BindModelAsync(ModelBindingContext bindingContext)
            {
                if (bindingContext == null)
                {
                    throw Error.ArgumentNull("bindingContext");
                }

                HttpRequest request = bindingContext.HttpContext.Request;

                if (request == null)
                {
                    throw Error.Argument("actionContext", SRResources.ActionContextMustHaveRequest);
                }

                ActionDescriptor actionDescriptor = bindingContext.ActionContext.ActionDescriptor;

                if (actionDescriptor == null)
                {
                    throw Error.Argument("actionContext", SRResources.ActionContextMustHaveDescriptor);
                }

                // Get the parameter description of the parameter to bind.
                ParameterDescriptor paramDescriptor = bindingContext.ActionContext.ActionDescriptor.Parameters
                    .Where(p => p.Name == bindingContext.FieldName)
                    .FirstOrDefault();

                // Now make sure parameter type is ODataQueryOptions or ODataQueryOptions<T>.
                Type parameterType = paramDescriptor?.ParameterType;
                if (IsODataQueryOptions(parameterType))
                {
                    // Get the entity type from the parameter type if it is ODataQueryOptions<T>.
                    // Fall back to the return type if not. Also, note that the entity type from the return type and ODataQueryOptions<T>
                    // can be different (example implementing $select or $expand).
                    Type entityClrType = null;
                    if (paramDescriptor != null)
                    {
                        entityClrType = GetEntityClrTypeFromParameterType(parameterType);
                    }

                    if (entityClrType == null)
                    {
                        entityClrType = GetEntityClrTypeFromActionReturnType(actionDescriptor);
                    }

                    IEdmModel userModel = request.GetModel();
                    IEdmModel model = userModel != EdmCoreModel.Instance ? userModel : GetEdmModel(actionDescriptor, request, entityClrType);
                    ODataQueryContext entitySetContext = new ODataQueryContext(model, entityClrType, request.ODataFeature().Path);

                    Func<ODataQueryContext, HttpRequest, ODataQueryOptions> createODataQueryOptions;
                    object constructorAsObject = null;
                    if (actionDescriptor.Properties.TryGetValue(CreateODataQueryOptionsCtorKey, out constructorAsObject))
                    {
                        createODataQueryOptions = (Func<ODataQueryContext, HttpRequest, ODataQueryOptions>)constructorAsObject;
                    }
                    else
                    {
                        createODataQueryOptions = (Func<ODataQueryContext, HttpRequest, ODataQueryOptions>)
                            Delegate.CreateDelegate(typeof(Func<ODataQueryContext, HttpRequest, ODataQueryOptions>),
                                _createODataQueryOptions.MakeGenericMethod(entityClrType));
                    };

                    ODataQueryOptions parameterValue = createODataQueryOptions(entitySetContext, request);
                    bindingContext.Result = ModelBindingResult.Success(parameterValue);
                }

                return Task.CompletedTask;
            }

            public static bool IsODataQueryOptions(Type parameterType)
            {
                if (parameterType == null)
                {
                    return false;
                }
                return ((parameterType == typeof(ODataQueryOptions)) ||
                        (parameterType.IsGenericType &&
                         parameterType.GetGenericTypeDefinition() == typeof(ODataQueryOptions<>)));
            }

            public static ODataQueryOptions<T> CreateODataQueryOptions<T>(ODataQueryContext context, HttpRequest request)
            {
                return new ODataQueryOptions<T>(context, request);
            }

            internal static Type GetEntityClrTypeFromActionReturnType(ActionDescriptor actionDescriptor)
            {
                // It is a developer programming error to use this binding attribute
                // on actions that return void.
                ControllerActionDescriptor controllerActionDescriptor = actionDescriptor as ControllerActionDescriptor;
                if (controllerActionDescriptor == null)
                {
                    throw Error.InvalidOperation(
                                    SRResources.FailedToBuildEdmModelBecauseReturnTypeIsNull,
                                    actionDescriptor.DisplayName,
                                    actionDescriptor.ToString());
                }

                if (controllerActionDescriptor.MethodInfo.ReturnType == null)
                {
                    throw Error.InvalidOperation(
                                    SRResources.FailedToBuildEdmModelBecauseReturnTypeIsNull,
                                    controllerActionDescriptor.ActionName,
                                    controllerActionDescriptor.ControllerName);
                }

                Type entityClrType = TypeHelper.GetImplementedIEnumerableType(controllerActionDescriptor.MethodInfo.ReturnType);

                if (entityClrType == null)
                {
                    // It is a developer programming error to use this binding attribute
                    // on actions that return a collection whose element type cannot be
                    // determined, such as a non-generic IQueryable or IEnumerable.
                    throw Error.InvalidOperation(
                                    SRResources.FailedToRetrieveTypeToBuildEdmModel,
                                    controllerActionDescriptor.ActionName,
                                    controllerActionDescriptor.ControllerName,
                                    controllerActionDescriptor.MethodInfo.ReturnType.FullName);
                }

                return entityClrType;
            }

            internal static IEdmModel GetEdmModel(ActionDescriptor actionDescriptor, HttpRequest request, Type entityClrType)
            {
                if (actionDescriptor == null)
                {
                    throw Error.ArgumentNull("actionDescriptor");
                }

                if (request == null)
                {
                    throw Error.ArgumentNull("request");
                }

                if (entityClrType == null)
                {
                    throw Error.ArgumentNull("entityClrType");
                }

                IEdmModel model = null;

                return model;
            }
        }
    }
}
