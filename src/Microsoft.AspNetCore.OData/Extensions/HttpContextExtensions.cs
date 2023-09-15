//-----------------------------------------------------------------------------
// <copyright file="HttpContextExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.OData;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Common;
using System.IO;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Formatter.MediaType;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Query;

namespace Microsoft.AspNetCore.OData.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpContext"/>.
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Return the <see cref="IODataFeature"/> from the <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/> instance to extend.</param>
        /// <returns>The <see cref="IODataFeature"/>.</returns>
        public static IODataFeature ODataFeature(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw Error.ArgumentNull(nameof(httpContext));
            }

            IODataFeature odataFeature = httpContext.Features.Get<IODataFeature>();
            if (odataFeature == null)
            {
                odataFeature = new ODataFeature();
                httpContext.Features.Set(odataFeature);
            }

            return odataFeature;
        }

        /// <summary>
        /// Return the <see cref="IODataBatchFeature"/> from the <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/> instance to extend.</param>
        /// <returns>The <see cref="IODataBatchFeature"/>.</returns>
        public static IODataBatchFeature ODataBatchFeature(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw Error.ArgumentNull(nameof(httpContext));
            }

            IODataBatchFeature odataBatchFeature = httpContext.Features.Get<IODataBatchFeature>();
            if (odataBatchFeature == null)
            {
                odataBatchFeature = new ODataBatchFeature();
                httpContext.Features.Set(odataBatchFeature);
            }

            return odataBatchFeature;
        }

        /// <summary>
        /// Returns the <see cref="ODataOptions"/> instance from the DI container.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/> instance to extend.</param>
        /// <returns>The <see cref="ODataOptions"/> instance from the DI container.</returns>
        public static ODataOptions ODataOptions(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw Error.ArgumentNull(nameof(httpContext));
            }

            return httpContext.RequestServices?.GetService<IOptions<ODataOptions>>()?.Value;
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="context">TODO</param>
        /// <param name="value">TODO</param>
        /// <param name="cancellationToken">TODO</param>
        /// <returns>TODO</returns>
        public static Task WriteODataPayloadAsync<T>(
            this HttpContext context,
            object? value,
            CancellationToken cancellationToken = default)
        {
            return WriteODataPayloadAsync(context, value, typeof(T), cancellationToken);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="context">TODO</param>
        /// <param name="value">TODO</param>
        /// <param name="type">TODO</param>
        /// <param name="cancellationToken">TODO</param>
        /// <returns>TODO</returns>
        public static Task WriteODataPayloadAsync(
            this HttpContext context,
            object? value,
            Type type,
            CancellationToken cancellationToken = default)
        {
            // Ensure we have a valid request.
            HttpRequest request = context.Request;

            if (request == null)
            {
                throw Error.InvalidOperation(SRResources.WriteToResponseAsyncMustHaveRequest);
            }

            // Ignore non-OData requests.
            if (request.ODataFeature().Path == null)
            {
                return context.Response.WriteAsJsonAsync(value, type, cancellationToken);
            }

            HttpResponse response = context.Response;
            response.ContentType = context.Request.ContentType;

            MediaTypeHeaderValue contentType = GetContentType(response.Headers[HeaderNames.ContentType].FirstOrDefault());

            // Determine the content type.
            MediaTypeHeaderValue newMediaType = null;
            if (ODataOutputFormatter.TryGetContentHeader(type, contentType, out newMediaType))
            {
                response.Headers[HeaderNames.ContentType] = new StringValues(newMediaType.ToString());
            }

            // Set the character set.
            MediaTypeHeaderValue currentContentType = GetContentType(response.Headers[HeaderNames.ContentType].FirstOrDefault());
            RequestHeaders requestHeader = request.GetTypedHeaders();
            // Starting from ASP .NET Core 3.0 AcceptCharset returns an empty collection instead of null.
            if (requestHeader?.AcceptCharset.Count > 0)
            {
                IEnumerable<string> acceptCharsetValues = requestHeader.AcceptCharset.Select(cs => cs.Value.Value);

                string newCharSet = string.Empty;
                if (ODataOutputFormatter.TryGetCharSet(currentContentType, acceptCharsetValues, out newCharSet))
                {
                    currentContentType.Charset = new StringSegment(newCharSet);
                    response.Headers[HeaderNames.ContentType] = new StringValues(currentContentType.ToString());
                }
            }

            // Add version header.
            response.Headers["OData-Version"] = ODataUtils.ODataVersionToString(request.GetODataVersion());

            type = TypeHelper.GetTaskInnerTypeOrSelf(type);

            if (typeof(Stream).IsAssignableFrom(type))
            {
                // Ideally, it should go into the "ODataRawValueSerializer"
                // However, OData lib doesn't provide the method to overwrite/copyto stream
                // So, Here's the workaround
                Stream objStream = value as Stream;
                return CopyStreamAsync(objStream, response);
            }

            Uri baseAddress = ODataOutputFormatter.GetDefaultBaseAddress(request);

            IODataSerializerProvider serializerProvider = request.GetRouteServices().GetRequiredService<IODataSerializerProvider>();

            IODataSerializer serializer = ODataOutputFormatterHelper.GetSerializer(type, value, request, serializerProvider);

            ODataPath path = request.ODataFeature().Path;
            IEdmNavigationSource targetNavigationSource = path.GetNavigationSource();

            // serialize a response
            string preferHeader = RequestPreferenceHelpers.GetRequestPreferHeader(request.Headers);
            string annotationFilter = null;
            if (!string.IsNullOrEmpty(preferHeader))
            {
                ODataMessageWrapper messageWrapper = ODataMessageWrapperHelper.Create(response.Body, response.Headers);
                messageWrapper.SetHeader(RequestPreferenceHelpers.PreferHeaderName, preferHeader);
                annotationFilter = messageWrapper.PreferHeader().AnnotationFilter;
            }

            IODataResponseMessageAsync responseMessage = ODataMessageWrapperHelper.Create(new StreamWrapper(response.Body), response.Headers, request.GetRouteServices());
            if (annotationFilter != null)
            {
                responseMessage.PreferenceAppliedHeader().AnnotationFilter = annotationFilter;
            }

            ODataMessageWriterSettings writerSettings = request.GetWriterSettings();
            writerSettings.BaseUri = baseAddress;

            ODataVersion version = request.GetODataVersion();
            //use v401 to write delta payloads.
            if (serializer.ODataPayloadKind == ODataPayloadKind.Delta)
            {
                writerSettings.Version = ODataVersion.V401;
            }
            else
            {
                writerSettings.Version = version;
            }

            writerSettings.Validations = writerSettings.Validations & ~ValidationKinds.ThrowOnUndeclaredPropertyForNonOpenType;

            string metadataLink = request.CreateODataLink(MetadataSegment.Instance);
            if (metadataLink == null)
            {
                throw new SerializationException(SRResources.UnableToDetermineMetadataUrl);
            }

            // Set this variable if the SelectExpandClause is different from the processed clause on the Query options
            SelectExpandClause selectExpandDifferentFromQueryOptions = null;
            ODataQueryOptions queryOptions = request.GetQueryOptions();
            SelectExpandClause processedSelectExpandClause = request.ODataFeature().SelectExpandClause;
            if (queryOptions != null && queryOptions.SelectExpand != null)
            {
                if (queryOptions.SelectExpand.ProcessedSelectExpandClause != processedSelectExpandClause)
                {
                    selectExpandDifferentFromQueryOptions = processedSelectExpandClause;
                }
            }
            else if (processedSelectExpandClause != null)
            {
                selectExpandDifferentFromQueryOptions = processedSelectExpandClause;
            }

            writerSettings.ODataUri = new ODataUri
            {
                ServiceRoot = baseAddress,

                // TODO: 1604 Convert webapi.odata's ODataPath to ODL's ODataPath, or use ODL's ODataPath.
                SelectAndExpand = processedSelectExpandClause,
                Apply = request.ODataFeature().ApplyClause,
                Path = path
            };

            ODataMetadataLevel metadataLevel = ODataMetadataLevel.Minimal;
            if (contentType != null)
            {
                IEnumerable<KeyValuePair<string, string>> parameters =
                    contentType.Parameters.Select(val => new KeyValuePair<string, string>(val.Name.ToString(),
                    val.Value.ToString()));
                metadataLevel = ODataMediaTypes.GetMetadataLevel(contentType.MediaType.ToString(), parameters);
            }

            IEdmModel model = request.GetModel();

            using (ODataMessageWriter messageWriter = new ODataMessageWriter(responseMessage, writerSettings, model))
            {
                ODataSerializerContext writeContext = ODataOutputFormatterHelper.BuildSerializerContext(request);
                writeContext.NavigationSource = targetNavigationSource;
                writeContext.Model = model;
                writeContext.RootElementName = GetRootElementName(path) ?? "root";
                writeContext.SkipExpensiveAvailabilityChecks = serializer.ODataPayloadKind == ODataPayloadKind.ResourceSet;
                writeContext.Path = path;
                writeContext.MetadataLevel = metadataLevel;
                writeContext.QueryOptions = queryOptions;
                writeContext.SetComputedProperties(queryOptions?.Compute?.ComputeClause);
                writeContext.Type = type;

                //Set the SelectExpandClause on the context if it was explicitly specified.
                if (selectExpandDifferentFromQueryOptions != null)
                {
                    writeContext.SelectExpandClause = selectExpandDifferentFromQueryOptions;
                }

                return serializer.WriteObjectAsync(value, type, messageWriter, writeContext);
            }
        }

        private static MediaTypeHeaderValue GetContentType(string contentTypeValue)
        {
            MediaTypeHeaderValue contentType = null;
            if (!string.IsNullOrEmpty(contentTypeValue))
            {
                MediaTypeHeaderValue.TryParse(contentTypeValue, out contentType);
            }

            return contentType;
        }

        private static async Task CopyStreamAsync(Stream source, HttpResponse response)
        {
            if (source != null)
            {
                await source.CopyToAsync(response.Body).ConfigureAwait(false);
            }

            await response.Body.FlushAsync().ConfigureAwait(false);
        }

        private static string GetRootElementName(ODataPath path)
        {
            if (path != null)
            {
                ODataPathSegment lastSegment = path.LastSegment;
                if (lastSegment != null)
                {
                    OperationSegment actionSegment = lastSegment as OperationSegment;
                    if (actionSegment != null)
                    {
                        IEdmAction action = actionSegment.Operations.Single() as IEdmAction;
                        if (action != null)
                        {
                            return action.Name;
                        }
                    }

                    PropertySegment propertyAccessSegment = lastSegment as PropertySegment;
                    if (propertyAccessSegment != null)
                    {
                        return propertyAccessSegment.Property.Name;
                    }
                }
            }

            return null;
        }
#endif
    }
}
