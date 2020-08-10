// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.MediaType;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Formatter
{
    internal static class ODataOutputFormatterHelper
    {
        public static ODataSerializerContext BuildSerializerContext(HttpRequest request) =>
            new ODataSerializerContext()
            {
                Request = request,
                LinkGenerator = request.HttpContext.RequestServices.GetRequiredService<LinkGenerator>()
            };
        

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class coupling acceptable")]
        internal static void WriteToStream(
            Type type,
            object value,
            IEdmModel model,
            ODataVersion version,
            Uri baseAddress,
            MediaTypeHeaderValue contentType,
            IUrlHelper urlHelper,
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
            if (!String.IsNullOrEmpty(preferHeader))
            {
                ODataMessageWrapper messageWrapper = ODataMessageWrapperHelper.Create(response.Body, response.Headers);
                messageWrapper.SetHeader(RequestPreferenceHelpers.PreferHeaderName, preferHeader);
                annotationFilter = messageWrapper.PreferHeader().AnnotationFilter;
            }

            IODataResponseMessage responseMessage = ODataMessageWrapperHelper.Create(response.Body, response.Headers/*, request.HttpContext.RequestServices*/);
            if (annotationFilter != null)
            {
                responseMessage.PreferenceAppliedHeader().AnnotationFilter = annotationFilter;
            }

            ODataMessageWriterSettings writerSettings = request.GetWriterSettings();
            writerSettings.BaseUri = baseAddress;
            writerSettings.Version = version;
            writerSettings.Validations = writerSettings.Validations & ~ValidationKinds.ThrowOnUndeclaredPropertyForNonOpenType;

            string metadataLink = urlHelper.CreateODataLink(MetadataSegment.Instance);
            if (metadataLink == null || metadataLink == "")
            {
                metadataLink = request.CreateODataLink(baseAddress.OriginalString, MetadataSegment.Instance);
            }

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

                serializer.WriteObject(value, type, messageWriter, writeContext);
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
                //var applyClause = request.Context.ApplyClause;

                //// get the most appropriate serializer given that we support inheritance.
                //if (applyClause == null)
                //{
                //    type = value == null ? type : value.GetType();
                //}
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
}
