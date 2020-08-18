// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.MediaType;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter
{
    /// <summary>
    /// <see cref="TextOutputFormatter"/> class to handle OData.
    /// </summary>
    public class ODataOutputFormatter : TextOutputFormatter, IMediaTypeMappingCollection
    {
        /// <summary>
        /// The set of payload kinds this formatter will accept in CanWriteType.
        /// </summary>
        private readonly ISet<ODataPayloadKind> _payloadKinds;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataOutputFormatter"/> class.
        /// </summary>
        /// <param name="payloadKinds">The kind of payloads this formatter supports.</param>
        public ODataOutputFormatter(IEnumerable<ODataPayloadKind> payloadKinds)
        {
            if (payloadKinds == null)
            {
                throw new ArgumentNullException(nameof(payloadKinds));
            }

            _payloadKinds = new HashSet<ODataPayloadKind>(payloadKinds);
        }

        /// <summary>
        /// Gets or sets a method that allows consumers to provide an alternate base
        /// address for OData Uri.
        /// </summary>
        public Func<HttpRequest, Uri> BaseAddressFactory { get; set; }

        /// <summary>
        /// Gets a collection of <see cref="MediaTypeMapping"/> objects.
        /// </summary>
        public ICollection<MediaTypeMapping> MediaTypeMappings { get; } = new List<MediaTypeMapping>();

        /// <inheritdoc/>
        public override bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            // Ensure we have a valid request.
            HttpRequest request = context.HttpContext.Request;
            if (request == null)
            {
                throw Error.InvalidOperation(SRResources.ReadFromStreamAsyncMustHaveRequest);
            }

            // Ignore non-OData requests.
            if (request.ODataFeature().Path == null)
            {
                return false;
            }

            // Allow the base class to make its determination, which includes
            // checks for SupportedMediaTypes.
            bool suportedMediaTypeFound = false;
            if (SupportedMediaTypes.Any())
            {
                suportedMediaTypeFound = base.CanWriteResult(context);
            }

            // See if the request satisfies any mappings.
            IEnumerable<MediaTypeMapping> matchedMappings = (MediaTypeMappings == null) ? null : MediaTypeMappings
                .Where(m => (m.TryMatchMediaType(request) > 0));

            // Now pick the best content type. If a media mapping was found, use that and override the
            // value specified by the controller, if any. Otherwise, let the base class decide.
            if (matchedMappings != null && matchedMappings.Any())
            {
                context.ContentType = matchedMappings.First().MediaType.ToString();
            }
            else if (!suportedMediaTypeFound)
            {
                return false;
            }

            // We need the type in order to write it.
            Type type = context.ObjectType ?? context.Object?.GetType();
            if (type == null)
            {
                return false;
            }
            type = TypeHelper.GetTaskInnerTypeOrSelf(type);

            ODataSerializerProvider serializerProvider = request.HttpContext.RequestServices.GetRequiredService<ODataSerializerProvider>();

            // See if this type is a SingleResult or is derived from SingleResult.
            bool isSingleResult = false;
            if (type.IsGenericType)
            {
                Type genericType = type.GetGenericTypeDefinition();
                Type baseType = type.BaseType;
                isSingleResult = (genericType == typeof(SingleResult<>) || baseType == typeof(SingleResult));
            }

            ODataPayloadKind? payloadKind;

            Type elementType;
            if (typeof(IEdmObject).IsAssignableFrom(type) ||
                (TypeHelper.IsCollection(type, out elementType) && typeof(IEdmObject).IsAssignableFrom(elementType)))
            {
                payloadKind = GetEdmObjectPayloadKind(type, request);
            }
            else
            {
                payloadKind = GetClrObjectResponsePayloadKind(type, isSingleResult, serializerProvider, request);
            }

            return payloadKind == null ? false : _payloadKinds.Contains(payloadKind.Value);
        }

        private static ODataPayloadKind? GetClrObjectResponsePayloadKind(Type type, bool isGenericSingleResult, ODataSerializerProvider serializerProvider
            , HttpRequest request)
        {
            // SingleResult<T> should be serialized as T.
            if (isGenericSingleResult)
            {
                type = type.GetGenericArguments()[0];
            }

            ODataSerializer serializer = serializerProvider.GetODataPayloadSerializer(type, request);
            return serializer == null ? null : (ODataPayloadKind?)serializer.ODataPayloadKind;
        }

        /// <inheritdoc/>
        public override void WriteResponseHeaders(OutputFormatterWriteContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            Type type = context.ObjectType;
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            type = TypeHelper.GetTaskInnerTypeOrSelf(type);

            HttpRequest request = context.HttpContext.Request;
            if (request == null)
            {
                throw Error.InvalidOperation(SRResources.WriteToStreamAsyncMustHaveRequest);
            }

            HttpResponse response = context.HttpContext.Response;
            response.ContentType = context.ContentType.Value;

            MediaTypeHeaderValue contentType = GetContentType(response.Headers[HeaderNames.ContentType].FirstOrDefault());

            // Determine the content type.
            MediaTypeHeaderValue newMediaType = null;
            if (TryGetContentHeader(type, contentType, out newMediaType))
            {
                response.Headers[HeaderNames.ContentType] = new StringValues(newMediaType.ToString());
            }

            // Set the character set.
            MediaTypeHeaderValue currentContentType = GetContentType(response.Headers[HeaderNames.ContentType].FirstOrDefault());
            RequestHeaders requestHeader = request.GetTypedHeaders();
            if (requestHeader != null && requestHeader.AcceptCharset != null)
            {
                IEnumerable<string> acceptCharsetValues = requestHeader.AcceptCharset.Select(cs => cs.Value.Value);

                string newCharSet = string.Empty;
                if (TryGetCharSet(currentContentType, acceptCharsetValues, out newCharSet))
                {
                    currentContentType.Charset = new StringSegment(newCharSet);
                    response.Headers[HeaderNames.ContentType] = new StringValues(currentContentType.ToString());
                }
            }

            // Add version header.
            response.Headers["OData-Version"] = ODataUtils.ODataVersionToString(request.GetODataResponseVersion());
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            Type type = context.ObjectType;
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            type = TypeHelper.GetTaskInnerTypeOrSelf(type);

            HttpRequest request = context.HttpContext.Request;
            if (request == null)
            {
                throw Error.InvalidOperation(SRResources.WriteToStreamAsyncMustHaveRequest);
            }

            try
            {
                var body = request.HttpContext.Features.Get<AspNetCore.Http.Features.IHttpBodyControlFeature>();
                if (body != null)
                {
                    body.AllowSynchronousIO = true;
                }

                HttpResponse response = context.HttpContext.Response;
                Uri baseAddress = GetBaseAddressInternal(request);
                MediaTypeHeaderValue contentType = GetContentType(response.Headers[HeaderNames.ContentType].FirstOrDefault());

                ODataSerializerProvider serializerProvider = request.HttpContext.RequestServices.GetRequiredService<ODataSerializerProvider>();

                ODataOutputFormatterHelper.WriteToStream(
                    type,
                    context.Object,
                    request.GetModel(),
                    request.GetODataResponseVersion(),
                    baseAddress,
                    contentType,
                    request.GetUrlHelper(),
                    request,
                    request.Headers,
                    serializerProvider);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }

        /// <summary>
        /// Internal method used for selecting the base address to be used with OData uris.
        /// If the consumer has provided a delegate for overriding our default implementation,
        /// we call that, otherwise we default to existing behavior below.
        /// </summary>
        /// <param name="request">The HttpRequest object for the given request.</param>
        /// <returns>The base address to be used as part of the service root; must terminate with a trailing '/'.</returns>
        private Uri GetBaseAddressInternal(HttpRequest request)
        {
            if (BaseAddressFactory != null)
            {
                return BaseAddressFactory(request);
            }
            else
            {
                return ODataOutputFormatter.GetDefaultBaseAddress(request);
            }
        }

        /// <summary>
        /// Returns a base address to be used in the service root when reading or writing OData uris.
        /// </summary>
        /// <param name="request">The HttpRequest object for the given request.</param>
        /// <returns>The base address to be used as part of the service root in the OData uri; must terminate with a trailing '/'.</returns>
        public static Uri GetDefaultBaseAddress(HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            //LinkGenerator linkGenerator = request.HttpContext.RequestServices.GetRequiredService<LinkGenerator>();
            //if (linkGenerator != null)
            //{
            // //   string uri = linkGenerator.GetUriByAction(request.HttpContext);

            //    Endpoint endPoint = request.HttpContext.GetEndpoint();
            //    EndpointNameMetadata name = endPoint.Metadata.GetMetadata<EndpointNameMetadata>();


            //    string aUri = linkGenerator.GetUriByName(request.HttpContext, name?.EndpointName,
            //        request.RouteValues, request.Scheme, request.Host, request.PathBase);

            //    return new Uri(aUri);
            //}

            string baseAddress = request.CreateODataLink();
            if (baseAddress == null)
            {
                throw new SerializationException(SRResources.UnableToDetermineBaseUrl);
            }

            return baseAddress[baseAddress.Length - 1] != '/' ? new Uri(baseAddress + '/') : new Uri(baseAddress);
        }

        internal static bool TryGetCharSet(MediaTypeHeaderValue mediaType, IEnumerable<string> acceptCharsetValues, out string charSet)
        {
            charSet = String.Empty;

            // In general, in Web API we pick a default charset based on the supported character sets
            // of the formatter. However, according to the OData spec, the service shouldn't be sending
            // a character set unless explicitly specified, so if the client didn't send the charset we chose
            // we just clean it.
            if (mediaType != null &&
                !acceptCharsetValues
                    .Any(cs => cs.Equals(mediaType.Charset.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }


        internal static bool TryGetContentHeader(Type type, MediaTypeHeaderValue mediaType, out MediaTypeHeaderValue newMediaType)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            newMediaType = null;

            // When the user asks for application/json we really need to set the content type to
            // application/json; odata.metadata=minimal. If the user provides the media type and is
            // application/json we are going to add automatically odata.metadata=minimal. Otherwise we are
            // going to fallback to the default implementation.

            // When calling this formatter as part of content negotiation the content negotiator will always
            // pick a non null media type. In case the user creates a new ObjectContent<T> and doesn't pass in a
            // media type, we delegate to the base class to rely on the default behavior. It's the user's
            // responsibility to pass in the right media type.

            if (mediaType != null)
            {
                if (mediaType.MediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase) &&
                    !mediaType.Parameters.Any(p => p.Name.Equals("odata.metadata", StringComparison.OrdinalIgnoreCase)))
                {
                    mediaType.Parameters.Add(new NameValueHeaderValue("odata.metadata", "minimal"));
                }

                //newMediaType = (MediaTypeHeaderValue)((ICloneable)mediaType).Clone();
                newMediaType = mediaType.Copy();
                return true;
            }
            else
            {
                // This is the case when a user creates a new ObjectContent<T> passing in a null mediaType
                return false;
            }
        }

        private MediaTypeHeaderValue GetContentType(string contentTypeValue)
        {
            MediaTypeHeaderValue contentType = null;
            if (!string.IsNullOrEmpty(contentTypeValue))
            {
                MediaTypeHeaderValue.TryParse(contentTypeValue, out contentType);
            }

            return contentType;
        }

        private static ODataPayloadKind? GetEdmObjectPayloadKind(Type type, HttpRequest request)
        {
            //if (internalRequest.IsCountRequest())
            //{
            //    return ODataPayloadKind.Value;
            //}

            Type elementType;
            if (TypeHelper.IsCollection(type, out elementType))
            {
                if (typeof(IEdmComplexObject).IsAssignableFrom(elementType) || typeof(IEdmEnumObject).IsAssignableFrom(elementType))
                {
                    return ODataPayloadKind.Collection;
                }
                else if (typeof(IEdmEntityObject).IsAssignableFrom(elementType))
                {
                    return ODataPayloadKind.ResourceSet;
                }
                else if (typeof(IEdmChangedObject).IsAssignableFrom(elementType))
                {
                    return ODataPayloadKind.Delta;
                }
            }
            else
            {
                if (typeof(IEdmComplexObject).IsAssignableFrom(elementType) || typeof(IEdmEnumObject).IsAssignableFrom(elementType))
                {
                    return ODataPayloadKind.Property;
                }
                else if (typeof(IEdmEntityObject).IsAssignableFrom(elementType))
                {
                    return ODataPayloadKind.Resource;
                }
            }

            return null;
        }
    }
}
