//-----------------------------------------------------------------------------
// <copyright file="ODataDeletedResourceSerializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.OData;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Deltas;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization;

/// <summary>
/// ODataSerializer for serializing instances of <see cref="IEdmDeltaDeletedResourceObject"/>/>
/// </summary>
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Relies on many ODataLib classes.")]
public class ODataDeletedResourceSerializer : ODataEdmTypeSerializer
{
    private const string Resource = "DeletedResource";

    /// <inheritdoc />
    public ODataDeletedResourceSerializer(IODataSerializerProvider serializerProvider)
        : base(ODataPayloadKind.Resource, serializerProvider)
    {
    }

    /// <inheritdoc />
    public override async Task WriteObjectAsync(object graph, Type type, ODataMessageWriter messageWriter,
        ODataSerializerContext writeContext)
    {
        if (messageWriter == null)
        {
            throw Error.ArgumentNull(nameof(messageWriter));
        }

        if (writeContext == null)
        {
            throw Error.ArgumentNull(nameof(writeContext));
        }

        bool isUntypedPath = writeContext.Path.IsUntypedPropertyPath();
        IEdmTypeReference edmType = writeContext.GetEdmType(graph, type, isUntypedPath);
        Contract.Assert(edmType != null);

        IEdmNavigationSource navigationSource = writeContext.NavigationSource;
        ODataWriter writer = await messageWriter.CreateODataResourceWriterAsync(navigationSource, edmType.ToStructuredType())
            .ConfigureAwait(false);
        await WriteObjectInlineAsync(graph, edmType, writer, writeContext).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task WriteObjectInlineAsync(object graph, IEdmTypeReference expectedType, ODataWriter writer,
        ODataSerializerContext writeContext)
    {
        if (writer == null)
        {
            throw Error.ArgumentNull(nameof(writer));
        }

        if (writeContext == null)
        {
            throw Error.ArgumentNull(nameof(writeContext));
        }

        if (graph == null || graph is NullEdmComplexObject)
        {
            throw new SerializationException(Error.Format(SRResources.CannotSerializerNull, Resource));
        }
        else
        {
            await WriteDeletedResourceAsync(graph, writer, writeContext, expectedType).ConfigureAwait(false);
        }
    }

    private async Task WriteDeletedResourceAsync(object graph, ODataWriter writer, ODataSerializerContext writeContext,
    IEdmTypeReference expectedType)
    {
        Contract.Assert(writeContext != null);

        //TODO: do we need this?
        //if (graph.GetType().IsDynamicTypeWrapper())
        //{
        //    await new ODataResourceSerializer(SerializerProvider).WriteDynamicTypeResourceAsync(graph, writer, expectedType, writeContext).ConfigureAwait(false);
        //    return;
        //}

        IEdmStructuredTypeReference structuredType = ODataResourceSerializer.GetResourceType(graph, writeContext);
        ResourceContext resourceContext = new ResourceContext(writeContext, structuredType, graph);

        SelectExpandNode selectExpandNode = CreateSelectExpandNode(resourceContext);
        if (selectExpandNode != null)
        {
            ODataDeletedResource odataDeletedResource;

            if (graph is EdmDeltaDeletedResourceObject edmDeltaDeletedEntity)
            {
                odataDeletedResource = CreateDeletedResource(edmDeltaDeletedEntity.Id, edmDeltaDeletedEntity.Reason ?? DeltaDeletedEntryReason.Deleted, selectExpandNode, resourceContext);
                if (edmDeltaDeletedEntity.NavigationSource != null)
                {
                    resourceContext.NavigationSource = edmDeltaDeletedEntity.NavigationSource;
                    ODataResourceSerializationInfo serializationInfo = new ODataResourceSerializationInfo
                    {
                        NavigationSourceName = edmDeltaDeletedEntity.NavigationSource.Name
                    };
                    odataDeletedResource.SetSerializationInfo(serializationInfo);
                }
            }
            else if (graph is IDeltaDeletedResource deltaDeletedResource)
            {
                odataDeletedResource = CreateDeletedResource(deltaDeletedResource.Id, deltaDeletedResource.Reason ?? DeltaDeletedEntryReason.Deleted, selectExpandNode, resourceContext);
            }
            else
            {
                throw new SerializationException(Error.Format(SRResources.CannotWriteType, GetType().Name, graph?.GetType().FullName));
            }

            bool isDelta = (graph is IDelta || graph is IEdmChangedObject);
            await writer.WriteStartAsync(odataDeletedResource).ConfigureAwait(false);
            await new ODataResourceSerializer(SerializerProvider).WriteResourceContent(writer, selectExpandNode, resourceContext, isDelta);
            await writer.WriteEndAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Creates the <see cref="SelectExpandNode"/> that describes the set of properties and actions to select and expand while writing this entity.
    /// </summary>
    /// <param name="resourceContext">Contains the entity instance being written and the context.</param>
    /// <returns>
    /// The <see cref="SelectExpandNode"/> that describes the set of properties and actions to select and expand while writing this entity.
    /// </returns>
    public virtual SelectExpandNode CreateSelectExpandNode(ResourceContext resourceContext)
    {
        if (resourceContext == null)
        {
            throw Error.ArgumentNull(nameof(resourceContext));
        }

        ODataSerializerContext writeContext = resourceContext.SerializerContext;
        IEdmStructuredType structuredType = resourceContext.StructuredType;

        object selectExpandNode;

        Tuple<SelectExpandClause, IEdmStructuredType> key = Tuple.Create(writeContext.SelectExpandClause, structuredType);
        if (!writeContext.Items.TryGetValue(key, out selectExpandNode))
        {
            // cache the selectExpandNode so that if we are writing a feed we don't have to construct it again.
            selectExpandNode = new SelectExpandNode(structuredType, writeContext);
            writeContext.Items[key] = selectExpandNode;
        }

        return selectExpandNode as SelectExpandNode;
    }

    /// <summary>
    /// Creates the <see cref="ODataResource"/> to be written while writing this resource.
    /// </summary>
    /// <param name="id">The id of the Deleted Resource to be written (may be null if properties contains all key properties)</param>
    /// <param name="reason">The <see cref="DeltaDeletedEntryReason"/> for the removal of the resource.</param>
    /// <param name="selectExpandNode">The <see cref="SelectExpandNode"/> describing the response graph.</param>
    /// <param name="resourceContext">The context for the resource instance being written.</param>
    /// <returns>The created <see cref="ODataResource"/>.</returns>
    public virtual ODataDeletedResource CreateDeletedResource(Uri id, DeltaDeletedEntryReason reason, SelectExpandNode selectExpandNode, ResourceContext resourceContext)
    {
        if (selectExpandNode == null)
        {
            throw Error.ArgumentNull(nameof(selectExpandNode));
        }

        if (resourceContext == null)
        {
            throw Error.ArgumentNull(nameof(resourceContext));
        }

        string typeName = resourceContext.StructuredType.FullTypeName();

        ODataDeletedResource resource = new ODataDeletedResource
        {
            Id = id ?? (resourceContext.NavigationSource == null ? null : resourceContext.GenerateSelfLink(false)),
            TypeName = typeName ?? "Edm.Untyped",
            Properties = CreateStructuralPropertyBag(selectExpandNode, resourceContext),
            Reason = reason
        };

        ODataResourceSerializer.InitializeODataResource(selectExpandNode, resource, resourceContext);

        string etag = CreateETag(resourceContext);
        if (etag != null)
        {
            resource.ETag = etag;
        }

        // Try to add the dynamic properties if the structural type is open.
        AppendDynamicProperties(resource, selectExpandNode, resourceContext);

        return resource;
    }

    /// <summary>
    /// Appends the dynamic properties of primitive, enum or the collection of them into the given <see cref="ODataResource"/>.
    /// If the dynamic property is a property of the complex or collection of complex, it will be saved into
    /// the dynamic complex properties dictionary of <paramref name="resourceContext"/> and be written later.
    /// </summary>
    /// <param name="resource">The <see cref="ODataDeletedResource"/> describing the resource.</param>
    /// <param name="selectExpandNode">The <see cref="SelectExpandNode"/> describing the response graph.</param>
    /// <param name="resourceContext">The context for the resource instance being written.</param>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Relies on many classes.")]
    [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These are simple conversion function and cannot be split up.")]
    public virtual void AppendDynamicProperties(ODataDeletedResource resource, SelectExpandNode selectExpandNode,
        ResourceContext resourceContext)
    {
        Contract.Assert(resource != null);
        Contract.Assert(selectExpandNode != null);
        Contract.Assert(resourceContext != null);

        ODataResourceSerializer.AppendDynamicPropertiesInternal(resource, selectExpandNode, resourceContext, SerializerProvider);
    }

    /// <summary>
    /// Creates the ETag for the given entity.
    /// </summary>
    /// <param name="resourceContext">The context for the resource instance being written.</param>
    /// <returns>The created ETag.</returns>
    public virtual string CreateETag(ResourceContext resourceContext)
    {
        if (resourceContext == null)
        {
            throw Error.ArgumentNull(nameof(resourceContext));
        }

        if (resourceContext.Request != null)
        {
            IEdmModel model = resourceContext.EdmModel;
            IEdmNavigationSource navigationSource = resourceContext.NavigationSource;

            IEnumerable<IEdmStructuralProperty> concurrencyProperties;
            if (model != null && navigationSource != null)
            {
                concurrencyProperties = model.GetConcurrencyProperties(navigationSource);
            }
            else
            {
                concurrencyProperties = Enumerable.Empty<IEdmStructuralProperty>();
            }

            IDictionary<string, object> properties = null;
            foreach (IEdmStructuralProperty etagProperty in concurrencyProperties)
            {
                properties ??= new SortedDictionary<string, object>();
                    
                properties.Add(etagProperty.Name, resourceContext.GetPropertyValue(etagProperty.Name));
            }

            if (properties != null)
            {
                return resourceContext.Request.CreateETag(properties, resourceContext.TimeZone);
            }
        }

        return null;
    }

    //TODO: call method in ODataResourceSerializer
    private IEnumerable<ODataProperty> CreateStructuralPropertyBag(SelectExpandNode selectExpandNode, ResourceContext resourceContext)
    {
        Contract.Assert(selectExpandNode != null);
        Contract.Assert(resourceContext != null);

        int propertiesCount = (selectExpandNode.SelectedStructuralProperties?.Count ?? 0) + (selectExpandNode.SelectedComputedProperties?.Count ?? 0);
        List<ODataProperty> properties = new List<ODataProperty>(propertiesCount);

        if (selectExpandNode.SelectedStructuralProperties != null)
        {
            IEnumerable<IEdmStructuralProperty> structuralProperties = selectExpandNode.SelectedStructuralProperties;

            if (null != resourceContext.EdmObject && resourceContext.EdmObject.IsDeltaResource())
            {
                IDelta deltaObject = null;
                if (resourceContext.EdmObject is TypedEdmEntityObject obj)
                {
                    deltaObject = obj.Instance as IDelta;
                }
                else
                {
                    deltaObject = resourceContext.EdmObject as IDelta;
                }

                if (deltaObject != null)
                {
                    IEnumerable<string> changedProperties = deltaObject.GetChangedPropertyNames();
                    structuralProperties = structuralProperties.Where(p => changedProperties.Contains(p.Name) || p.IsKey());
                }
            }

            foreach (IEdmStructuralProperty structuralProperty in structuralProperties)
            {
                if (structuralProperty.Type != null && structuralProperty.Type.IsStream())
                {
                    // skip the stream property, the stream property is written in its own logic
                    continue;
                }

                if (structuralProperty.Type != null &&
                    (structuralProperty.Type.IsUntyped() || structuralProperty.Type.IsCollectionUntyped()))
                {
                    // skip it here, we use a different method to write all 'declared' untyped properties
                    continue;
                }

                ODataProperty property = CreateStructuralProperty(structuralProperty, resourceContext);
                if (property != null)
                {
                    properties.Add(property);
                }
            }
        }

        // Try to add computed properties
        if (selectExpandNode.SelectedComputedProperties != null)
        {
            foreach (string propertyName in selectExpandNode.SelectedComputedProperties)
            {
                ODataProperty property = CreateComputedProperty(propertyName, resourceContext);
                if (property != null)
                {
                    properties.Add(property);
                }
            }
        }

        return properties;
    }

    /// <summary>
    /// Creates the <see cref="ODataProperty"/> to be written for the given resource.
    /// </summary>
    /// <param name="propertyName">The computed property being written.</param>
    /// <param name="resourceContext">The context for the resource instance being written.</param>
    /// <returns>The <see cref="ODataProperty"/> to write.</returns>
    public virtual ODataProperty CreateComputedProperty(string propertyName, ResourceContext resourceContext)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw Error.ArgumentNullOrEmpty(nameof(propertyName));
        }

        if (resourceContext == null)
        {
            throw Error.ArgumentNull(nameof(resourceContext));
        }

        // The computed value is from the Linq expression binding.
        object propertyValue = resourceContext.GetPropertyValue(propertyName);
        if (propertyValue == null)
        {
            return new ODataProperty { Name = propertyName, Value = null };
        }

        ODataSerializerContext writeContext = resourceContext.SerializerContext;

        IEdmTypeReference edmTypeReference = resourceContext.SerializerContext.GetEdmType(propertyValue, propertyValue.GetType());
        if (edmTypeReference == null)
        {
            throw Error.NotSupported(SRResources.TypeOfDynamicPropertyNotSupported, propertyValue.GetType().FullName, propertyName);
        }

        IODataEdmTypeSerializer serializer = SerializerProvider.GetEdmTypeSerializer(edmTypeReference);
        if (serializer == null)
        {
            throw new SerializationException(Error.Format(SRResources.TypeCannotBeSerialized, edmTypeReference.FullName()));
        }

        return serializer.CreateProperty(propertyValue, edmTypeReference, propertyName, writeContext);
    }

    /// <summary>
    /// Creates the <see cref="ODataProperty"/> to be written for the given entity and the structural property.
    /// </summary>
    /// <param name="structuralProperty">The EDM structural property being written.</param>
    /// <param name="resourceContext">The context for the entity instance being written.</param>
    /// <returns>The <see cref="ODataProperty"/> to write.</returns>
    public virtual ODataProperty CreateStructuralProperty(IEdmStructuralProperty structuralProperty, ResourceContext resourceContext)
    {
        if (structuralProperty == null)
        {
            throw Error.ArgumentNull(nameof(structuralProperty));
        }
        if (resourceContext == null)
        {
            throw Error.ArgumentNull(nameof(resourceContext));
        }

        ODataSerializerContext writeContext = resourceContext.SerializerContext;

        IODataEdmTypeSerializer serializer = SerializerProvider.GetEdmTypeSerializer(structuralProperty.Type);
        if (serializer == null)
        {
            throw new SerializationException(
                Error.Format(SRResources.TypeCannotBeSerialized, structuralProperty.Type.FullName()));
        }

        object propertyValue = resourceContext.GetPropertyValue(structuralProperty.Name);

        IEdmTypeReference propertyType = structuralProperty.Type;
        if (propertyValue != null)
        {
            if (!propertyType.IsPrimitive() && !propertyType.IsEnum())
            {
                IEdmTypeReference actualType = writeContext.GetEdmType(propertyValue, propertyValue.GetType());
                if (propertyType != null && propertyType != actualType)
                {
                    propertyType = actualType;
                }
            }
        }

        return serializer.CreateProperty(propertyValue, propertyType, structuralProperty.Name, writeContext);
    }
}