//-----------------------------------------------------------------------------
// <copyright file="ODataResourceDeserializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Formatter.Wrapper;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Formatter.Deserialization
{
    /// <summary>
    /// Represents an <see cref="ODataDeserializer"/> for reading OData resource payloads.
    /// </summary>
    public class ODataResourceDeserializer : ODataEdmTypeDeserializer
    {
        private static readonly Regex ContentIdReferencePattern = new Regex(@"\$\d", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataResourceDeserializer"/> class.
        /// </summary>
        /// <param name="deserializerProvider">The deserializer provider to use to read inner objects.</param>
        public ODataResourceDeserializer(IODataDeserializerProvider deserializerProvider)
            : base(ODataPayloadKind.Resource, deserializerProvider)
        {
        }

        /// <inheritdoc />
        public override async Task<object> ReadAsync(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
        {
            if (messageReader == null)
            {
                throw new ArgumentNullException(nameof(messageReader));
            }

            if (readContext == null)
            {
                throw new ArgumentNullException(nameof(readContext));
            }

            IEdmTypeReference edmType = readContext.GetEdmType(type);
            Contract.Assert(edmType != null);

            if (!edmType.IsStructured())
            {
                throw Error.Argument("type", SRResources.ArgumentMustBeOfType, "Structured");
            }

            IEdmStructuredTypeReference structuredType = edmType.AsStructured();

            IEdmNavigationSource navigationSource = null;
            if (structuredType.IsEntity())
            {
                if (readContext.Path == null)
                {
                    throw Error.Argument("readContext", SRResources.ODataPathMissing);
                }

                navigationSource = readContext.Path.GetNavigationSource();
                if (navigationSource == null)
                {
                    throw new SerializationException(SRResources.NavigationSourceMissingDuringDeserialization);
                }
            }

            ODataReader odataReader = await messageReader
                .CreateODataResourceReaderAsync(navigationSource, structuredType.StructuredDefinition()).ConfigureAwait(false);
            ODataResourceWrapper topLevelResource = await odataReader.ReadResourceOrResourceSetAsync().ConfigureAwait(false)
                as ODataResourceWrapper;
            Contract.Assert(topLevelResource != null);

            return ReadInline(topLevelResource, structuredType, readContext);
        }

        /// <inheritdoc />
        public sealed override object ReadInline(object item, IEdmTypeReference edmType, ODataDeserializerContext readContext)
        {
            if (edmType == null)
            {
                throw new ArgumentNullException(nameof(edmType));
            }

            if (edmType.IsComplex() && item == null)
            {
                return null;
            }

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (!edmType.IsStructured())
            {
                throw Error.Argument("edmType", SRResources.ArgumentMustBeOfType, "Entity or Complex");
            }

            ODataResourceWrapper resourceWrapper = item as ODataResourceWrapper;
            if (resourceWrapper == null || !(resourceWrapper.Item is ODataResourceWrapper))
            {
                throw Error.Argument("item", SRResources.ArgumentMustBeOfType, typeof(ODataResource).Name);
            }

            // Recursion guard to avoid stack overflows
            RuntimeHelpers.EnsureSufficientExecutionStack();

            resourceWrapper = UpdateResourceWrapper(resourceWrapper, readContext);

            return ReadResource(resourceWrapper, edmType.AsStructured(), readContext);
        }

        /// <summary>
        /// Deserializes the given <paramref name="resourceWrapper"/> under the given <paramref name="readContext"/>.
        /// </summary>
        /// <param name="resourceWrapper">The OData resource to deserialize.</param>
        /// <param name="structuredType">The type of the resource to deserialize.</param>
        /// <param name="readContext">The deserializer context.</param>
        /// <returns>The deserialized resource.</returns>
        public virtual object ReadResource(ODataResourceWrapper resourceWrapper, IEdmStructuredTypeReference structuredType,
            ODataDeserializerContext readContext)
        {
            if (resourceWrapper == null)
            {
                throw new ArgumentNullException(nameof(resourceWrapper));
            }

            if (readContext == null)
            {
                throw new ArgumentNullException(nameof(readContext));
            }

            if (!String.IsNullOrEmpty(resourceWrapper.Resource.TypeName) &&
                structuredType.FullName() != resourceWrapper.Resource.TypeName &&
                resourceWrapper.Resource.TypeName != "Edm.Untyped")
            {
                // received a derived type in a base type deserializer. delegate it to the appropriate derived type deserializer.
                IEdmModel model = readContext.Model;

                if (model == null)
                {
                    throw Error.Argument("readContext", SRResources.ModelMissingFromReadContext);
                }

                IEdmStructuredType actualType = model.FindType(resourceWrapper.Resource.TypeName) as IEdmStructuredType;
                if (actualType == null)
                {
                    throw new ODataException(Error.Format(SRResources.ResourceTypeNotInModel, resourceWrapper.Resource.TypeName));
                }

                if (actualType.IsAbstract)
                {
                    string message = Error.Format(SRResources.CannotInstantiateAbstractResourceType, resourceWrapper.Resource.TypeName);
                    throw new ODataException(message);
                }

                IEdmTypeReference actualStructuredType;
                IEdmEntityType actualEntityType = actualType as IEdmEntityType;
                if (actualEntityType != null)
                {
                    actualStructuredType = new EdmEntityTypeReference(actualEntityType, isNullable: false);
                }
                else
                {
                    actualStructuredType = new EdmComplexTypeReference(actualType as IEdmComplexType, isNullable: false);
                }

                IODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(actualStructuredType);
                if (deserializer == null)
                {
                    throw new SerializationException(
                        Error.Format(SRResources.TypeCannotBeDeserialized, actualEntityType.FullName()));
                }

                object resource = deserializer.ReadInline(resourceWrapper, actualStructuredType, readContext);

                EdmStructuredObject structuredObject = resource as EdmStructuredObject;
                if (structuredObject != null)
                {
                    structuredObject.ExpectedEdmType = structuredType.StructuredDefinition();
                }

                return resource;
            }
            else
            {
                object resource = CreateResourceInstance(structuredType, readContext);
                ApplyResourceProperties(resource, resourceWrapper, structuredType, readContext);
                ApplyDeletedResource(resource, resourceWrapper, readContext);
                return resource;
            }
        }

        /// <summary>
        /// Creates a new instance of the backing CLR object for the given resource type.
        /// </summary>
        /// <param name="structuredType">The EDM type of the resource to create.</param>
        /// <param name="readContext">The deserializer context.</param>
        /// <returns>The created CLR object.</returns>
        public virtual object CreateResourceInstance(IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
        {
            if (structuredType == null)
            {
                throw Error.ArgumentNull(nameof(structuredType));
            }

            if (readContext == null)
            {
                throw Error.ArgumentNull(nameof(readContext));
            }

            IEdmModel model = readContext.Model;
            if (model == null)
            {
                throw Error.Argument("readContext", SRResources.ModelMissingFromReadContext);
            }

            if (readContext.IsNoClrType)
            {
                if (structuredType.IsEntity())
                {
                    if (readContext.IsDeltaDeleted)
                    {
                        return new EdmDeltaDeletedResourceObject(structuredType.AsEntity());
                    }
                    else
                    {
                        return new EdmEntityObject(structuredType.AsEntity());
                    }
                }

                return new EdmComplexObject(structuredType.AsComplex());
            }
            else
            {
                Type clrType = model.GetClrType(structuredType);
                if (clrType == null)
                {
                    throw new ODataException(
                        Error.Format(SRResources.MappingDoesNotContainResourceType, structuredType.FullName()));
                }

                if (readContext.IsDeltaOfT || readContext.IsDeltaDeleted)
                {
                    IEnumerable<string> updatablePoperties = model.GetAllProperties(structuredType.StructuredDefinition());

                    if (structuredType.IsOpen())
                    {
                        PropertyInfo dynamicDictionaryPropertyInfo = model.GetDynamicPropertyDictionary(
                            structuredType.StructuredDefinition());

                        return Activator.CreateInstance(readContext.ResourceType, clrType, updatablePoperties,
                            dynamicDictionaryPropertyInfo);
                    }
                    else
                    {
                        return Activator.CreateInstance(readContext.ResourceType, clrType, updatablePoperties);
                    }
                }
                else
                {
                    return Activator.CreateInstance(clrType);
                }
            }
        }

        /// <summary>
        /// Deserializes the delete information from <paramref name="resourceWrapper"/> into <paramref name="resource"/>.
        /// </summary>
        /// <param name="resource">The object into which the nested properties should be read.</param>
        /// <param name="resourceWrapper">The resource object containing the nested properties.</param>
        /// <param name="readContext">The deserializer context.</param>
        public virtual void ApplyDeletedResource(object resource, ODataResourceWrapper resourceWrapper, ODataDeserializerContext readContext)
        {
            if (resourceWrapper == null)
            {
                throw Error.ArgumentNull(nameof(resourceWrapper));
            }

            if (readContext == null)
            {
                throw Error.ArgumentNull(nameof(readContext));
            }

            if (!resourceWrapper.IsDeletedResource)
            {
                return;
            }

            if (resourceWrapper.Resource is ODataDeletedResource deletedResource)
            {
                if (resource is IDeltaDeletedResource deltaDeletedResource)
                {
                    // typed scenario
                    deltaDeletedResource.Id = deletedResource.Id;
                    deltaDeletedResource.Reason = deletedResource.Reason;
                }
                else if (resource is IEdmDeltaDeletedResourceObject deletedObject)
                {
                    // non-typed scenario
                    deletedObject.Id = deletedResource.Id;
                    deletedObject.Reason = deletedResource.Reason ?? DeltaDeletedEntryReason.Deleted;
                }
            }
        }

        /// <summary>
        /// Deserializes the nested properties from <paramref name="resourceWrapper"/> into <paramref name="resource"/>.
        /// </summary>
        /// <param name="resource">The object into which the nested properties should be read.</param>
        /// <param name="resourceWrapper">The resource object containing the nested properties.</param>
        /// <param name="structuredType">The type of the resource.</param>
        /// <param name="readContext">The deserializer context.</param>
        public virtual void ApplyNestedProperties(object resource, ODataResourceWrapper resourceWrapper,
            IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
        {
            if (resourceWrapper == null)
            {
                throw Error.ArgumentNull(nameof(resourceWrapper));
            }

            foreach (ODataNestedResourceInfoWrapper nestedResourceInfo in resourceWrapper.NestedResourceInfos)
            {
                ApplyNestedProperty(resource, nestedResourceInfo, structuredType, readContext);
            }
        }

        /// <summary>
        /// Deserializes the nested property from <paramref name="resourceInfoWrapper"/> into <paramref name="resource"/>.
        /// </summary>
        /// <param name="resource">The object into which the nested property should be read.</param>
        /// <param name="resourceInfoWrapper">The nested resource info.</param>
        /// <param name="structuredType">The type of the resource.</param>
        /// <param name="readContext">The deserializer context.</param>
        public virtual void ApplyNestedProperty(object resource, ODataNestedResourceInfoWrapper resourceInfoWrapper,
             IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
        {
            if (resource == null)
            {
                throw Error.ArgumentNull(nameof(resource));
            }

            if (resourceInfoWrapper == null)
            {
                throw Error.ArgumentNull(nameof(resourceInfoWrapper));
            }

            // ODL has "FindProperty" method to find property using property name case-sensitive.
            // We use "ResolveProperty" method to support case-insensitive
            IEdmProperty edmProperty = structuredType.StructuredDefinition().ResolveProperty(resourceInfoWrapper.NestedResourceInfo.Name);
            if (edmProperty == null)
            {
                if (!structuredType.IsOpen())
                {
                    throw new ODataException(
                        Error.Format(SRResources.NestedPropertyNotfound, resourceInfoWrapper.NestedResourceInfo.Name,
                            structuredType.FullName()));
                }
            }

            IList<ODataItemWrapper> nestedItems;
            ODataEntityReferenceLinkWrapper[] referenceLinks = resourceInfoWrapper.NestedItems.OfType<ODataEntityReferenceLinkWrapper>().ToArray();
            if (referenceLinks.Length > 0)
            {
                // Be noted:
                // 1) OData v4.0, it's "Orders@odata.bind", and we get "ODataEntityReferenceLinkWrapper"(s) for that.
                // 2) OData v4.01, it's {"odata.id" ...}, and we get "ODataResource"(s) for that.
                // So, in OData v4, if it's a single, NestedItems contains one ODataEntityReferenceLinkWrapper,
                // if it's a collection, NestedItems contains multiple ODataEntityReferenceLinkWrapper(s)
                // We can use the following codes to adjust the `ODataEntityReferenceLinkWrapper` to `ODataResourceWrapper`.
                // In OData v4.01, we will not be here.
                // Only supports declared property
                Contract.Assert(edmProperty != null);

                nestedItems = new List<ODataItemWrapper>();
                if (edmProperty.Type.IsCollection())
                {
                    IEdmCollectionTypeReference edmCollectionTypeReference = edmProperty.Type.AsCollection();
                    ODataResourceSetWrapper resourceSetWrapper = CreateResourceSetWrapper(edmCollectionTypeReference, referenceLinks, readContext);
                    nestedItems.Add(resourceSetWrapper);
                }
                else
                {
                    ODataResourceWrapper resourceWrapper = CreateResourceWrapper(edmProperty.Type, referenceLinks[0], readContext);
                    nestedItems.Add(resourceWrapper);
                }
            }
            else
            {
                nestedItems = resourceInfoWrapper.NestedItems;
            }

            foreach (ODataItemWrapper childItem in nestedItems)
            {
                // it maybe null.
                if (childItem == null)
                {
                    if (edmProperty == null)
                    {
                        // for the dynamic, OData.net has a bug. see https://github.com/OData/odata.net/issues/977
                        ApplyDynamicResourceInNestedProperty(resourceInfoWrapper.NestedResourceInfo.Name, resource,
                            structuredType, null, readContext);
                    }
                    else
                    {
                        ApplyResourceInNestedProperty(edmProperty, resource, null, readContext);
                    }
                }

                ODataResourceSetWrapper resourceSetWrapper = childItem as ODataResourceSetWrapper;
                if (resourceSetWrapper != null)
                {
                    if (edmProperty == null)
                    {
                        ApplyDynamicResourceSetInNestedProperty(resourceInfoWrapper.NestedResourceInfo.Name,
                            resource, structuredType, resourceSetWrapper, readContext);
                    }
                    else
                    {
                        ApplyResourceSetInNestedProperty(edmProperty, resource, resourceSetWrapper, readContext);
                    }

                    continue;
                }

                if (childItem is ODataDeltaResourceSetWrapper deltaResourceSetWrapper)
                {
                    Contract.Assert(edmProperty != null, "nested delta resource cannot be dynamic property!");
                    ApplyNestedDeltaResourceSet(edmProperty, resource, deltaResourceSetWrapper, readContext);
                    continue;
                }

                // It must be resource by now.
                ODataResourceWrapper resourceWrapper = (ODataResourceWrapper)childItem;
                if (resourceWrapper != null)
                {
                    if (edmProperty == null)
                    {
                        ApplyDynamicResourceInNestedProperty(resourceInfoWrapper.NestedResourceInfo.Name, resource,
                            structuredType, resourceWrapper, readContext);
                    }
                    else
                    {
                        ApplyResourceInNestedProperty(edmProperty, resource, resourceWrapper, readContext);
                    }
                }
            }
        }

        /// <summary>
        /// Deserializes the structural properties from <paramref name="resourceWrapper"/> into <paramref name="resource"/>.
        /// </summary>
        /// <param name="resource">The object into which the structural properties should be read.</param>
        /// <param name="resourceWrapper">The resource object containing the structural properties.</param>
        /// <param name="structuredType">The type of the resource.</param>
        /// <param name="readContext">The deserializer context.</param>
        public virtual void ApplyStructuralProperties(object resource, ODataResourceWrapper resourceWrapper,
            IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
        {
            if (resourceWrapper == null)
            {
                throw new ArgumentNullException(nameof(resourceWrapper));
            }

            foreach (ODataProperty property in resourceWrapper.Resource.Properties)
            {
                ApplyStructuralProperty(resource, property, structuredType, readContext);
            }
        }

        /// <summary>
        /// Deserializes the given <paramref name="structuralProperty"/> into <paramref name="resource"/>.
        /// </summary>
        /// <param name="resource">The object into which the structural property should be read.</param>
        /// <param name="structuralProperty">The structural property.</param>
        /// <param name="structuredType">The type of the resource.</param>
        /// <param name="readContext">The deserializer context.</param>
        public virtual void ApplyStructuralProperty(object resource, ODataProperty structuralProperty,
            IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            if (structuralProperty == null)
            {
                throw new ArgumentNullException(nameof(structuralProperty));
            }

            if (structuredType == null)
            {
                throw new ArgumentNullException(nameof(structuredType));
            }

            if (readContext == null)
            {
                throw new ArgumentNullException(nameof(readContext));
            }

            DeserializationHelpers.ApplyProperty(structuralProperty, structuredType, resource, DeserializerProvider, readContext);
        }

        private void ApplyResourceProperties(object resource, ODataResourceWrapper resourceWrapper,
            IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
        {
            ApplyStructuralProperties(resource, resourceWrapper, structuredType, readContext);
            ApplyNestedProperties(resource, resourceWrapper, structuredType, readContext);
        }

        private void ApplyResourceInNestedProperty(IEdmProperty nestedProperty, object resource,
            ODataResourceWrapper resourceWrapper, ODataDeserializerContext readContext)
        {
            Contract.Assert(nestedProperty != null);
            Contract.Assert(resource != null);
            Contract.Assert(readContext != null);

            object value = ReadNestedResourceInline(resourceWrapper, nestedProperty.Type, readContext);

            // First resolve Data member alias or annotation, then set the regular
            // or delta resource accordingly.
            string propertyName = readContext.Model.GetClrPropertyName(nestedProperty);

            DeserializationHelpers.SetProperty(resource, propertyName, value);
        }

        private void ApplyDynamicResourceInNestedProperty(string propertyName, object resource, IEdmStructuredTypeReference resourceStructuredType,
            ODataResourceWrapper resourceWrapper, ODataDeserializerContext readContext)
        {
            Contract.Assert(resource != null);
            Contract.Assert(readContext != null);

            object value = null;
            if (resourceWrapper != null)
            {
                IEdmSchemaType elementType = readContext.Model.FindDeclaredType(resourceWrapper.Resource.TypeName);
                IEdmTypeReference edmTypeReference = elementType.ToEdmTypeReference(true);

                value = ReadNestedResourceInline(resourceWrapper, edmTypeReference, readContext);
            }

            DeserializationHelpers.SetDynamicProperty(resource, propertyName, value,
                resourceStructuredType.StructuredDefinition(), readContext.Model);
        }

        private object ReadNestedResourceInline(ODataResourceWrapper resourceWrapper, IEdmTypeReference edmType, ODataDeserializerContext readContext)
        {
            Contract.Assert(edmType != null);
            Contract.Assert(readContext != null);

            if (resourceWrapper == null)
            {
                return null;
            }

            IODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(edmType);
            if (deserializer == null)
            {
                throw new SerializationException(Error.Format(SRResources.TypeCannotBeDeserialized, edmType.FullName()));
            }

            IEdmStructuredTypeReference structuredType = edmType.AsStructured();

            ODataDeserializerContext nestedReadContext = new ODataDeserializerContext
            {
                Path = readContext.Path,
                Model = readContext.Model,
                Request = readContext.Request,
                TimeZone = readContext.TimeZone
            };

            Type clrType;
            if (readContext.IsNoClrType)
            {
                clrType = structuredType.IsEntity()
                    ? typeof(EdmEntityObject)
                    : typeof(EdmComplexObject);
            }
            else
            {
                clrType = readContext.Model.GetClrType(structuredType);

                if (clrType == null)
                {
                    throw new ODataException(
                        Error.Format(SRResources.MappingDoesNotContainResourceType, structuredType.FullName()));
                }
            }

            nestedReadContext.ResourceType = readContext.IsDeltaOfT
                ? typeof(Delta<>).MakeGenericType(clrType)
                : clrType;
            return deserializer.ReadInline(resourceWrapper, edmType, nestedReadContext);
        }

        private void ApplyResourceSetInNestedProperty(IEdmProperty nestedProperty, object resource,
            ODataResourceSetWrapper resourceSetWrapper, ODataDeserializerContext readContext)
        {
            Contract.Assert(nestedProperty != null);
            Contract.Assert(resource != null);
            Contract.Assert(readContext != null);

            object value = ReadNestedResourceSetInline(resourceSetWrapper, nestedProperty.Type, readContext);

            string propertyName = readContext.Model.GetClrPropertyName(nestedProperty);
            DeserializationHelpers.SetCollectionProperty(resource, nestedProperty, value, propertyName);
        }

        private void ApplyDynamicResourceSetInNestedProperty(string propertyName, object resource, IEdmStructuredTypeReference structuredType,
            ODataResourceSetWrapper resourceSetWrapper, ODataDeserializerContext readContext)
        {
            Contract.Assert(resource != null);
            Contract.Assert(readContext != null);

            if (string.IsNullOrEmpty(resourceSetWrapper.ResourceSet.TypeName))
            {
                string message = Error.Format(SRResources.DynamicResourceSetTypeNameIsRequired, propertyName);
                throw new ODataException(message);
            }

            string elementTypeName =
                DeserializationHelpers.GetCollectionElementTypeName(resourceSetWrapper.ResourceSet.TypeName,
                    isNested: false);
            IEdmSchemaType elementType = readContext.Model.FindDeclaredType(elementTypeName);

            IEdmTypeReference edmTypeReference = elementType.ToEdmTypeReference(true);
            EdmCollectionTypeReference collectionType = new EdmCollectionTypeReference(new EdmCollectionType(edmTypeReference));

            IODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(collectionType);
            if (deserializer == null)
            {
                throw new SerializationException(Error.Format(SRResources.TypeCannotBeDeserialized, collectionType.FullName()));
            }

            IEnumerable value = ReadNestedResourceSetInline(resourceSetWrapper, collectionType, readContext) as IEnumerable;
            object result = value;
            if (value != null)
            {
                if (readContext.IsNoClrType)
                {
                    result = value.ConvertToEdmObject(collectionType);
                }
            }

            DeserializationHelpers.SetDynamicProperty(resource, structuredType, EdmTypeKind.Collection, propertyName,
                result, collectionType, readContext.Model);
        }

        private object ReadNestedResourceSetInline(ODataResourceSetWrapper resourceSetWrapper, IEdmTypeReference edmType,
            ODataDeserializerContext readContext)
        {
            Contract.Assert(resourceSetWrapper != null);
            Contract.Assert(edmType != null);
            Contract.Assert(readContext != null);

            IODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(edmType);
            if (deserializer == null)
            {
                throw new SerializationException(Error.Format(SRResources.TypeCannotBeDeserialized, edmType.FullName()));
            }

            IEdmStructuredTypeReference structuredType = edmType.AsCollection().ElementType().AsStructured();
            ODataDeserializerContext nestedReadContext = readContext.CloneWithoutType();

            if (readContext.IsNoClrType)
            {
                if (structuredType.IsEntity())
                {
                    nestedReadContext.ResourceType = typeof(EdmEntityObjectCollection);
                }
                else
                {
                    nestedReadContext.ResourceType = typeof(EdmComplexObjectCollection);
                }
            }
            else
            {
                Type clrType = readContext.Model.GetClrType(structuredType);

                if (clrType == null)
                {
                    throw new ODataException(
                        Error.Format(SRResources.MappingDoesNotContainResourceType, structuredType.FullName()));
                }

                nestedReadContext.ResourceType = typeof(List<>).MakeGenericType(clrType);
            }

            return deserializer.ReadInline(resourceSetWrapper, edmType, nestedReadContext);
        }

        private void ApplyNestedDeltaResourceSet(IEdmProperty nestedProperty, object resource,
            ODataDeltaResourceSetWrapper deltaResourceSetWrapper, ODataDeserializerContext readContext)
        {
            Contract.Assert(nestedProperty != null);
            Contract.Assert(resource != null);
            Contract.Assert(readContext != null);

            IEdmTypeReference edmType = nestedProperty.Type;
            IODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(edmType, true);
            if (deserializer == null)
            {
                throw new SerializationException(Error.Format(SRResources.TypeCannotBeDeserialized, edmType.FullName()));
            }

            IEdmStructuredTypeReference structuredType = edmType.AsCollection().ElementType().AsStructured();
            ODataDeserializerContext nestedReadContext = readContext.CloneWithoutType();

            if (readContext.IsNoClrType)
            {
                nestedReadContext.ResourceType = typeof(EdmChangedObjectCollection);
            }
            else
            {
                Type clrType = readContext.Model.GetClrType(structuredType);

                if (clrType == null)
                {
                    throw new ODataException(
                        Error.Format(SRResources.MappingDoesNotContainResourceType, structuredType.FullName()));
                }

                nestedReadContext.ResourceType = typeof(DeltaSet<>).MakeGenericType(clrType);
            }

            object value = deserializer.ReadInline(deltaResourceSetWrapper, edmType, nestedReadContext);

            string propertyName = readContext.Model.GetClrPropertyName(nestedProperty);
            DeserializationHelpers.SetCollectionProperty(resource, nestedProperty, value, propertyName);
        }

        /// <summary>
        /// Create <see cref="ODataResourceSetWrapper"/> from a set of <see cref="ODataEntityReferenceLinkWrapper"/>
        /// </summary>
        /// <param name="edmPropertyType">The Edm property type.</param>
        /// <param name="refLinks">The reference links.</param>
        /// <param name="readContext">The reader context.</param>
        /// <returns>The created <see cref="ODataResourceSetWrapper"/>.</returns>
        private static ODataResourceSetWrapper CreateResourceSetWrapper(IEdmCollectionTypeReference edmPropertyType,
            ODataEntityReferenceLinkWrapper[] refLinks, ODataDeserializerContext readContext)
        {
            Contract.Assert(edmPropertyType != null);
            Contract.Assert(refLinks != null);
            Contract.Assert(readContext != null);

            ODataResourceSet resourceSet = new ODataResourceSet
            {
                TypeName = edmPropertyType.FullName(),
            };

            IEdmTypeReference elementType = edmPropertyType.ElementType();
            ODataResourceSetWrapper resourceSetWrapper = new ODataResourceSetWrapper(resourceSet);
            foreach (ODataEntityReferenceLinkWrapper refLinkWrapper in refLinks)
            {
                ODataResourceWrapper resourceWrapper = CreateResourceWrapper(elementType, refLinkWrapper, readContext);
                resourceSetWrapper.Resources.Add(resourceWrapper);
            }

            return resourceSetWrapper;
        }

        /// <summary>
        /// Create <see cref="ODataResourceWrapper"/> from a <see cref="ODataEntityReferenceLinkWrapper"/>
        /// </summary>
        /// <param name="edmPropertyType">The Edm property type.</param>
        /// <param name="refLink">The reference link.</param>
        /// <param name="readContext">The reader context.</param>
        /// <returns>The created <see cref="ODataResourceSetWrapper"/>.</returns>
        private static ODataResourceWrapper CreateResourceWrapper(IEdmTypeReference edmPropertyType, ODataEntityReferenceLinkWrapper refLink, ODataDeserializerContext readContext)
        {
            Contract.Assert(edmPropertyType != null);
            Contract.Assert(refLink != null);
            Contract.Assert(readContext != null);

            ODataResource resource = new ODataResource
            {
                TypeName = edmPropertyType.FullName(),
            };

            resource.Properties = CreateKeyProperties(refLink.EntityReferenceLink.Url, readContext) ?? Array.Empty<ODataProperty>();
            ODataResourceWrapper resourceWrapper = new ODataResourceWrapper(resource);

            foreach (ODataInstanceAnnotation instanceAnnotation in refLink.EntityReferenceLink.InstanceAnnotations)
            {
                resource.InstanceAnnotations.Add(instanceAnnotation);
            }

            return resourceWrapper;
        }

        /// <summary>
        /// Update the resource wrapper if it's have the "Id" value.
        /// </summary>
        /// <param name="resourceWrapper">The resource wrapper.</param>
        /// <param name="readContext">The read context.</param>
        /// <returns>The resource wrapper.</returns>
        private ODataResourceWrapper UpdateResourceWrapper(ODataResourceWrapper resourceWrapper, ODataDeserializerContext readContext)
        {
            Contract.Assert(readContext != null);

            if (resourceWrapper == null ||
                resourceWrapper.Resource == null ||
                resourceWrapper.Resource.Id == null)
            {
                return resourceWrapper;
            }

            IEnumerable<ODataProperty> keys = CreateKeyProperties(resourceWrapper.Resource.Id, readContext);
            if (keys == null)
            {
                return resourceWrapper;
            }

            if (resourceWrapper.Resource.Properties == null)
            {
                resourceWrapper.Resource.Properties = keys;
            }
            else
            {
                IDictionary<string, ODataProperty> newPropertiesDic = resourceWrapper.Resource.Properties.ToDictionary(p => p.Name, p => p);
                foreach (ODataProperty key in keys)
                {
                    // Logic: if we have the key property, try to keep the key property and get rid of the key value from ID.
                    // Need to double confirm whether it is the right logic?
                    if (!newPropertiesDic.ContainsKey(key.Name))
                    {
                        newPropertiesDic[key.Name] = key;
                    }
                }

                resourceWrapper.Resource.Properties = newPropertiesDic.Values;
            }

            return resourceWrapper;
        }

        /// <summary>
        /// Do uri parsing to get the key values.
        /// </summary>
        /// <param name="id">The key Id.</param>
        /// <param name="readContext">The reader context.</param>
        /// <returns>The key properties.</returns>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private static IList<ODataProperty> CreateKeyProperties(Uri id, ODataDeserializerContext readContext)
        {
            Contract.Assert(id != null);
            Contract.Assert(readContext != null);

            if (readContext.Request == null)
            {
                return null;
            }

            try
            {
                IEdmModel model = readContext.Model;
                HttpRequest request = readContext.Request;
                IServiceProvider requestContainer = request.GetRouteServices();
                Uri resolvedId = id;

                string idOriginalString = id.OriginalString;
                if (ContentIdReferencePattern.IsMatch(idOriginalString))
                {
                    // We can expect request.ODataBatchFeature() to not be null
                    string resolvedUri = ContentIdHelpers.ResolveContentId(
                        idOriginalString,
                        request.ODataBatchFeature().ContentIdMapping);
                    resolvedId = new Uri(resolvedUri, UriKind.RelativeOrAbsolute);
                }

                Uri serviceRootUri = new Uri(request.CreateODataLink());
                IODataPathParser pathParser = requestContainer?.GetService<IODataPathParser>();
                if (pathParser == null) // Seems like IODataPathParser is NOT injected into DI container by default
                {
                    pathParser = new DefaultODataPathParser();
                }

                IList<ODataProperty> properties = null;
                ODataPath path = pathParser.Parse(model, serviceRootUri, resolvedId, requestContainer);
                KeySegment keySegment = path.OfType<KeySegment>().LastOrDefault();
                if (keySegment != null)
                {
                    properties = new List<ODataProperty>();
                    foreach (KeyValuePair<string, object> key in keySegment.Keys)
                    {
                        properties.Add(new ODataProperty
                        {
                            Name = key.Key,
                            Value = key.Value
                        });
                    }
                }

                return properties;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
