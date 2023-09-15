//-----------------------------------------------------------------------------
// <copyright file="ODataBinding.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Formatter.Wrapper
{
#if NET7_0_OR_GREATER
    /// <summary>
    /// TODO
    /// </summary>
    /// <typeparam name="T">TODO</typeparam>
    public class ODataBinding<T> where T : class
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="value">TODO</param>
        public ODataBinding(T value) => Value = value;

        /// <summary>
        /// TODO
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="context">TODO</param>
        /// <param name="parameter">TODO</param>
        /// <returns>TODO</returns>
        /// <exception cref="NotImplementedException">TODO</exception>
        public static async ValueTask<ODataBinding<T>> BindAsync(HttpContext context, ParameterInfo parameter)
        {
            HttpRequest request = context.Request;
            Type type = typeof(T);

            IEdmTypeReference expectedPayloadType;
            IODataDeserializer deserializer = GetDeserializer(request, type, out expectedPayloadType);

            object result;
            object defaultValue = GetDefaultValueForType(type);

            try
            {
                IEdmModel model = request.GetModel();
                Uri baseAddress = ODataInputFormatter.GetDefaultBaseAddress(request);
                ODataMessageReaderSettings odataReaderSettings = request.GetReaderSettings();
                odataReaderSettings.BaseUri = baseAddress;
                odataReaderSettings.Validations = odataReaderSettings.Validations & ~ValidationKinds.ThrowOnUndeclaredPropertyForNonOpenType;
                odataReaderSettings.Version = ODataVersion.V4;

                IODataRequestMessage odataRequestMessage =
                    ODataMessageWrapperHelper.Create(new StreamWrapper(request.Body), request.Headers, request.GetODataContentIdMapping(), request.GetRouteServices());

                using (ODataMessageReader odataMessageReader = new ODataMessageReader(odataRequestMessage, odataReaderSettings, model))
                {
                    ODataPath path = request.ODataFeature().Path;
                    ODataDeserializerContext readContext = BuildDeserializerContext(request);

                    readContext.Path = path;
                    readContext.Model = model;
                    readContext.ResourceType = type;
                    readContext.ResourceEdmType = expectedPayloadType;

                    result = await deserializer.ReadAsync(odataMessageReader, type, readContext).ConfigureAwait(false);
                }

                return new ODataBinding<T>(result as T);
            }
            catch (Exception ex)
            {
                LoggerError(request.HttpContext, ex);
                return new ODataBinding<T>(defaultValue as T);
            }
        }

        private static object GetDefaultValueForType(Type modelType)
        {
            if (modelType == null)
            {
                throw new ArgumentNullException(nameof(modelType));
            }

            if (modelType.IsValueType)
            {
                return Activator.CreateInstance(modelType);
            }

            return null;
        }

        private static IODataDeserializer GetDeserializer(HttpRequest request, Type type, out IEdmTypeReference expectedPayloadType)
        {
            Contract.Assert(request != null);

            IODataFeature odataFeature = request.ODataFeature();
            ODataPath path = odataFeature.Path;
            IEdmModel model = odataFeature.Model;
            expectedPayloadType = null;

            IODataDeserializerProvider deserializerProvider = request.GetRouteServices().GetRequiredService<IODataDeserializerProvider>();

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
    }

#endif
}
