//-----------------------------------------------------------------------------
// <copyright file="ODataDeserializerContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Formatter.Deserialization
{
    /// <summary>
    /// This class encapsulates the state and settings that get passed to <see cref="IODataDeserializer"/>.
    /// </summary>
    public class ODataDeserializerContext
    {
        private bool? _isDeltaOfT;
        private bool? _isDeltaDeleted;
        private bool? _isNoClrType;

        /// <summary>
        /// Gets or sets the type of the top-level object the request needs to be deserialized into.
        /// </summary>
        public Type ResourceType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IEdmTypeReference"/> of the top-level object the request needs to be deserialized into.
        /// </summary>
        public IEdmTypeReference ResourceEdmType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ODataPath"/> of the request.
        /// </summary>
        public ODataPath Path { get; set; }

        /// <summary>
        /// Gets or sets the EDM model associated with the request.
        /// </summary>
        public IEdmModel Model { get; set; }

        /// <summary>
        /// Gets or sets the HTTP Request whose response is being serialized.
        /// </summary>
        public HttpRequest Request { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TimeZoneInfo"/>.
        /// </summary>
        public TimeZoneInfo TimeZone { get; set; }

        internal bool IsDeltaOfT
        {
            get
            {
                if (!_isDeltaOfT.HasValue)
                {
                    _isDeltaOfT = ResourceType != null && ResourceType.IsGenericType &&
                        ResourceType.GetGenericTypeDefinition() == typeof(Delta<>);
                }

                return _isDeltaOfT.Value;
            }
        }

        internal bool IsDeltaDeleted
        {
            get
            {
                if (_isDeltaDeleted == null)
                {
                    if (typeof(IEdmDeltaDeletedResourceObject).IsAssignableFrom(ResourceType))
                    {
                        _isDeltaDeleted = true;
                    }
                    else
                    {
                        _isDeltaDeleted = ResourceType != null &&
                            ResourceType.IsGenericType &&
                            ResourceType.GetGenericTypeDefinition() == typeof(DeltaDeletedResource<>);
                    }
                }

                return _isDeltaDeleted.Value;
            }
        }

        // TODO: need refactor this part.
        // We can't only use the resource type to identify there's no Clr Type or not.
        // We should use the model type mapping to identify there's no Clr Type or not.
        internal bool IsNoClrType
        {
            get
            {
                if (!_isNoClrType.HasValue)
                {
                    _isNoClrType = (TypeHelper.IsTypeAssignableFrom(typeof(IEdmObject), ResourceType) &&
                        (typeof(EdmUntypedObject) != ResourceType && typeof(EdmUntypedCollection) != ResourceType))
                        || typeof(ODataUntypedActionParameters) == ResourceType;
                }

                return _isNoClrType.Value;
            }
        }

        internal IEdmTypeReference GetEdmType(Type type)
        {
            if (ResourceEdmType != null)
            {
                return ResourceEdmType;
            }

            return EdmLibHelper.GetExpectedPayloadType(type, Path, Model);
        }

        internal ODataDeserializerContext CloneWithoutType()
        {
            return new ODataDeserializerContext
            {
                Path = this.Path,
                Model = this.Model,
                Request = this.Request,
                TimeZone = this.TimeZone
            };
        }

        private CachedItem _cached = new CachedItem();

        internal IODataInstanceAnnotationContainer GetContainer(object resource, IEdmStructuredType structuredType)
        {
            if (resource == null || structuredType == null)
            {
                return null;
            }

            // looking for cached first, we use the "reference equality"
            if (object.ReferenceEquals(_cached.Resource, resource))
            {
                return _cached.Container;
            }

            _cached.Resource = resource; // update the cache
            _cached.Container = null;
            PropertyInfo propertyInfo = Model.GetInstanceAnnotationsContainer(structuredType);
            if (propertyInfo == null)
            {
                return null;
            }

            object value;
            IDelta delta = resource as IDelta;
            if (delta != null)
            {
                delta.TryGetPropertyValue(propertyInfo.Name, out value);
            }
            else
            {
                value = propertyInfo.GetValue(resource);
            }

            IODataInstanceAnnotationContainer instanceAnnotationContainer = value as IODataInstanceAnnotationContainer;
            if (instanceAnnotationContainer == null)
            {
                try
                {
                    if (propertyInfo.PropertyType == typeof(ODataInstanceAnnotationContainer) || propertyInfo.PropertyType == typeof(IODataInstanceAnnotationContainer))
                    {
                        instanceAnnotationContainer = new ODataInstanceAnnotationContainer();
                    }
                    else
                    {
                        instanceAnnotationContainer = Activator.CreateInstance(propertyInfo.PropertyType) as IODataInstanceAnnotationContainer;
                    }

                    if (delta != null)
                    {
                        delta.TrySetPropertyValue(propertyInfo.Name, instanceAnnotationContainer);
                    }
                    else
                    {
                        propertyInfo.SetValue(resource, instanceAnnotationContainer);
                    }
                }
                catch (Exception ex)
                {
                    throw new ODataException(Error.Format(SRResources.CannotCreateInstanceForProperty, propertyInfo.Name), ex);
                }
            }

            _cached.Container = instanceAnnotationContainer;
            return instanceAnnotationContainer;
        }

        private struct CachedItem
        {
            public object Resource;
            public IODataInstanceAnnotationContainer Container;
        }
    }
}
