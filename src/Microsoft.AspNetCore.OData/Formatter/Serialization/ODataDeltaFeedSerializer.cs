﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
    /// <summary>
    /// OData serializer for serializing a collection of <see cref="IEdmEntityType" />
    /// The Collection is of <see cref="IEdmChangedObject"/> which is the base interface implemented by all objects which are a part of the DeltaFeed payload.
    /// </summary>
    public class ODataDeltaFeedSerializer : ODataEdmTypeSerializer
    {
        private const string DeltaFeed = "deltafeed";

        /// <summary>
        /// Initializes a new instance of <see cref="ODataDeltaFeedSerializer"/>.
        /// </summary>
        /// <param name="serializerProvider">The <see cref="ODataSerializerProvider"/> to use to write nested entries.</param>
        public ODataDeltaFeedSerializer(ODataSerializerProvider serializerProvider)
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
                throw new SerializationException(Error.Format(SRResources.CannotSerializerNull, DeltaFeed));
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

            await WriteDeltaFeedInlineAsync(graph, feedType, writer, writeContext).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes the given object specified by the parameter graph as a part of an existing OData message using the given
        /// messageWriter and the writeContext.
        /// </summary>
        /// <param name="graph">The object to be written.</param>
        /// <param name="expectedType">The expected EDM type of the object represented by <paramref name="graph"/>.</param>
        /// <param name="writer">The <see cref="ODataDeltaWriter" /> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        public virtual async Task WriteDeltaFeedInlineAsync(object graph, IEdmTypeReference expectedType, ODataWriter writer,
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
                throw new SerializationException(Error.Format(SRResources.CannotSerializerNull, DeltaFeed));
            }

            IEnumerable enumerable = graph as IEnumerable; // Data to serialize
            if (enumerable == null)
            {
                throw new SerializationException(
                    Error.Format(SRResources.CannotWriteType, GetType().Name, graph.GetType().FullName));
            }

            await WriteFeedAsync(enumerable, expectedType, writer, writeContext).ConfigureAwait(false);
        }

        private async Task WriteFeedAsync(IEnumerable enumerable, IEdmTypeReference feedType, ODataWriter writer,
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
                ODataDeltaResourceSet deltaFeed = CreateODataDeltaFeed(enumerable, feedType.AsCollection(), writeContext);
                if (deltaFeed == null)
                {
                    throw new SerializationException(Error.Format(SRResources.CannotSerializerNull, DeltaFeed));
                }

                // save the next page link for later to support JSON odata.streaming.
                Func<object, Uri> nextLinkGenerator = GetNextLinkGenerator(deltaFeed, enumerable, writeContext);
                deltaFeed.NextPageLink = null;

                //Start writing of the Delta Feed
                await writer.WriteStartAsync(deltaFeed).ConfigureAwait(false);

                object lastResource = null;
                //Iterate over all the entries present and select the appropriate write method.
                //Write method creates ODataDeltaDeletedEntry / ODataDeltaDeletedLink / ODataDeltaLink or ODataEntry.
                foreach (object entry in enumerable)
                {
                    if (entry == null)
                    {
                        throw new SerializationException(SRResources.NullElementInCollection);
                    }

                    lastResource = entry;
                    IEdmChangedObject edmChangedObject = entry as IEdmChangedObject;
                    if (edmChangedObject == null)
                    {
                        throw new SerializationException(Error.Format(
                            SRResources.CannotWriteType, GetType().Name, enumerable.GetType().FullName));
                    }

                    switch (edmChangedObject.DeltaKind)
                    {
                        case EdmDeltaEntityKind.DeletedEntry:
                            await WriteDeltaDeletedEntryAsync(entry, writer, writeContext).ConfigureAwait(false);
                            break;
                        case EdmDeltaEntityKind.DeletedLinkEntry:
                            await WriteDeltaDeletedLinkAsync(entry, writer, writeContext).ConfigureAwait(false);
                            break;
                        case EdmDeltaEntityKind.LinkEntry:
                            await WriteDeltaLinkAsync(entry, writer, writeContext).ConfigureAwait(false);
                            break;
                        case EdmDeltaEntityKind.Entry:
                            {
                                ODataResourceSerializer entrySerializer = SerializerProvider.GetEdmTypeSerializer(elementType) as ODataResourceSerializer;
                                if (entrySerializer == null)
                                {
                                    throw new SerializationException(
                                        Error.Format(SRResources.TypeCannotBeSerialized, elementType.FullName()));
                                }
                                await entrySerializer.WriteDeltaObjectInlineAsync(entry, elementType, writer, writeContext)
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

                deltaFeed.NextPageLink = nextLinkGenerator(lastResource);
            }

            //End Writing of the Delta Feed
            await writer.WriteEndAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a function that takes in an object and generates nextlink uri.
        /// </summary>
        /// <param name="deltaFeed">The resource set describing a collection of structured objects.</param>
        /// <param name="enumerable">>The instance representing the resourceSet being written.</param>
        /// <param name="writeContext">The serializer context.</param>
        /// <returns>The function that generates the NextLink from an object.</returns>
        /// <returns></returns>
        internal static Func<object, Uri> GetNextLinkGenerator(ODataDeltaResourceSet deltaFeed, IEnumerable enumerable, ODataSerializerContext writeContext)
        {
            return ODataResourceSetSerializer.GetNextLinkGenerator(deltaFeed, enumerable, writeContext);
        }

        /// <summary>
        /// Create the <see cref="ODataDeltaResourceSet"/> to be written for the given feed instance.
        /// </summary>
        /// <param name="feedInstance">The instance representing the feed being written.</param>
        /// <param name="feedType">The EDM type of the feed being written.</param>
        /// <param name="writeContext">The serializer context.</param>
        /// <returns>The created <see cref="ODataDeltaResourceSet"/> object.</returns>
        public virtual ODataDeltaResourceSet CreateODataDeltaFeed(IEnumerable feedInstance, IEdmCollectionTypeReference feedType,
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
        /// <param name="graph">The object to be written.</param>
        /// <param name="writer">The <see cref="ODataDeltaWriter" /> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        public virtual async Task WriteDeltaDeletedEntryAsync(object graph, ODataWriter writer, ODataSerializerContext writeContext)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull(nameof(writer));
            }

            EdmDeltaDeletedEntityObject edmDeltaDeletedEntity = graph as EdmDeltaDeletedEntityObject;
            if (edmDeltaDeletedEntity == null)
            {
                throw new SerializationException(Error.Format(SRResources.CannotWriteType, GetType().Name, graph?.GetType().FullName));
            }

            Uri id = StringToUri(edmDeltaDeletedEntity.Id);
            ODataDeletedResource deletedResource = new ODataDeletedResource(id, edmDeltaDeletedEntity.Reason);

            if (edmDeltaDeletedEntity.NavigationSource != null)
            {
                ODataResourceSerializationInfo serializationInfo = new ODataResourceSerializationInfo
                {
                    NavigationSourceName = edmDeltaDeletedEntity.NavigationSource.Name
                };
                deletedResource.SetSerializationInfo(serializationInfo);
            }

            if (deletedResource != null)
            {
                await writer.WriteStartAsync(deletedResource).ConfigureAwait(false);
                await writer.WriteEndAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Writes the given deltaDeletedLink specified by the parameter graph as a part of an existing OData message using the given
        /// messageWriter and the writeContext.
        /// </summary>
        /// <param name="graph">The object to be written.</param>
        /// <param name="writer">The <see cref="ODataDeltaWriter" /> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        public virtual async Task WriteDeltaDeletedLinkAsync(object graph, ODataWriter writer, ODataSerializerContext writeContext)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull(nameof(writer));
            }

            EdmDeltaDeletedLink edmDeltaDeletedLink = graph as EdmDeltaDeletedLink;
            if (edmDeltaDeletedLink == null)
            {
                throw new SerializationException(
                    Error.Format(SRResources.CannotWriteType, GetType().Name, graph?.GetType().FullName));
            }

            ODataDeltaDeletedLink deltaDeletedLink = new ODataDeltaDeletedLink(
                edmDeltaDeletedLink.Source,
                edmDeltaDeletedLink.Target,
                edmDeltaDeletedLink.Relationship);

            if (deltaDeletedLink != null)
            {
                await writer.WriteDeltaDeletedLinkAsync(deltaDeletedLink).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Writes the given deltaLink specified by the parameter graph as a part of an existing OData message using the given
        /// messageWriter and the writeContext.
        /// </summary>
        /// <param name="graph">The object to be written.</param>
        /// <param name="writer">The <see cref="ODataDeltaWriter" /> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        public virtual async Task WriteDeltaLinkAsync(object graph, ODataWriter writer, ODataSerializerContext writeContext)
        {
            if (graph == null)
            {
                throw Error.ArgumentNull(nameof(graph));
            }

            if (writer == null)
            {
                throw Error.ArgumentNull(nameof(writer));
            }

            EdmDeltaLink edmDeltaLink = graph as EdmDeltaLink;
            if (edmDeltaLink == null)
            {
                throw new SerializationException(
                    Error.Format(SRResources.CannotWriteType, GetType().Name, graph.GetType().FullName));
            }

            ODataDeltaLink deltaLink = new ODataDeltaLink(
                edmDeltaLink.Source,
                edmDeltaLink.Target,
                edmDeltaLink.Relationship);

            if (deltaLink != null)
            {
                await writer.WriteDeltaLinkAsync(deltaLink).ConfigureAwait(false);
            }
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

            string message = Error.Format(SRResources.CannotWriteType, typeof(ODataResourceSetSerializer).Name, feedType.FullName());
            throw new SerializationException(message);
        }

        /// <summary>
        /// Safely returns the specified string as a relative or absolute Uri.
        /// </summary>
        /// <param name="uriString">The string to convert to a Uri.</param>
        /// <returns>The string as a Uri.</returns>
        internal static Uri StringToUri(string uriString)
        {
            Uri uri;
            try
            {
                uri = new Uri(uriString, UriKind.RelativeOrAbsolute);
            }
            catch (FormatException)
            {
                // The Uri constructor throws a format exception if it can't figure out the type of Uri
                uri = new Uri(uriString, UriKind.Relative);
            }

            return uri;
        }
    }
}
