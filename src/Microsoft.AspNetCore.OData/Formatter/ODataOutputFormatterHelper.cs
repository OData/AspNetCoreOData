// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.MediaType;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Formatter
{
    internal static class ODataOutputFormatterHelper
    {
        public static ODataSerializerContext BuildSerializerContext(HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            TimeZoneInfo timeZone = null;
            IOptions<ODataOptions> odataOptions = request.HttpContext.RequestServices.GetService<IOptions<ODataOptions>>();
            if (odataOptions != null && odataOptions.Value == null)
            {
                timeZone = odataOptions.Value.TimeZone;
            }

            return new ODataSerializerContext()
            {
                Request = request,
                TimeZone = timeZone,
            };
        }
        

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class coupling acceptable")]
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        internal static async Task WriteToStreamAsync(
            Type type,
            object value,
            IEdmModel model,
            ODataVersion version,
            Uri baseAddress,
            MediaTypeHeaderValue contentType,
            HttpRequest request,
            IHeaderDictionary requestHeaders,
            ODataSerializerProvider serializerProvider)
        {
            if (model == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustHaveModel);
            }

            ODataSerializer serializer = GetSerializer(type, value, request, serializerProvider);

            ODataPath path = request.ODataFeature().Path;
            IEdmNavigationSource targetNavigationSource = path == null ? null : path.GetNavigationSource();
            HttpResponse response = request.HttpContext.Response;

            // serialize a response
            string preferHeader = RequestPreferenceHelpers.GetRequestPreferHeader(requestHeaders);
            string annotationFilter = null;
            if (!string.IsNullOrEmpty(preferHeader))
            {
                ODataMessageWrapper messageWrapper = ODataMessageWrapperHelper.Create(response.Body, response.Headers);
                messageWrapper.SetHeader(RequestPreferenceHelpers.PreferHeaderName, preferHeader);
                annotationFilter = messageWrapper.PreferHeader().AnnotationFilter;
            }

            IODataResponseMessageAsync responseMessage = ODataMessageWrapperHelper.Create(new StreamWrapper(response.Body), response.Headers, request.GetSubServiceProvider());
            if (annotationFilter != null)
            {
                responseMessage.PreferenceAppliedHeader().AnnotationFilter = annotationFilter;
            }

            ODataMessageWriterSettings writerSettings = request.GetWriterSettings();
            writerSettings.BaseUri = baseAddress;
            writerSettings.Version = version;
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
                //Path = (path == null || IsOperationPath(path)) ? null : path.Path,
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

            using (ODataMessageWriter messageWriter = new ODataMessageWriter(responseMessage, writerSettings, model))
            {
                ODataSerializerContext writeContext = BuildSerializerContext(request);
                writeContext.NavigationSource = targetNavigationSource;
                writeContext.Model = model;
                writeContext.RootElementName = GetRootElementName(path) ?? "root";
                writeContext.SkipExpensiveAvailabilityChecks = serializer.ODataPayloadKind == ODataPayloadKind.ResourceSet;
                writeContext.Path = path;
                writeContext.MetadataLevel = metadataLevel;
                writeContext.QueryOptions = queryOptions;

                //Set the SelectExpandClause on the context if it was explicitly specified.
                if (selectExpandDifferentFromQueryOptions != null)
                {
                    writeContext.SelectExpandClause = selectExpandDifferentFromQueryOptions;
                }

                await serializer.WriteObjectAsync(value, type, messageWriter, writeContext).ConfigureAwait(false);
            }
        }

        private static ODataSerializer GetSerializer(Type type, object value, HttpRequest request,
            ODataSerializerProvider serializerProvider)
        {
            ODataSerializer serializer;

            IEdmObject edmObject = value as IEdmObject;
            if (edmObject != null)
            {
                IEdmTypeReference edmType = edmObject.GetEdmType();
                if (edmType == null)
                {
                    throw new SerializationException(Error.Format(SRResources.EdmTypeCannotBeNull,
                        edmObject.GetType().FullName, typeof(IEdmObject).Name));
                }

                serializer = serializerProvider.GetEdmTypeSerializer(edmType);
                if (serializer == null)
                {
                    string message = Error.Format(SRResources.TypeCannotBeSerialized, edmType.ToTraceString());
                    throw new SerializationException(message);
                }
            }
            else
            {
                var applyClause = request.ODataFeature().ApplyClause;

                // get the most appropriate serializer given that we support inheritance.
                if (applyClause == null)
                {
                    type = value == null ? type : value.GetType();
                }
                type = value == null ? type : value.GetType();

                serializer = serializerProvider.GetODataPayloadSerializer(type, request);
                if (serializer == null)
                {
                    string message = Error.Format(SRResources.TypeCannotBeSerialized, type.Name);
                    throw new SerializationException(message);
                }
            }

            return serializer;
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
    }

    // Since OData metadata write is not async.
    // Any $metadata request will throw "Synchronous operations are disallowed. Call WriteAsync or set AllowSynchronousIO to true."
    // So, we have to use "StreamWrapper" to override "Write(byte[] buffer, int offset, int count)"
    // Once we enable async for metadata writer, we should remove this class.
    internal class StreamWrapper : Stream
    {
        private Stream stream;
        public StreamWrapper(Stream stream)
        {
            this.stream = stream;
        }

        public override bool CanRead => this.stream.CanRead;

        public override bool CanSeek => this.stream.CanSeek;

        public override bool CanWrite => this.stream.CanWrite;

        public override long Length => this.stream.Length;

        public override int ReadTimeout { get => this.stream.ReadTimeout; set => this.stream.ReadTimeout = value; }

        public override int WriteTimeout { get => this.stream.WriteTimeout; set => this.stream.WriteTimeout = value; }

        public override bool CanTimeout => this.stream.CanTimeout;

        public override void Close()
        {
            this.stream.Close();
        }

        public override long Position { get => this.stream.Position; set => this.stream.Position = value; }

        public override void Flush()
        {
            this.stream.FlushAsync().Wait();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return stream.FlushAsync(cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.stream.ReadAsync(buffer, offset, count).Result;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return this.stream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override int ReadByte()
        {
            return this.stream.ReadByte();
        }

        public override void WriteByte(byte value)
        {
            this.stream.WriteByte(value);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return this.stream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.stream.WriteAsync(buffer, offset, count).GetAwaiter().GetResult();
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return this.stream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.stream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.stream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return this.stream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.stream.EndWrite(asyncResult);
        }

        public override string ToString()
        {
            return this.stream.ToString();
        }
    }
}
