// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
    /// <summary>
    /// OData serializer for serializing a collection of <see cref="IEdmEntityType" />
    /// The Collection is of <see cref="IEdmChangedObject"/> which is the base interface implemented by all objects which are a part of the DeltaResourceSet payload.
    /// </summary>
    public class ODataDeltaResourceSetSerializer : ODataEdmTypeSerializer
    {
        private const string DeltaResourceSet = "DeltaResourceSet";

        /// <summary>
        /// Initializes a new instance of <see cref="ODataDeltaResourceSetSerializer"/>.
        /// </summary>
        /// <param name="serializerProvider">The <see cref="ODataSerializerProvider"/> to use to write nested entries.</param>
        public ODataDeltaResourceSetSerializer(ODataSerializerProvider serializerProvider)
            : base(ODataPayloadKind.Delta, serializerProvider)
        {
        }

        /// <inheritdoc />
        public override async Task WriteObjectAsync(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (messageWriter == null)
            {
                throw Error.ArgumentNull(nameof(messageWriter));
            }

            if (writeContext == null)
            {
                throw Error.ArgumentNull(nameof(writeContext));
            }

            if (graph == null)
            {
                throw new SerializationException(Error.Format(SRResources.CannotSerializerNull, DeltaResourceSet));
            }

            IEdmEntitySetBase entitySet = writeContext.NavigationSource as IEdmEntitySetBase;
            if (entitySet == null)
            {
                throw new SerializationException(SRResources.EntitySetMissingDuringSerialization);
            }

            IEdmTypeReference feedType = writeContext.GetEdmType(graph, type);
            Contract.Assert(feedType != null);

            IEdmEntityTypeReference entityType = GetResourceType(feedType).AsEntity();
            ODataWriter writer = await messageWriter.CreateODataDeltaResourceSetWriterAsync(entitySet, entityType.EntityDefinition())
                .ConfigureAwait(false);

            await WriteObjectInlineAsync(graph, feedType, writer, writeContext).ConfigureAwait(false);
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

            if (expectedType == null)
            {
                throw Error.ArgumentNull(nameof(expectedType));
            }

            if (graph == null)
            {
                throw new SerializationException(Error.Format(SRResources.CannotSerializerNull, DeltaResourceSet));
            }

            IEnumerable enumerable = graph as IEnumerable; // Data to serialize
            if (enumerable == null)
            {
                throw new SerializationException(
                    Error.Format(SRResources.CannotWriteType, GetType().Name, graph.GetType().FullName));
            }

            await WriteDeltaResourceSetAsync(enumerable, expectedType, writer, writeContext).ConfigureAwait(false);
        }

        private async Task WriteDeltaResourceSetAsync(IEnumerable enumerable, IEdmTypeReference feedType, ODataWriter writer,
            ODataSerializerContext writeContext)
        {
            Contract.Assert(writer != null);
            Contract.Assert(writeContext != null);
            Contract.Assert(enumerable != null);
            Contract.Assert(feedType != null);

            IEdmStructuredTypeReference elementType = GetResourceType(feedType);

            if (elementType.IsComplex())
            {
                ODataResourceSet resourceSet = new ODataResourceSet()
                {
                    TypeName = feedType.FullName()
                };

                await writer.WriteStartAsync(resourceSet).ConfigureAwait(false);

                ODataResourceSerializer entrySerializer = SerializerProvider.GetEdmTypeSerializer(elementType) as ODataResourceSerializer;
                if (entrySerializer == null)
                {
                    throw new SerializationException(
                        Error.Format(SRResources.TypeCannotBeSerialized, elementType.FullName()));
                }

                foreach (object entry in enumerable)
                {
                    await entrySerializer.WriteDeltaObjectInlineAsync(entry, elementType, writer, writeContext).ConfigureAwait(false);
                }
            }
            else
            {
                ODataDeltaResourceSet deltaResourceSet = CreateODataDeltaResourceSet(enumerable, feedType.AsCollection(), writeContext);
                if (deltaResourceSet == null)
                {
                    throw new SerializationException(Error.Format(SRResources.CannotSerializerNull, DeltaResourceSet));
                }

                // save the next page link for later to support JSON odata.streaming.
                Func<object, Uri> nextLinkGenerator = GetNextLinkGenerator(deltaResourceSet, enumerable, writeContext);
                deltaResourceSet.NextPageLink = null;

                //Start writing of the Delta Feed
                await writer.WriteStartAsync(deltaResourceSet).ConfigureAwait(false);

                object lastResource = null;
                //Iterate over all the entries present and select the appropriate write method.
                //Write method creates ODataDeltaDeletedEntry / ODataDeltaDeletedLink / ODataDeltaLink or ODataEntry.
                foreach (object item in enumerable)
                {
                    if (item == null)
                    {
                        throw new SerializationException(SRResources.NullElementInCollection);
                    }

                    lastResource = item;
                    DeltaItemKind kind = GetDelteItemKind(item);
                    switch (kind)
                    {
                        case DeltaItemKind.DeletedResource:
                            await WriteDeltaDeletedResourceAsync(item, writer, writeContext).ConfigureAwait(false);
                            break;
                        case DeltaItemKind.DeltaDeletedLink:
                            await WriteDeltaDeletedLinkAsync(item, writer, writeContext).ConfigureAwait(false);
                            break;
                        case DeltaItemKind.DeltaLink:
                            await WriteDeltaLinkAsync(item, writer, writeContext).ConfigureAwait(false);
                            break;
                        case DeltaItemKind.Resource:
                            {
                                ODataResourceSerializer entrySerializer = SerializerProvider.GetEdmTypeSerializer(elementType) as ODataResourceSerializer;
                                if (entrySerializer == null)
                                {
                                    throw new SerializationException(
                                        Error.Format(SRResources.TypeCannotBeSerialized, elementType.FullName()));
                                }
                                await entrySerializer.WriteDeltaObjectInlineAsync(item, elementType, writer, writeContext)
                                    .ConfigureAwait(false);
                                break;
                            }
                        default:
                            break;
                    }
                }

                // Subtle and surprising behavior: If the NextPageLink property is set before calling WriteStart(feed),
                // the next page link will be written early in a manner not compatible with odata.streaming=true. Instead, if
                // the next page link is not set when calling WriteStart(feed) but is instead set later on that feed
                // object before calling WriteEnd(), the next page link will be written at the end, as required for
                // odata.streaming=true support.

                deltaResourceSet.NextPageLink = nextLinkGenerator(lastResource);
            }

            //End Writing of the Delta Feed
            await writer.WriteEndAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a function that takes in an object and generates nextlink uri.
        /// </summary>
        /// <param name="deltaResourceSet">The resource set describing a collection of structured objects.</param>
        /// <param name="enumerable">>The instance representing the resourceSet being written.</param>
        /// <param name="writeContext">The serializer context.</param>
        /// <returns>The function that generates the NextLink from an object.</returns>
        /// <returns></returns>
        internal static Func<object, Uri> GetNextLinkGenerator(ODataDeltaResourceSet deltaResourceSet, IEnumerable enumerable, ODataSerializerContext writeContext)
        {
            return ODataResourceSetSerializer.GetNextLinkGenerator(deltaResourceSet, enumerable, writeContext);
        }

        /// <summary>
        /// Create the <see cref="ODataDeltaResourceSet"/> to be written for the given feed instance.
        /// </summary>
        /// <param name="feedInstance">The instance representing the feed being written.</param>
        /// <param name="feedType">The EDM type of the feed being written.</param>
        /// <param name="writeContext">The serializer context.</param>
        /// <returns>The created <see cref="ODataDeltaResourceSet"/> object.</returns>
        public virtual ODataDeltaResourceSet CreateODataDeltaResourceSet(IEnumerable feedInstance, IEdmCollectionTypeReference feedType,
            ODataSerializerContext writeContext)
        {
            if (writeContext == null)
            {
                throw Error.ArgumentNull(nameof(writeContext));
            }

            ODataDeltaResourceSet feed = new ODataDeltaResourceSet();

            if (writeContext.ExpandedResource == null)
            {
                // If we have more OData format specific information apply it now, only if we are the root feed.
                PageResult odataFeedAnnotations = feedInstance as PageResult;
                if (odataFeedAnnotations != null)
                {
                    feed.Count = odataFeedAnnotations.Count;
                    feed.NextPageLink = odataFeedAnnotations.NextPageLink;
                }
                else if (writeContext.Request != null)
                {
                    IODataFeature odataFeature = writeContext.Request.ODataFeature();
                    feed.NextPageLink = odataFeature.NextLink;
                    feed.DeltaLink = odataFeature.DeltaLink;

                    long? countValue = odataFeature.TotalCount;
                    if (countValue.HasValue)
                    {
                        feed.Count = countValue.Value;
                    }
                }
            }
            return feed;
        }

        /// <summary>
        /// Writes the given deltaDeletedEntry specified by the parameter graph as a part of an existing OData message using the given
        /// messageWriter and the writeContext.
        /// </summary>
        /// <param name="value">The object to be written.</param>
        /// <param name="writer">The <see cref="ODataDeltaWriter" /> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        public virtual async Task WriteDeltaDeletedResourceAsync(object value, ODataWriter writer, ODataSerializerContext writeContext)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull(nameof(writer));
            }

            ODataDeletedResource odataDeletedResource;

            if (value is EdmDeltaDeletedResourceObject edmDeltaDeletedEntity)
            {
                odataDeletedResource = new ODataDeletedResource(edmDeltaDeletedEntity.Id, edmDeltaDeletedEntity.Reason ?? DeltaDeletedEntryReason.Deleted);

                if (edmDeltaDeletedEntity.NavigationSource != null)
                {
                    ODataResourceSerializationInfo serializationInfo = new ODataResourceSerializationInfo
                    {
                        NavigationSourceName = edmDeltaDeletedEntity.NavigationSource.Name
                    };
                    odataDeletedResource.SetSerializationInfo(serializationInfo);
                }
            }
            else if (value is IDeltaDeletedResource deltaDeletedResource)
            {
                odataDeletedResource = new ODataDeletedResource(deltaDeletedResource.Id, deltaDeletedResource.Reason ?? DeltaDeletedEntryReason.Deleted);
            }
            else
            {
                throw new SerializationException(Error.Format(SRResources.CannotWriteType, GetType().Name, value?.GetType().FullName));
            }

            if (odataDeletedResource != null)
            {
                await writer.WriteStartAsync(odataDeletedResource).ConfigureAwait(false);
                await writer.WriteEndAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Writes the given deltaDeletedLink specified by the parameter graph as a part of an existing OData message using the given
        /// messageWriter and the writeContext.
        /// </summary>
        /// <param name="value">The object to be written.</param>
        /// <param name="writer">The <see cref="ODataDeltaWriter" /> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        public virtual async Task WriteDeltaDeletedLinkAsync(object value, ODataWriter writer, ODataSerializerContext writeContext)
        {
            if (value == null)
            {
                throw Error.ArgumentNull(nameof(value));
            }

            if (writer == null)
            {
                throw Error.ArgumentNull(nameof(writer));
            }

            ODataDeltaDeletedLink odataDeltaDeletedLink;
            if (value is EdmDeltaDeletedLink edmDeltaDeletedLink)
            {
                odataDeltaDeletedLink = new ODataDeltaDeletedLink(edmDeltaDeletedLink.Source, edmDeltaDeletedLink.Target, edmDeltaDeletedLink.Relationship);
            }
            else if (value is IDeltaDeletedLink deltaDeletedLink)
            {
                odataDeltaDeletedLink = new ODataDeltaDeletedLink(deltaDeletedLink.Source, deltaDeletedLink.Target, deltaDeletedLink.Relationship);
            }
            else
            {
                throw new SerializationException(Error.Format(SRResources.CannotWriteType, GetType().Name, value.GetType().FullName));
            }

            if (odataDeltaDeletedLink != null)
            {
                await writer.WriteDeltaDeletedLinkAsync(odataDeltaDeletedLink).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Writes the given deltaLink specified by the parameter graph as a part of an existing OData message using the given
        /// messageWriter and the writeContext.
        /// </summary>
        /// <param name="value">The object to be written.</param>
        /// <param name="writer">The <see cref="ODataDeltaWriter" /> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        public virtual async Task WriteDeltaLinkAsync(object value, ODataWriter writer, ODataSerializerContext writeContext)
        {
            if (value == null)
            {
                throw Error.ArgumentNull(nameof(value));
            }

            if (writer == null)
            {
                throw Error.ArgumentNull(nameof(writer));
            }

            ODataDeltaLink odataDeltaLink;
            if (value is EdmDeltaLink edmDeltaLink) // typeless
            {
                odataDeltaLink = new ODataDeltaLink(edmDeltaLink.Source, edmDeltaLink.Target, edmDeltaLink.Relationship);
            }
            else if (value is IDeltaLink deltaLink) // typed
            {
                odataDeltaLink = new ODataDeltaLink(deltaLink.Source, deltaLink.Target, deltaLink.Relationship);
            }
            else
            {
                throw new SerializationException(Error.Format(SRResources.CannotWriteType, GetType().Name, value.GetType().FullName));
            }

            if (odataDeltaLink != null)
            {
                await writer.WriteDeltaLinkAsync(odataDeltaLink).ConfigureAwait(false);
            }
        }

        internal DeltaItemKind GetDelteItemKind(object item)
        {
            IEdmChangedObject edmChangedObject = item as IEdmChangedObject;
            if (edmChangedObject != null)
            {
                return edmChangedObject.DeltaKind;
            }

            IDeltaSetItem deltaSetItem = item as IDeltaSetItem;
            if (deltaSetItem != null)
            {
                return deltaSetItem.Kind;
            }

            throw new SerializationException(Error.Format(
                SRResources.CannotWriteType, GetType().Name, item.GetType().FullName));
        }

        private static IEdmStructuredTypeReference GetResourceType(IEdmTypeReference feedType)
        {
            if (feedType.IsCollection())
            {
                IEdmTypeReference elementType = feedType.AsCollection().ElementType();
                if (elementType.IsEntity() || elementType.IsComplex())
                {
                    return elementType.AsStructured();
                }
            }

            string message = Error.Format(SRResources.CannotWriteType, typeof(ODataDeltaResourceSetSerializer).Name, feedType.FullName());
            throw new SerializationException(message);
        }
    }
}
