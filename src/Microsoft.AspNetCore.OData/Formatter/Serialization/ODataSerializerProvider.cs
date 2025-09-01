//-----------------------------------------------------------------------------
// <copyright file="ODataSerializerProvider.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization;

/// <summary>
/// The default implementation of <see cref="IODataSerializerProvider"/>.
/// </summary>
public class ODataSerializerProvider: IODataSerializerProvider
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataSerializerProvider"/> class.
    /// </summary>
    /// <param name="serviceProvider">The root container.</param>
    public ODataSerializerProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw Error.ArgumentNull(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public virtual IODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
    {
        if (edmType == null)
        {
            throw new ArgumentNullException(nameof(edmType));
        }

        switch (edmType.TypeKind())
        {
            case EdmTypeKind.Enum:
                return _serviceProvider.GetRequiredService<ODataEnumSerializer>();

            case EdmTypeKind.Primitive:
                if (edmType.IsSpatial())
                {
                    return _serviceProvider.GetRequiredService<ODataSpatialSerializer>();
                }

                return _serviceProvider.GetRequiredService<ODataPrimitiveSerializer>();

            case EdmTypeKind.Collection:
                IEdmCollectionTypeReference collectionType = edmType.AsCollection();
                if (collectionType.Definition.IsDeltaResourceSet())
                {
                    return _serviceProvider.GetRequiredService<ODataDeltaResourceSetSerializer>();
                }
                else if (collectionType.ElementType().IsEntity() || collectionType.ElementType().IsComplex() 
                    || collectionType.ElementType().IsUntyped())
                {
                    return _serviceProvider.GetRequiredService<ODataResourceSetSerializer>();
                }
                else
                {
                    return _serviceProvider.GetRequiredService<ODataCollectionSerializer>();
                }

            case EdmTypeKind.Complex:
            case EdmTypeKind.Entity:
            case EdmTypeKind.Untyped:
                return _serviceProvider.GetRequiredService<ODataResourceSerializer>();

            default:
                return null;
        }
    }

    /// <inheritdoc />
    public virtual IODataSerializer GetODataPayloadSerializer(Type type, HttpRequest request)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        ODataPath path = request.ODataFeature().Path;
        Type errorType = typeof(SerializableError);

        // handle the special types.
        if (type == typeof(ODataServiceDocument))
        {
            return _serviceProvider.GetRequiredService<ODataServiceDocumentSerializer>();
        }
        else if (type == typeof(Uri) || type == typeof(ODataEntityReferenceLink))
        {
            return _serviceProvider.GetRequiredService<ODataEntityReferenceLinkSerializer>();
        }
        else if (TypeHelper.IsTypeAssignableFrom(typeof(IEnumerable<Uri>), type) || type == typeof(ODataEntityReferenceLinks))
        {
            return _serviceProvider.GetRequiredService<ODataEntityReferenceLinksSerializer>();
        }
        else if (type == typeof(ODataError) || type == errorType)
        {
            return _serviceProvider.GetRequiredService<ODataErrorSerializer>();
        }
        else if (TypeHelper.IsTypeAssignableFrom(typeof(IEdmModel), type))
        {
            return _serviceProvider.GetRequiredService<ODataMetadataSerializer>();
        }
        else if (typeof(IDeltaSet).IsAssignableFrom(type))
        {
            return _serviceProvider.GetRequiredService<ODataDeltaResourceSetSerializer>();
        }

        IEdmModel model = request.GetModel();

        // if it is not a special type, assume it has a corresponding EdmType.
        IEdmTypeReference edmType = model.GetEdmTypeReference(type);

        if (edmType != null)
        {
            bool isCountRequest = path != null && path.LastSegment is CountSegment;
            bool isRawValueRequest = path != null && path.LastSegment is ValueSegment;
            bool isStreamRequest = path.IsStreamPropertyPath();

            if (((edmType.IsPrimitive() || edmType.IsEnum()) && isRawValueRequest) || isCountRequest || isStreamRequest)
            {
                // Should rethink about the stream property serializer
                return _serviceProvider.GetRequiredService<ODataRawValueSerializer>();
            }
            else
            {
                return GetEdmTypeSerializer(edmType);
            }
        }
        else
        {
            // Ok, we are here because the value type is not defined in OData Edm model
            // (Known primitive, known enum, known strucutred or collection are handled above.)
            // One of such case is that it's an untyped property (or dynamic property) query request
            // and the value type is an unknown to OData Edm model.
            if (path.IsUntypedPropertyPath())
            {
                edmType = TypeHelper.GetUntypedEdmType(type);
                return GetEdmTypeSerializer(edmType);
            }

            return null;
        }
    }
}
