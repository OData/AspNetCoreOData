//-----------------------------------------------------------------------------
// <copyright file="ODataResourceSetDeserializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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

            ODataReader resourceSetReader = await messageReader.CreateODataResourceSetReaderAsync().ConfigureAwait(false);
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

            IODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(elementType);
            if (deserializer == null)
            {
                throw new SerializationException(
                    Error.Format(SRResources.TypeCannotBeDeserialized, elementType.FullName()));
            }

            foreach (ODataResourceWrapper resourceWrapper in resourceSet.Resources)
            {
                if (resourceWrapper == null)
                {
                    yield return null;
                }
                //else if (resourceWrapper is ODataPrimitiveResourceWrapper primitiveResourceWrapper)
                //{
                //    yield return primitiveResourceWrapper.Value.Value;
                //}
                else
                {
                    yield return deserializer.ReadInline(resourceWrapper, elementType, readContext);
                }
            }

            foreach (ODataItemWrapper wrapper in resourceSet.Items)
            {
                if (wrapper is ODataPrimitiveWrapper primitiveResourceWrapper)
                {
                    yield return primitiveResourceWrapper.Value.Value;
                }
                else if (wrapper is ODataResourceSetWrapper resourceSetWrapper)
                {
                    yield return this.ReadInline(resourceSetWrapper, elementType, readContext);
                }
            }
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
