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
public class ODataDeletedResourceSerializer : ODataResourceSerializer
{
    private const string Resource = "DeletedResource";

    /// <inheritdoc />
    public ODataDeletedResourceSerializer(IODataSerializerProvider serializerProvider)
        : base(serializerProvider)
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

        InitializeODataResource(selectExpandNode, resource, resourceContext);

        string etag = CreateETag(resourceContext);
        if (etag != null)
        {
            resource.ETag = etag;
        }

        // Try to add the dynamic properties if the structural type is open.
        AppendDynamicPropertiesInternal(resource, selectExpandNode, resourceContext);

        return resource;
    }
}