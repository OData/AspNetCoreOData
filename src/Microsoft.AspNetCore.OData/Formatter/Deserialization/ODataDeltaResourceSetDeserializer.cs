// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Formatter.Wrapper;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Deserialization
{
    /// <summary>
    /// Represents an <see cref="ODataDeserializer"/> that can read OData delta resource sets.
    /// </summary>
    public class ODataDeltaResourceSetDeserializer : ODataEdmTypeDeserializer
    {
        // private static readonly MethodInfo CastMethodInfo = typeof(Enumerable).GetMethod("Cast");

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataDeltaResourceSetDeserializer"/> class.
        /// </summary>
        /// <param name="deserializerProvider">The deserializer provider to use to read inner objects.</param>
        public ODataDeltaResourceSetDeserializer(ODataDeserializerProvider deserializerProvider)
            : base(ODataPayloadKind.Delta, deserializerProvider)
        {
        }

        /// <inheritdoc />
        public override async Task<object> ReadAsync(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
        {
            if (messageReader == null)
            {
                throw Error.ArgumentNull(nameof(messageReader));
            }

            if (readContext == null)
            {
                throw Error.ArgumentNull(nameof(readContext));
            }

            // TODO: Need to modify how to get the Edm type for the delta resource set?
            IEdmTypeReference edmType = readContext.GetEdmType(type);
            Contract.Assert(edmType != null);

            IEdmEntityType entityType = edmType.Definition as IEdmEntityType;

            EdmDeltaCollectionType edmCollectionType = new EdmDeltaCollectionType(edmType);
            edmType = new EdmCollectionTypeReference(edmCollectionType);

            // TODO: is it ok to read the top level collection of entity?
            if (!(edmType.IsCollection() && edmType.AsCollection().ElementType().IsStructured()))
            {
                throw Error.Argument("edmType", SRResources.ArgumentMustBeOfType, EdmTypeKind.Complex + " or " + EdmTypeKind.Entity);
            }

            IEdmNavigationSource navigationSource;
            if (readContext.Path == null)
            {
                throw Error.Argument("readContext", SRResources.ODataPathMissing);
            }

            navigationSource = readContext.Path.GetNavigationSource();
            if (navigationSource == null)
            {
                throw new SerializationException(SRResources.NavigationSourceMissingDuringDeserialization);
            }

            ODataReader resourceSetReader = await messageReader.CreateODataDeltaResourceSetReaderAsync(navigationSource as IEdmEntitySet, entityType).ConfigureAwait(false);
            object deltaResourceSet = await resourceSetReader.ReadResourceOrResourceSetAsync().ConfigureAwait(false);
            return ReadInline(deltaResourceSet, edmType, readContext);
        }

        /// <inheritdoc />
        public sealed override object ReadInline(object item, IEdmTypeReference edmType, ODataDeserializerContext readContext)
        {
            if (item == null)
            {
                return null;
            }

            if (edmType == null)
            {
                throw Error.ArgumentNull(nameof(edmType));
            }

            if (readContext == null)
            {
                throw Error.ArgumentNull(nameof(readContext));
            }

            if (!edmType.IsCollection() || !edmType.AsCollection().ElementType().IsStructured())
            {
                throw Error.Argument(nameof(edmType), SRResources.TypeMustBeResourceSet, edmType.ToTraceString());
            }

            ODataDeltaResourceSetWrapper resourceSet = item as ODataDeltaResourceSetWrapper;
            if (resourceSet == null)
            {
                throw Error.Argument(nameof(item), SRResources.ArgumentMustBeOfType, typeof(ODataResourceSetWrapper).Name);
            }

            // Recursion guard to avoid stack overflows
            RuntimeHelpers.EnsureSufficientExecutionStack();

            IEdmStructuredTypeReference elementType = edmType.AsCollection().ElementType().AsStructured();

            IEnumerable result = ReadDeltaResourceSet(resourceSet, elementType, readContext);
            if (result != null)
            {
                if (readContext.IsNoClrType)
                {
                    EdmChangedObjectCollection changedObjCollection = new EdmChangedObjectCollection(elementType.Definition as IEdmEntityType);
                    foreach (IEdmChangedObject changedObject in result)
                    {
                        changedObjCollection.Add(changedObject);
                    }

                    return changedObjCollection;
                }
                else
                {
                    Type elementClrType = readContext.Model.GetClrType(elementType);
                    Type changedObjCollType = typeof(DeltaSet<>).MakeGenericType(elementClrType);

                    DeltaSet deltaSet = Activator.CreateInstance(changedObjCollType) as DeltaSet;
                    foreach (var delta in result)
                    {
                        IDeltaItem deltaItem = delta as IDeltaItem;
                        if (deltaItem != null)
                        {
                            deltaSet.Add(deltaItem);
                        }
                    }

                    return deltaSet;
                }
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// Deserializes the given <paramref name="deltaResourceSet"/> under the given <paramref name="readContext"/>.
        /// </summary>
        /// <param name="deltaResourceSet">The delta resource set to deserialize.</param>
        /// <param name="elementType">The element type.</param>
        /// <param name="readContext">The deserializer context.</param>
        /// <returns>The deserialized resource set object.</returns>
        public virtual IEnumerable ReadDeltaResourceSet(ODataDeltaResourceSetWrapper deltaResourceSet, IEdmStructuredTypeReference elementType, ODataDeserializerContext readContext)
        {
            if (deltaResourceSet == null)
            {
                throw Error.ArgumentNull(nameof(deltaResourceSet));
            }

            if (readContext == null)
            {
                throw Error.ArgumentNull(nameof(readContext));
            }

            // Delta Items
            foreach (ODataItemWrapper itemWrapper in deltaResourceSet.DeltaItems)
            {
                // Deleted Link
                ODataDeltaDeletedLinkWrapper deletedLinkWrapper = itemWrapper as ODataDeltaDeletedLinkWrapper;
                if (deletedLinkWrapper != null)
                {
                    yield return ReadDeltaDeletedLink(deletedLinkWrapper, elementType, readContext);
                }

                // Added Link
                ODataDeltaLinkWrapper deltaLinkWrapper = itemWrapper as ODataDeltaLinkWrapper;
                if (deltaLinkWrapper != null)
                {
                    yield return ReadDeltaLink(deltaLinkWrapper, elementType, readContext);
                }

                // DeletedResource
                ODataDeletedResourceWrapper deletedResourceWrapper = itemWrapper as ODataDeletedResourceWrapper;
                if (deletedResourceWrapper != null)
                {
                    // TODO: deleted resource
                    yield return null;
                }

                // Added resource or updated resource
                ODataResourceWrapper resourceWrapper = itemWrapper as ODataResourceWrapper;
                if (resourceWrapper != null)
                {
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

                    IEdmTypeReference edmTypeReference = actualType.ToEdmTypeReference(true);
                    ODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(edmTypeReference);
                    if (deserializer == null)
                    {
                        throw new SerializationException(
                            Error.Format(SRResources.TypeCannotBeDeserialized, edmTypeReference.FullName()));
                    }

                    // TODO: normal resource
                    // yield return deserializer.ReadInline(resourceWrapper, edmTypeReference, readContext);
                    if (readContext.IsDeltaOfT)
                    {
                        Type elementClrType = readContext.Model.GetClrType(elementType);
                        Type deltaType = typeof(Deltas.Delta<>).MakeGenericType(elementClrType);
                        Deltas.IDelta delta = Activator.CreateInstance(deltaType) as Deltas.IDelta;
                        yield return delta;
                    }
                    else
                    {
                        yield return new EdmDeltaResourceObject(actualType as IEdmEntityType);
                    }

                    continue;
                }
            }
        }

        /// <summary>
        /// Deserializes the given <paramref name="deletedLink"/> under the given <paramref name="readContext"/>.
        /// </summary>
        /// <param name="deletedLink">The given deleted link.</param>
        /// <param name="elementType">The element type.</param>
        /// <param name="readContext">The deserializer context.</param>
        /// <returns>The created <see cref="IEdmDeltaDeletedLink"/>.</returns>
        public virtual object ReadDeltaDeletedLink(ODataDeltaDeletedLinkWrapper deletedLink, IEdmStructuredTypeReference elementType, ODataDeserializerContext readContext)
        {
            if (deletedLink == null)
            {
                throw Error.ArgumentNull(nameof(deletedLink));
            }

            if (readContext == null)
            {
                throw Error.ArgumentNull(nameof(readContext));
            }

            if (readContext.IsDeltaOfT)
            {
                // refactor how to get the CLR type
                Type elementClrType = readContext.Model.GetClrType(elementType);

                Type deltaLinkType = typeof(DeltaDeletedLink<>).MakeGenericType(elementClrType);
                IDeltaDeletedLink deltaLink = Activator.CreateInstance(deltaLinkType) as IDeltaDeletedLink;

                deltaLink.Source = deletedLink.DeltaDeletedLink.Source;
                deltaLink.Target = deletedLink.DeltaDeletedLink.Target;
                deltaLink.Relationship = deletedLink.DeltaDeletedLink.Relationship;
                return deltaLink;
            }
            else
            {
                EdmDeltaDeletedLink edmDeltaLink = new EdmDeltaDeletedLink(elementType?.Definition as IEdmEntityType);
                edmDeltaLink.Source = deletedLink.DeltaDeletedLink.Source;
                edmDeltaLink.Target = deletedLink.DeltaDeletedLink.Target;
                edmDeltaLink.Relationship = deletedLink.DeltaDeletedLink.Relationship;
                return edmDeltaLink;
            }
        }

        /// <summary>
        /// Deserializes the given <paramref name="link"/> under the given <paramref name="readContext"/>.
        /// </summary>
        /// <param name="link">The given delta link.</param>
        /// <param name="elementType">The element type.</param>
        /// <param name="readContext">The deserializer context.</param>
        /// <returns>The created <see cref="IEdmDeltaLink"/>.</returns>
        public virtual object ReadDeltaLink(ODataDeltaLinkWrapper link, IEdmStructuredTypeReference elementType, ODataDeserializerContext readContext)
        {
            if (link == null)
            {
                throw Error.ArgumentNull(nameof(link));
            }

            if (readContext == null)
            {
                throw Error.ArgumentNull(nameof(readContext));
            }

            if (readContext.IsDeltaOfT)
            {
                // refactor how to get the CLR type
                Type elementClrType = readContext.Model.GetClrType(elementType);

                Type deltaLinkType = typeof(DeltaLink<>).MakeGenericType(elementClrType);
                IDeltaLink deltaLink = Activator.CreateInstance(deltaLinkType) as IDeltaLink;

                deltaLink.Source = link.DeltaLink.Source;
                deltaLink.Target = link.DeltaLink.Target;
                deltaLink.Relationship = link.DeltaLink.Relationship;
                return deltaLink;
            }
            else
            {
                EdmDeltaLink edmDeltaLink = new EdmDeltaLink(elementType?.Definition as IEdmEntityType);
                edmDeltaLink.Source = link.DeltaLink.Source;
                edmDeltaLink.Target = link.DeltaLink.Target;
                edmDeltaLink.Relationship = link.DeltaLink.Relationship;
                return edmDeltaLink;
            }
        }
    }
}
