// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Formatter
{
    /// <summary>
    /// The implementation of <see cref="TextInputFormatter"/> class to handle OData reading.
    /// </summary>
    public class ODataInputFormatter : TextInputFormatter
    {
        /// <summary>
        /// The set of payload kinds this formatter will accept in CanRead.
        /// </summary>
        private readonly ISet<ODataPayloadKind> _payloadKinds;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataInputFormatter"/> class.
        /// </summary>
        /// <param name="payloadKinds">The kind of payloads this formatter supports.</param>
        public ODataInputFormatter(IEnumerable<ODataPayloadKind> payloadKinds)
        {
            if (payloadKinds == null)
            {
                throw new ArgumentNullException(nameof(payloadKinds));
            }

            _payloadKinds = new HashSet<ODataPayloadKind>(payloadKinds);
        }

        /// <summary>
        /// Gets or sets a method that allows consumers to provide an alternate base address for OData Uri.
        /// </summary>
        public Func<HttpRequest, Uri> BaseAddressFactory { get; set; }

        /// <inheritdoc/>
        public override bool CanRead(InputFormatterContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

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

            Type type = context.ModelType;
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            ODataDeserializer deserializer = GetDeserializer(request, type, out _);
            if (deserializer != null)
            {
                return _payloadKinds.Contains(deserializer.ODataPayloadKind);
            }

            return false;
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
        public override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Type type = context.ModelType;
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            HttpRequest request = context.HttpContext.Request;
            if (request == null)
            {
                throw Error.InvalidOperation(SRResources.ReadFromStreamAsyncMustHaveRequest);
            }

            // If content length is 0 then return default value for this type
            RequestHeaders contentHeaders = request.GetTypedHeaders();
            object defaultValue = GetDefaultValueForType(type);
            if (contentHeaders == null || contentHeaders.ContentLength == null)
            {
                return Task.FromResult(InputFormatterResult.Success(defaultValue));
            }

            try
            {
                var body = request.HttpContext.Features.Get<Http.Features.IHttpBodyControlFeature>();
                if (body != null)
                {
                    body.AllowSynchronousIO = true;
                }

                IList<IDisposable> toDispose = new List<IDisposable>();

                Uri baseAddress = GetBaseAddressInternal(request);

                object result = ReadFromStream(type, defaultValue, baseAddress, request, toDispose);

                foreach (IDisposable obj in toDispose)
                {
                    obj.Dispose();
                }

                return Task.FromResult(InputFormatterResult.Success(result));
            }
            catch (Exception ex)
            {
                context.ModelState.AddModelError(context.ModelName, ex, context.Metadata);
                return Task.FromResult(InputFormatterResult.Failure());
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
                throw new ArgumentNullException(nameof(request));
            }

            string baseAddress = request.CreateODataLink();

            if (baseAddress == null)
            {
                throw new SerializationException(SRResources.UnableToDetermineBaseUrl);
            }

            return baseAddress[baseAddress.Length - 1] != '/' ? new Uri(baseAddress + '/') : new Uri(baseAddress);

            /*
            LinkGenerator linkGenerator = request.HttpContext.RequestServices.GetRequiredService<LinkGenerator>();
            if (linkGenerator != null)
            {
                Endpoint endPoint = request.HttpContext.GetEndpoint();
                EndpointNameMetadata endpointName = endPoint.Metadata.GetMetadata<EndpointNameMetadata>();

                if (endpointName != null)
                {
                    string aUri = linkGenerator.GetUriByName(request.HttpContext, endpointName.EndpointName,
                        request.RouteValues, request.Scheme, request.Host, request.PathBase);
                    return new Uri(aUri);
                }

                RouteNameMetadata routeName = endPoint.Metadata.GetMetadata<RouteNameMetadata>();
                if (routeName != null)
                {
                    string aUri = linkGenerator.GetUriByRouteValues(request.HttpContext, routeName.RouteName,
                        request.RouteValues, request.Scheme, request.Host, request.PathBase);
                    return new Uri(aUri);
                }
            }

            return null;*/
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is sent to the logger, which may throw it.")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "oDataMessageReader mis registered for disposal.")]
        internal static object ReadFromStream(Type type, object defaultValue, Uri baseAddress, HttpRequest request, IList<IDisposable> disposes)
        {
            object result;
            IEdmModel model = request.GetModel();
            IEdmTypeReference expectedPayloadType;
            ODataDeserializer deserializer = GetDeserializer(request, type, out expectedPayloadType);
            if (deserializer == null)
            {
                throw Error.Argument("type", SRResources.FormatterReadIsNotSupportedForType, type.FullName, typeof(ODataInputFormatter).FullName);
            }

            try
            {
                ODataMessageReaderSettings oDataReaderSettings = request.GetReaderSettings();
                oDataReaderSettings.BaseUri = baseAddress;
                oDataReaderSettings.Validations = oDataReaderSettings.Validations & ~ValidationKinds.ThrowOnUndeclaredPropertyForNonOpenType;

                IODataRequestMessage oDataRequestMessage =
                    ODataMessageWrapperHelper.Create(request.Body, request.Headers, request.GetODataContentIdMapping(), request.GetSubServiceProvider());
                ODataMessageReader oDataMessageReader = new ODataMessageReader(oDataRequestMessage, oDataReaderSettings, model);
                disposes.Add(oDataMessageReader);

                ODataPath path = request.ODataFeature().Path;
                ODataDeserializerContext readContext = new ODataDeserializerContext
                {
                    Request = request,
                };

                readContext.Path = path;
                readContext.Model = model;
                readContext.ResourceType = type;
                readContext.ResourceEdmType = expectedPayloadType;

                result = deserializer.Read(oDataMessageReader, type, readContext);
            }
            catch (Exception ex)
            {
                LoggerError(request.HttpContext, ex);
                result = defaultValue;
            }

            return result;
        }

        private static void LoggerError(HttpContext context, Exception ex)
        {
            ILogger logger = context.RequestServices.GetService<ILogger>();
            if (logger == null)
            {
                throw ex;
            }

            logger.LogError(ex, String.Empty);
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
                return ODataInputFormatter.GetDefaultBaseAddress(request);
            }
        }

        /// <summary>
        /// Gets the deserializer and the expected payload type.
        /// </summary>
        /// <param name="request">The HttpRequest.</param>
        /// <param name="type">The input type.</param>
        /// <param name="expectedPayloadType">Output the expected payload type.</param>
        /// <returns>null or the OData deserializer</returns>
        private static ODataDeserializer GetDeserializer(HttpRequest request, Type type,  out IEdmTypeReference expectedPayloadType)
        {
            Contract.Assert(request != null);

            IODataFeature odataFeature = request.ODataFeature();
            ODataPath path = odataFeature.Path;
            IEdmModel model = odataFeature.Model;
            expectedPayloadType = null;

            ODataDeserializerProvider deserializerProvider = request.GetSubServiceProvider().GetRequiredService<ODataDeserializerProvider>();

            // Get the deserializer using the CLR type first from the deserializer provider.
            ODataDeserializer deserializer = deserializerProvider.GetODataDeserializer(type, request);
            if (deserializer == null)
            {
                expectedPayloadType = EdmLibHelper.GetExpectedPayloadType(type, path, model);
                if (expectedPayloadType != null)
                {
                    // we are in typeless mode, get the deserializer using the edm type from the path.
                    deserializer = deserializerProvider.GetEdmTypeDeserializer(expectedPayloadType);
                }
            }

            return deserializer;
        }
    }
}