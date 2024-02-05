//-----------------------------------------------------------------------------
// <copyright file="ODataResourceSetDeserializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
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
    /// Represents an <see cref="ODataDeserializer"/> that can read OData resource sets.
    /// </summary>
    public class ODataResourceSetDeserializer : ODataEdmTypeDeserializer
    {
        private static readonly MethodInfo CastMethodInfo = typeof(Enumerable).GetMethod("Cast");

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataResourceSetDeserializer"/> class.
        /// </summary>
        /// <param name="deserializerProvider">The deserializer provider to use to read inner objects.</param>
        public ODataResourceSetDeserializer(IODataDeserializerProvider deserializerProvider)
            : base(ODataPayloadKind.ResourceSet, deserializerProvider)
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

            IEdmTypeReference edmType = readContext.GetEdmType(type);
            Contract.Assert(edmType != null);

            // TODO: is it OK to read the top level collection of entity?
            if (!edmType.IsStructuredOrUntypedStructuredCollection())
            {
                throw Error.Argument("edmType", SRResources.ArgumentMustBeOfType, "Collection of complex, entity or untyped");
            }

            IEdmStructuredType structuredType = edmType.AsCollection().ElementType() as IEdmStructuredType;

            ODataReader resourceSetReader = await messageReader.CreateODataResourceSetReaderAsync(structuredType).ConfigureAwait(false);
            object resourceSet = await resourceSetReader.ReadResourceOrResourceSetAsync().ConfigureAwait(false);
            return ReadInline(resourceSet, edmType, readContext);
        }

        /// <inheritdoc />
        public sealed override object ReadInline(object item, IEdmTypeReference edmType, ODataDeserializerContext readContext)
        {
            if (item == null)
            {
                return null;
            }

            IEdmTypeReference edmElementType = VerifyAndGetElementType(edmType, readContext);

            ODataResourceSetWrapper resourceSet = item as ODataResourceSetWrapper;
            if (resourceSet == null)
            {
                throw Error.Argument(nameof(item), SRResources.ArgumentMustBeOfType, typeof(ODataResourceSetWrapper).Name);
            }

            // Recursion guard to avoid stack overflows
            RuntimeHelpers.EnsureSufficientExecutionStack();

            IEdmStructuredTypeReference elementType = edmElementType.IsUntyped() ?
                EdmUntypedStructuredTypeReference.NullableTypeReference :
                edmElementType.AsStructured();

            IEnumerable result = ReadResourceSet(resourceSet, elementType, readContext);
            if (edmElementType.IsUntyped())
            {
                EdmUntypedCollection untypedList = new EdmUntypedCollection();
                foreach (var element in result)
                {
                    untypedList.Add(element);
                }

                return untypedList;
            }

            if (result != null && elementType.IsComplex())
            {
                if (readContext.IsNoClrType)
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
        /// Deserializes the given <paramref name="resourceSet"/> under the given <paramref name="readContext"/>.
        /// </summary>
        /// <param name="resourceSet">The resource set to deserialize.</param>
        /// <param name="readContext">The deserializer context.</param>
        /// <param name="elementType">The element type of the resource set being read.</param>
        /// <returns>The deserialized resource set object.</returns>
        public virtual IEnumerable ReadResourceSet(ODataResourceSetWrapper resourceSet, IEdmStructuredTypeReference elementType, ODataDeserializerContext readContext)
        {
            if (resourceSet == null)
            {
                throw Error.ArgumentNull(nameof(resourceSet));
            }

            IList<ODataItemWrapper> items = GetItems(resourceSet);
            foreach (ODataItemWrapper wrapper in items)
            {
                if (wrapper == null)
                {
                    yield return null;
                }
                else if (wrapper is ODataPrimitiveWrapper primitiveResourceWrapper)
                {
                    yield return ReadPrimitiveItem(primitiveResourceWrapper, elementType, readContext);
                }
                else if (wrapper is ODataResourceWrapper resourceWrapper)
                {
                    yield return ReadResourceItem(resourceWrapper, elementType, readContext);
                }
                else if (wrapper is ODataResourceSetWrapper resourceSetWrapper)
                {
                    yield return ReadResourceSetItem(resourceSetWrapper, elementType, readContext);
                }
            }
        }

        /// <summary>
        /// Deserializes the given <paramref name="primitiveWrapper"/> under the given <paramref name="readContext"/>.
        /// </summary>
        /// <param name="primitiveWrapper">The primitive item in a set to deserialize.</param>
        /// <param name="elementType">The element type of the parent resource set being read.</param>
        /// <param name="readContext">The deserializer context.</param>
        /// <returns>The deserialized primitive object.</returns>
        public virtual object ReadPrimitiveItem(ODataPrimitiveWrapper primitiveWrapper, IEdmTypeReference elementType, ODataDeserializerContext readContext)
        {
            if (primitiveWrapper == null)
            {
                throw Error.ArgumentNull(nameof(primitiveWrapper));
            }

            return primitiveWrapper.Value.Value;
        }

        /// <summary>
        /// Deserializes the given <paramref name="resourceWrapper"/> under the given <paramref name="readContext"/>.
        /// </summary>
        /// <param name="resourceWrapper">The resource item in a set to deserialize.</param>
        /// <param name="elementType">The element type of the parent resource set being read.</param>
        /// <param name="readContext">The deserializer context.</param>
        /// <returns>The deserialized resource object.</returns>
        public virtual object ReadResourceItem(ODataResourceWrapper resourceWrapper, IEdmTypeReference elementType, ODataDeserializerContext readContext)
        {
            if (resourceWrapper == null)
            {
                throw Error.ArgumentNull(nameof(resourceWrapper));
            }

            IODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(elementType);
            if (deserializer == null)
            {
                throw new SerializationException(
                    Error.Format(SRResources.TypeCannotBeDeserialized, elementType.FullName()));
            }

            ODataDeserializerContext nestedReadContext = readContext.CloneWithoutType();
            if (elementType == null || elementType.IsUntyped())
            {
                // We should use the given type name to read
                elementType = readContext.Model.ResolveResourceType(resourceWrapper.Resource);
                if (elementType.IsUntyped())
                {
                    nestedReadContext.ResourceType = typeof(EdmUntypedObject);
                }
            }

            if (nestedReadContext.ResourceType == null)
            {
                if (readContext.IsNoClrType)
                {
                    if (elementType.IsEntity())
                    {
                        nestedReadContext.ResourceType = typeof(EdmEntityObject);
                    }
                    else
                    {
                        nestedReadContext.ResourceType = typeof(EdmComplexObject);
                    }
                }
                else
                {
                    Type clrType = readContext.Model.GetClrType(elementType);
                    if (clrType == null)
                    {
                        throw new ODataException(
                            Error.Format(SRResources.MappingDoesNotContainResourceType, elementType.FullName()));
                    }

                    nestedReadContext.ResourceType = clrType;
                }
            }

            return deserializer.ReadInline(resourceWrapper, elementType, nestedReadContext);
        }

        /// <summary>
        /// Deserializes the given <paramref name="resourceSetWrapper"/> under the given <paramref name="readContext"/>.
        /// </summary>
        /// <param name="resourceSetWrapper">The resource set item in a set to deserialize.</param>
        /// <param name="elementType">The element type of the parent resource set being read.</param>
        /// <param name="readContext">The deserializer context.</param>
        /// <returns>The deserialized resource set object.</returns>
        public virtual object ReadResourceSetItem(ODataResourceSetWrapper resourceSetWrapper, IEdmTypeReference elementType, ODataDeserializerContext readContext)
        {
            if (resourceSetWrapper == null)
            {
                throw Error.ArgumentNull(nameof(resourceSetWrapper));
            }

            IEdmCollectionTypeReference edmType = readContext.Model.ResolveResourceSetType(resourceSetWrapper.ResourceSet);

            IODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(edmType);
            if (deserializer == null)
            {
                throw new SerializationException(Error.Format(SRResources.TypeCannotBeDeserialized, edmType.FullName()));
            }

            IEdmTypeReference setItemElementType = edmType.AsCollection().ElementType();
            IEdmStructuredTypeReference structuredType = setItemElementType.ToStructuredTypeReference();

            ODataDeserializerContext nestedReadContext = readContext.CloneWithoutType();

            if (setItemElementType.IsUntyped())
            {
                nestedReadContext.ResourceType = typeof(EdmUntypedCollection);
            }
            else if (readContext.IsNoClrType)
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

        private static IList<ODataItemWrapper> GetItems(ODataResourceSetWrapper setWrapper)
        {
            // Could have extra 'resources' added by customer, since it's very very low possibility.
            // So, let's use this logic to avoid potential breaking.
            var extras = setWrapper.Resources.Except(setWrapper.Items).ToList();
            foreach (ODataItemWrapper itemWrapper in extras)
            {
                setWrapper.Items.Add(itemWrapper);
            }

            return setWrapper.Items;
        }

        private IEdmTypeReference VerifyAndGetElementType(IEdmTypeReference edmType, ODataDeserializerContext readContext)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull(nameof(edmType));
            }

            if (readContext == null)
            {
                throw Error.ArgumentNull(nameof(readContext));
            }

            if (!edmType.IsCollection())
            {
                throw Error.Argument(nameof(edmType), SRResources.TypeMustBeResourceSet, edmType.ToTraceString());
            }

            IEdmTypeReference edmElementType = edmType.AsCollection().ElementType();
            if (!edmElementType.IsStructured() && !edmElementType.IsUntyped())
            {
                throw Error.Argument(nameof(edmType), SRResources.TypeMustBeResourceSet, edmType.ToTraceString());
            }

            return edmElementType;
        }
    }
}
