//-----------------------------------------------------------------------------
// <copyright file="ETagActionFilterAttribute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.Net.Http.Headers;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Abstracts
{
    /// <summary>
    /// Defines a <see cref="ActionFilterAttribute"/> to add an ETag header value to an OData response when the response
    /// is a single resource that has an ETag defined.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ETagActionFilterAttribute : ActionFilterAttribute
    {
        /// <inheritdoc/>
        public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext == null)
            {
                throw Error.ArgumentNull(nameof(actionExecutedContext));
            }

            if (actionExecutedContext.HttpContext == null)
            {
                throw Error.ArgumentNull("httpContext");
            }

            HttpRequest request = actionExecutedContext.HttpContext.Request;
            ODataPath path = request.ODataFeature().Path;
            if (path == null)
            {
                throw Error.ArgumentNull("path");
            }

            IEdmModel model = request.GetModel();
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            IETagHandler etagHandler = request.GetETagHandler();
            if (etagHandler == null)
            {
                throw Error.ArgumentNull("etagHandler");
            }

            // Need a value to operate on.
            ObjectResult result = actionExecutedContext.Result as ObjectResult;
            if (result == null)
            {
                return;
            }

            HttpResponse response = actionExecutedContext.HttpContext.Response;
            EntityTagHeaderValue etag = GetETag(response?.StatusCode, path, model, result.Value, etagHandler);

            if (etag != null)
            {
                response.Headers["ETag"] = etag.ToString();
            }
        }

        private static EntityTagHeaderValue GetETag(int? statusCode, ODataPath path, IEdmModel model, object value, IETagHandler etagHandler)
        {
            Contract.Assert(path != null);
            Contract.Assert(model != null);
            Contract.Assert(etagHandler != null);

            // Do not interfere with null responses, we want to bubble it up to the top.
            // Do not handle 204 responses as the spec says a 204 response must not include an ETag header
            // unless the request's representation data was saved without any transformation applied to the body
            // (i.e., the resource's new representation data is identical to the representation data received in the
            // PUT request) and the ETag value reflects the new representation.
            // Even in that case returning an ETag is optional and it requires access to the original object which is
            // not possible with the current architecture, so if the user is interested he can set the ETag in that
            // case by himself on the response.
            if (statusCode == null ||
                !(statusCode.Value >= StatusCodes.Status200OK && statusCode.Value < StatusCodes.Status300MultipleChoices) ||
                statusCode.Value == StatusCodes.Status204NoContent)
            {
                return null;
            }

            IEdmEntityType edmType = GetSingleEntityEntityType(path);

            IEdmEntityTypeReference typeReference = GetTypeReference(model, edmType, value);
            if (typeReference != null)
            {
                ResourceContext context = CreateInstanceContext(model, typeReference, value);
                context.EdmModel = model;
                context.NavigationSource = path.GetNavigationSource();
                return CreateETag(context, etagHandler);
            }

            return null;
        }

        private static IEdmEntityTypeReference GetTypeReference(IEdmModel model, IEdmEntityType edmType, object value)
        {
            if (model == null || edmType == null || value == null)
            {
                return null;
            }

            IEdmObject edmObject = value as IEdmEntityObject;
            if (edmObject != null)
            {
                IEdmTypeReference edmTypeReference = edmObject.GetEdmType();
                return edmTypeReference.AsEntity();
            }

            IEdmTypeReference reference = model.GetEdmTypeReference(value.GetType());
            if (reference != null && reference.Definition.IsOrInheritsFrom(edmType))
            {
                return (IEdmEntityTypeReference)reference;
            }

            return null;
        }

        private static EntityTagHeaderValue CreateETag(ResourceContext resourceContext, IETagHandler handler)
        {
            IEdmModel model = resourceContext.EdmModel;

            IEnumerable<IEdmStructuralProperty> concurrencyProperties;
            if (model != null && resourceContext.NavigationSource != null)
            {
                concurrencyProperties = model.GetConcurrencyProperties(resourceContext.NavigationSource).OrderBy(c => c.Name);
            }
            else
            {
                concurrencyProperties = Enumerable.Empty<IEdmStructuralProperty>();
            }

            IDictionary<string, object> properties = new Dictionary<string, object>();
            foreach (IEdmStructuralProperty etagProperty in concurrencyProperties)
            {
                properties.Add(etagProperty.Name, resourceContext.GetPropertyValue(etagProperty.Name));
            }
            return handler.CreateETag(properties, resourceContext.TimeZone);
        }

        private static ResourceContext CreateInstanceContext(IEdmModel model, IEdmEntityTypeReference reference, object value)
        {
            Contract.Assert(reference != null);
            Contract.Assert(value != null);

            ODataSerializerContext serializerCtx = new ODataSerializerContext
            {
                Model = model
            };

            return new ResourceContext(serializerCtx, reference, value);
        }

        // Retrieves the IEdmEntityType from the path only in the case that we are addressing a single entity.
        // We iterate the path backwards and we return as soon as we realize we are referencing a single entity.
        // That is, as soon as we find a singleton segment, a key segment or a navigation segment with target
        // multiplicity 0..1 or 1.
        internal static IEdmEntityType GetSingleEntityEntityType(ODataPath path)
        {
            if (path == null || path.Count == 0)
            {
                return null;
            }

            int currentSegmentIndex = path.Count - 1;

            // Skip a possible sequence of casts at the end of the path.
            while (currentSegmentIndex >= 0 &&
                path.ElementAt(currentSegmentIndex) is TypeSegment)
            {
                currentSegmentIndex--;
            }

            if (currentSegmentIndex < 0)
            {
                return null;
            }

            ODataPathSegment currentSegment = path.ElementAt(currentSegmentIndex);

            if (currentSegment is SingletonSegment || currentSegment is KeySegment)
            {
                return (IEdmEntityType)path.GetEdmType();
            }

            NavigationPropertySegment navigationPropertySegment = currentSegment as NavigationPropertySegment;
            if (navigationPropertySegment != null)
            {
                if (navigationPropertySegment.NavigationProperty.TargetMultiplicity() == EdmMultiplicity.ZeroOrOne ||
                    navigationPropertySegment.NavigationProperty.TargetMultiplicity() == EdmMultiplicity.One)
                {
                    return (IEdmEntityType)path.GetEdmType();
                }
            }

            return null;
        }
    }
}
