//-----------------------------------------------------------------------------
// <copyright file="ODataDeletedResourceSerializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
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

            await writer.WriteStartAsync(odataDeletedResource).ConfigureAwait(false);
            ODataResourceSerializer serializer = SerializerProvider.GetEdmTypeSerializer(expectedType) as ODataResourceSerializer;
            if (serializer == null)
            {
                throw new SerializationException(
                    Error.Format(SRResources.TypeCannotBeSerialized, expectedType.ToTraceString()));
            }
            await serializer.WriteResourceContent(writer, selectExpandNode, resourceContext, /*isDelta*/ true);
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
        return ODataResourceSerializer.CreateSelectExpandNodeInternal(resourceContext);
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
            Properties = ODataResourceSerializer.CreateStructuralPropertyBag(selectExpandNode, resourceContext, this.CreateStructuralProperty, this.CreateComputedProperty),
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
        ODataResourceSerializer.AppendDynamicPropertiesInternal(resource, selectExpandNode, resourceContext, SerializerProvider);
    }

    /// <summary>
    /// Creates the ETag for the given entity.
    /// </summary>
    /// <param name="resourceContext">The context for the resource instance being written.</param>
    /// <returns>The created ETag.</returns>
    public virtual string CreateETag(ResourceContext resourceContext)
    {
        return ODataResourceSerializer.CreateETagInternal(resourceContext);
    }

    /// <summary>
    /// Creates the <see cref="ODataProperty"/> to be written for the given resource.
    /// </summary>
    /// <param name="propertyName">The computed property being written.</param>
    /// <param name="resourceContext">The context for the resource instance being written.</param>
    /// <returns>The <see cref="ODataProperty"/> to write.</returns>
    public virtual ODataProperty CreateComputedProperty(string propertyName, ResourceContext resourceContext)
    {
        return ODataResourceSerializer.CreateComputedPropertyInternal(propertyName, resourceContext, SerializerProvider);
    }

    /// <summary>
    /// Creates the <see cref="ODataProperty"/> to be written for the given entity and the structural property.
    /// </summary>
    /// <param name="structuralProperty">The EDM structural property being written.</param>
    /// <param name="resourceContext">The context for the entity instance being written.</param>
    /// <returns>The <see cref="ODataProperty"/> to write.</returns>
    public virtual ODataProperty CreateStructuralProperty(IEdmStructuralProperty structuralProperty, ResourceContext resourceContext)
    {
        return ODataResourceSerializer.CreateStructuralPropertyInternal(structuralProperty, resourceContext, SerializerProvider);
    }
}