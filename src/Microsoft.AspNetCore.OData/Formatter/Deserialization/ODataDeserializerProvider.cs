//-----------------------------------------------------------------------------
// <copyright file="ODataDeserializerProvider.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Deserialization
{
    /// <summary>
    /// The default <see cref="IODataDeserializerProvider"/>.
    /// </summary>
    public class ODataDeserializerProvider: IODataDeserializerProvider
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataDeserializerProvider"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public ODataDeserializerProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw Error.ArgumentNull(nameof(serviceProvider));
        }

        /// <inheritdoc />
        public virtual IODataEdmTypeDeserializer GetEdmTypeDeserializer(IEdmTypeReference edmType, bool isDelta = false)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull(nameof(edmType));
            }

            switch (edmType.TypeKind())
            {
                case EdmTypeKind.Entity:
                case EdmTypeKind.Complex:
                case EdmTypeKind.Untyped:
                    return _serviceProvider.GetRequiredService<ODataResourceDeserializer>();

                case EdmTypeKind.Enum:
                    return _serviceProvider.GetRequiredService<ODataEnumDeserializer>();

                case EdmTypeKind.Primitive:
                    return _serviceProvider.GetRequiredService<ODataPrimitiveDeserializer>();

                case EdmTypeKind.Collection:
                    if (isDelta)
                    {
                        return _serviceProvider.GetRequiredService<ODataDeltaResourceSetDeserializer>();
                    }

                    IEdmTypeReference elementType = edmType.AsCollection().ElementType();
                    if (elementType.IsUntyped())
                    {
                        if (typeof(IEdmStructuredType).IsAssignableFrom(elementType.Definition.GetType()))
                        {
                            return _serviceProvider.GetRequiredService<ODataResourceSetDeserializer>();
                        }

                        return _serviceProvider.GetRequiredService<ODataCollectionDeserializer>();
                    }

                    if (elementType.IsEntity() || elementType.IsComplex())
                    {
                        return _serviceProvider.GetRequiredService<ODataResourceSetDeserializer>();
                    }
                    else
                    {
                        return _serviceProvider.GetRequiredService<ODataCollectionDeserializer>();
                    }

                default:
                    return null;
            }
        }

        /// <inheritdoc />
        public virtual IODataDeserializer GetODataDeserializer(Type type, HttpRequest request)
        {
            if (type == null)
            {
                throw Error.ArgumentNull(nameof(type));
            }

            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            if (type == typeof(Uri))
            {
                return _serviceProvider.GetRequiredService<ODataEntityReferenceLinkDeserializer>();
            }

            if (type == typeof(ODataActionParameters) || type == typeof(ODataUntypedActionParameters))
            {
                return _serviceProvider.GetRequiredService<ODataActionPayloadDeserializer>();
            }

            if (IsDelta(type))
            {
                return _serviceProvider.GetRequiredService<ODataDeltaResourceSetDeserializer>();
            }

            IEdmModel model = request.GetModel();
            IEdmTypeReference edmType = model.GetEdmTypeReference(type);

            if (edmType == null)
            {
                return null;
            }
            else
            {
                return GetEdmTypeDeserializer(edmType);
            }
        }

        private static bool IsDelta(Type type)
        {
            if (type == typeof(EdmChangedObjectCollection) ||
                (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(DeltaSet<>)))
            {
                return true;
            }

            return false;
        }
    }
}
