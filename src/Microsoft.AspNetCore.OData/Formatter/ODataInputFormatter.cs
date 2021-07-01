// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
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

        /// <inheritdoc />
        public override IReadOnlyList<string> GetSupportedContentTypes(string contentType, Type objectType)
        {
            if (SupportedMediaTypes.Count == 0)
            {
                // note: this is parity with the base implementation when there are no matches
                return default;
            }

            return base.GetSupportedContentTypes(contentType, objectType);
        }

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

            IODataDeserializer deserializer = GetDeserializer(request, type, out _);
            if (deserializer != null)
            {
                return _payloadKinds.Contains(deserializer.ODataPayloadKind);
            }

            return false;
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
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

            object defaultValue = GetDefaultValueForType(type);
            try
            {

                IList<IDisposable> toDispose = new List<IDisposable>();

                Uri baseAddress = GetBaseAddressInternal(request);

                object result = await ReadFromStreamAsync(
                    type,
                    defaultValue,
                    baseAddress,
                    request.GetODataVersion(),
                    request,
                    toDispose).ConfigureAwait(false);

                foreach (IDisposable obj in toDispose)
                {
                    obj.Dispose();
                }

                return InputFormatterResult.Success(result);
            }
            catch (Exception ex)
            {
                context.ModelState.AddModelError(context.ModelName, ex, context.Metadata);
                return InputFormatterResult.Failure();
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

        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is sent to the logger, which may throw it.")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "oDataMessageReader mis registered for disposal.")]
        internal static async Task<object> ReadFromStreamAsync(
            Type type,
            object defaultValue,
            Uri baseAddress,
            ODataVersion version,
            HttpRequest request,
            IList<IDisposable> disposes)
        {
            object result;
            IEdmModel model = request.GetModel();
            IEdmTypeReference expectedPayloadType;
            IODataDeserializer deserializer = GetDeserializer(request, type, out expectedPayloadType);
            if (deserializer == null)
            {
                throw Error.Argument("type", SRResources.FormatterReadIsNotSupportedForType, type.FullName, typeof(ODataInputFormatter).FullName);
            }

            try
            {
                ODataMessageReaderSettings oDataReaderSettings = request.GetReaderSettings();
                oDataReaderSettings.BaseUri = baseAddress;
                oDataReaderSettings.Validations = oDataReaderSettings.Validations & ~ValidationKinds.ThrowOnUndeclaredPropertyForNonOpenType;
                oDataReaderSettings.Version = version;

                // WebAPI should read untyped values as structural values by setting ReadUntypedAsString=false.
                // In ODL 8.x, ReadUntypedAsString option will be deleted.
                oDataReaderSettings.ReadUntypedAsString = false;

                IODataRequestMessage oDataRequestMessage =
                    ODataMessageWrapperHelper.Create(new StreamWrapper(request.Body), request.Headers, request.GetODataContentIdMapping(), request.GetSubServiceProvider());
                ODataMessageReader oDataMessageReader = new ODataMessageReader(oDataRequestMessage, oDataReaderSettings, model);
                disposes.Add(oDataMessageReader);

                ODataPath path = request.ODataFeature().Path;
                ODataDeserializerContext readContext = BuildDeserializerContext(request);

                readContext.Path = path;
                readContext.Model = model;
                readContext.ResourceType = type;
                readContext.ResourceEdmType = expectedPayloadType;

                result = await deserializer.ReadAsync(oDataMessageReader, type, readContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LoggerError(request.HttpContext, ex);
                result = defaultValue;
            }

            return result;
        }

        private static ODataDeserializerContext BuildDeserializerContext(HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            return new ODataDeserializerContext()
            {
                Request = request,
                TimeZone = request.GetTimeZoneInfo(),
            };
        }

        private static void LoggerError(HttpContext context, Exception ex)
        {
            ILogger logger = context.RequestServices.GetService<ILogger>();
            if (logger == null)
            {
                ExceptionDispatchInfo.Capture(ex).Throw();
            }

            logger.LogError(ex, string.Empty);
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
        private static IODataDeserializer GetDeserializer(HttpRequest request, Type type,  out IEdmTypeReference expectedPayloadType)
        {
            Contract.Assert(request != null);

            IODataFeature odataFeature = request.ODataFeature();
            ODataPath path = odataFeature.Path;
            IEdmModel model = odataFeature.Model;
            expectedPayloadType = null;

            IODataDeserializerProvider deserializerProvider = request.GetSubServiceProvider().GetRequiredService<IODataDeserializerProvider>();

            // Get the deserializer using the CLR type first from the deserializer provider.
            IODataDeserializer deserializer = deserializerProvider.GetODataDeserializer(type, request);
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