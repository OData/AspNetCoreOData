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
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Formatter.Wrapper;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Deserialization
{
    /// <summary>
    /// Represents an <see cref="ODataDeserializer"/> that can read OData delta resource sets.
    /// </summary>
    public class ODataDeltaResourceSetDeserializer : ODataEdmTypeDeserializer
    {
        private static readonly MethodInfo CastMethodInfo = typeof(Enumerable).GetMethod("Cast");

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

            // TODO: is it ok to read the top level collection of entity?
            if (!(edmType.IsCollection() && edmType.AsCollection().ElementType().IsStructured()))
            {
                throw Error.Argument("edmType", SRResources.ArgumentMustBeOfType, EdmTypeKind.Complex + " or " + EdmTypeKind.Entity);
            }

            ODataReader resourceSetReader = await messageReader.CreateODataDeltaResourceSetReaderAsync().ConfigureAwait(false);
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

            IEnumerable result = ReadDeltaResourceSet(resourceSet, readContext);
            if (result != null && elementType.IsComplex())
            {
                if (readContext.IsUntyped)
                {
                    EdmComplexObjectCollection complexCollection = new EdmComplexObjectCollection(edmType.AsCollection());
                    foreach (EdmComplexObject complexObject in result)
                    {
                        complexCollection.Add(complexObject);
                    }
                    return complexCollection;
                }
                else
                {
                    Type elementClrType = readContext.Model.GetClrType(elementType);
                    IEnumerable castedResult =
                        CastMethodInfo.MakeGenericMethod(elementClrType).Invoke(null, new object[] { result }) as
                            IEnumerable;
                    return castedResult;
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
        /// <param name="readContext">The deserializer context.</param>
        /// <returns>The deserialized resource set object.</returns>
        public virtual IEnumerable ReadDeltaResourceSet(ODataDeltaResourceSetWrapper deltaResourceSet, ODataDeserializerContext readContext)
        {
            if (deltaResourceSet == null)
            {
                throw Error.ArgumentNull(nameof(deltaResourceSet));
            }

            if (readContext == null)
            {
                throw Error.ArgumentNull(nameof(readContext));
            }

            // resource
            foreach (ODataResourceBaseWrapper resourceBaseWrapper in deltaResourceSet.ResourceBases)
            {
                ODataResourceWrapper resourceWrapper = resourceBaseWrapper as ODataResourceWrapper;
                ODataDeletedResourceWrapper deletedResourceWrapper = resourceBaseWrapper as ODataDeletedResourceWrapper;
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
                    yield return deserializer.ReadInline(resourceWrapper, edmTypeReference, readContext);
                }
                else
                {
                    // TODO: deleted resource
                }
            }

            // Delta links
            foreach (ODataDeltaLinkBaseWrapper deltaLinkBaseWrapper in deltaResourceSet.DeltaLinks)
            {
                ODataDeltaDeletedLinkWrapper deletedLinkWrapper = deltaLinkBaseWrapper as ODataDeltaDeletedLinkWrapper;
                if (deletedLinkWrapper != null)
                {
                    yield return ReadDeltaDeletedLink(deletedLinkWrapper, readContext);
                }
                else
                {
                    yield return ReadDeltaLink((ODataDeltaLinkWrapper)deltaLinkBaseWrapper, readContext);
                }
            }
        }

        /// <summary>
        /// Deserializes the given <paramref name="deletedLink"/> under the given <paramref name="readContext"/>.
        /// </summary>
        /// <param name="deletedLink">The given deleted link.</param>
        /// <param name="readContext">The deserializer context.</param>
        /// <returns>The created <see cref="IEdmDeltaDeletedLink"/>.</returns>
        public virtual IEdmDeltaDeletedLink ReadDeltaDeletedLink(ODataDeltaDeletedLinkWrapper deletedLink, ODataDeserializerContext readContext)
        {
            // TODO:
            return null;
        }

        /// <summary>
        /// Deserializes the given <paramref name="link"/> under the given <paramref name="readContext"/>.
        /// </summary>
        /// <param name="link">The given delta link.</param>
        /// <param name="readContext">The deserializer context.</param>
        /// <returns>The created <see cref="IEdmDeltaLink"/>.</returns>
        public virtual IEdmDeltaLink ReadDeltaLink(ODataDeltaLinkWrapper link, ODataDeserializerContext readContext)
        {
            // TODO:
            return null;
        }
    }
}
