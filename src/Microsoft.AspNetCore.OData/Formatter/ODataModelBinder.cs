//-----------------------------------------------------------------------------
// <copyright file="ODataModelBinder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Formatter
{
    /// <summary>
    /// A model binder for ODataParameterValue values.
    /// </summary>
    /// <remarks>
    /// This class is similar to ODataModelBinderProvider in AspNet. The flow is similar but the
    /// type are dissimilar enough making a common version more complex than separate versions.
    /// </remarks>
    internal class ODataModelBinder : IModelBinder
    {
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We don't want to fail in model binding.")]
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw Error.ArgumentNull(nameof(bindingContext));
            }

            if (bindingContext.ModelMetadata == null)
            {
                throw Error.Argument(nameof(bindingContext), SRResources.ModelBinderUtil_ModelMetadataCannotBeNull);
            }

            ValueProviderResult valueProviderResult = ValueProviderResult.None;
            string modelName = ODataParameterValue.ParameterValuePrefix + bindingContext.ModelName;
            try
            {
                // Look in route data for a ODataParameterValue.
                object valueAsObject = null;
                if (!bindingContext.HttpContext.Request.ODataFeature().RoutingConventionsStore.TryGetValue(modelName, out valueAsObject))
                {
                    bindingContext.ActionContext.RouteData.Values.TryGetValue(modelName, out valueAsObject);
                }

                if (valueAsObject != null)
                {
                    StringValues stringValues = new StringValues(valueAsObject.ToString());
                    valueProviderResult = new ValueProviderResult(stringValues);
                    bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

                    ODataParameterValue paramValue = valueAsObject as ODataParameterValue;
                    if (paramValue != null)
                    {
                        HttpRequest request = bindingContext.HttpContext.Request;
                        object model = ConvertTo(paramValue, bindingContext, request.GetRouteServices());
                        bindingContext.Result = ModelBindingResult.Success(model);
                        return Task.CompletedTask;
                    }
                }
                else
                {
                    // If not in the route data, ask the value provider.
                    valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
                    if (valueProviderResult == ValueProviderResult.None)
                    {
                        valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
                    }

                    if (valueProviderResult != ValueProviderResult.None)
                    {
                        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

                        HttpRequest request = bindingContext.HttpContext.Request;
                        TimeZoneInfo timeZone = request.GetTimeZoneInfo();
                        object model = ODataModelBinderConverter.ConvertTo(valueProviderResult.FirstValue, bindingContext.ModelType, timeZone);
                        if (model != null)
                        {
                            bindingContext.Result = ModelBindingResult.Success(model);
                            return Task.CompletedTask;
                        }
                    }
                }

                // No matches, binding failed.
                bindingContext.Result = ModelBindingResult.Failed();
            }
            catch (ODataException ex)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, ex.Message);
                bindingContext.Result = ModelBindingResult.Failed();
            }
            catch (ValidationException ex)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, Error.Format(SRResources.ValueIsInvalid, valueProviderResult.FirstValue, ex.Message));
                bindingContext.Result = ModelBindingResult.Failed();
            }
            catch (FormatException ex)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, Error.Format(SRResources.ValueIsInvalid, valueProviderResult.FirstValue, ex.Message));
                bindingContext.Result = ModelBindingResult.Failed();
            }
            catch (Exception e)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, e.Message);
                bindingContext.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }

        internal static object ConvertTo(ODataParameterValue parameterValue, ModelBindingContext bindingContext, IServiceProvider requestContainer)
        {
            Contract.Assert(parameterValue != null && parameterValue.EdmType != null);

            object oDataValue = parameterValue.Value;
            if (oDataValue == null || oDataValue is ODataNullValue)
            {
                return null;
            }

            IEdmTypeReference edmTypeReference = parameterValue.EdmType;
            ODataDeserializerContext readContext = BuildDeserializerContext(bindingContext, edmTypeReference);
            return ODataModelBinderConverter.Convert(oDataValue, edmTypeReference, bindingContext.ModelType,
                bindingContext.ModelName, readContext, requestContainer);
        }

        internal static ODataDeserializerContext BuildDeserializerContext(ModelBindingContext bindingContext, IEdmTypeReference edmTypeReference)
        {
            HttpRequest request = bindingContext.HttpContext.Request;
            ODataPath path = request.ODataFeature().Path;
            IEdmModel edmModel = request.GetModel();

            TimeZoneInfo timeZone = null;
            IOptions<ODataOptions> odataOptions = request.HttpContext.RequestServices.GetService<IOptions<ODataOptions>>();
            if (odataOptions != null && odataOptions.Value != null)
            {
                timeZone = odataOptions.Value.TimeZone;
            }

            return new ODataDeserializerContext
            {
                Path = path,
                Model = edmModel,
                Request = request,
                ResourceType = bindingContext.ModelType,
                ResourceEdmType = edmTypeReference,
                TimeZone = timeZone
            };
        }
    }
}
