//-----------------------------------------------------------------------------
// <copyright file="ODataCollectionDeserializer.cs" company=".NET Foundation">
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
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Deserialization
{
    /// <summary>
    /// Represents an <see cref="IODataDeserializer"/> that can read OData collection payloads.
    /// </summary>
    public class ODataCollectionDeserializer : ODataEdmTypeDeserializer
    {
        private static readonly MethodInfo _castMethodInfo = typeof(Enumerable).GetMethod("Cast");

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataCollectionDeserializer"/> class.
        /// </summary>
        /// <param name="deserializerProvider">The deserializer provider to use to read inner objects.</param>
        public ODataCollectionDeserializer(IODataDeserializerProvider deserializerProvider)
            : base(ODataPayloadKind.Collection, deserializerProvider)
        {
        }

        /// <inheritdoc />
        public override async Task<object> ReadAsync(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
        {
            if (messageReader == null)
            {
                throw Error.ArgumentNull("messageReader");
            }

            if (readContext == null)
            {
                throw new ArgumentNullException(nameof(readContext));
            }

            IEdmTypeReference edmType = readContext.GetEdmType(type);
            Contract.Assert(edmType != null);

            if (!edmType.IsCollection())
            {
                throw Error.Argument("type", SRResources.ArgumentMustBeOfType, EdmTypeKind.Collection);
            }

            IEdmCollectionTypeReference collectionType = edmType.AsCollection();
            IEdmTypeReference elementType = collectionType.ElementType();
            ODataCollectionReader reader = await messageReader.CreateODataCollectionReaderAsync(elementType).ConfigureAwait(false);
            return ReadInline(await ReadCollectionAsync(reader).ConfigureAwait(false), edmType, readContext);
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
                throw new ArgumentNullException(nameof(edmType));
            }

            if (readContext == null)
            {
                throw new ArgumentNullException(nameof(readContext));
            }

            if (!edmType.IsCollection())
            {
                throw new SerializationException(
                    Error.Format(SRResources.TypeCannotBeDeserialized, edmType.ToTraceString()));
            }

            IEdmCollectionTypeReference collectionType = edmType.AsCollection();
            IEdmTypeReference elementType = collectionType.ElementType();

            ODataCollectionValue collection = item as ODataCollectionValue;

            if (collection == null)
            {
                throw Error.Argument("item", SRResources.ArgumentMustBeOfType, typeof(ODataCollectionValue).Name);
            }
            // Recursion guard to avoid stack overflows
            RuntimeHelpers.EnsureSufficientExecutionStack();

            IEnumerable result = ReadCollectionValue(collection, elementType, readContext);
            if (result != null)
            {
                if (readContext.IsNoClrType && elementType.IsEnum())
                {
                    return result.ConvertToEdmObject(collectionType);
                }
                else
                {
                    Type elementClrType = readContext.Model.GetClrType(elementType);
                    IEnumerable castedResult = _castMethodInfo.MakeGenericMethod(elementClrType).Invoke(null, new object[] { result }) as IEnumerable;
                    return castedResult;
                }
            }

            return null;
        }

        /// <summary>
        /// Deserializes the given <paramref name="collectionValue"/> under the given <paramref name="readContext"/>.
        /// </summary>
        /// <param name="collectionValue">The <see cref="ODataCollectionValue"/> to deserialize.</param>
        /// <param name="elementType">The element type of the collection to read.</param>
        /// <param name="readContext">The deserializer context.</param>
        /// <returns>The deserialized collection.</returns>
        public virtual IEnumerable ReadCollectionValue(ODataCollectionValue collectionValue, IEdmTypeReference elementType,
            ODataDeserializerContext readContext)
        {
            if (collectionValue == null)
            {
                throw Error.ArgumentNull("collectionValue");
            }
            if (elementType == null)
            {
                throw Error.ArgumentNull("elementType");
            }

            IODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(elementType);
            if (deserializer == null)
            {
                throw new SerializationException(
                    Error.Format(SRResources.TypeCannotBeDeserialized, elementType.FullName()));
            }

            foreach (object item in collectionValue.Items)
            {
                if (elementType.IsPrimitive())
                {
                    yield return item;
                }
                else
                {
                    yield return deserializer.ReadInline(item, elementType, readContext);
                }
            }
        }

        internal static async Task<ODataCollectionValue> ReadCollectionAsync(ODataCollectionReader reader)
        {
            ArrayList items = new ArrayList();
            string typeName = null;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                if (ODataCollectionReaderState.Value == reader.State)
                {
                    items.Add(reader.Item);
                }
                else if (ODataCollectionReaderState.CollectionStart == reader.State)
                {
                    typeName = reader.Item.ToString();
                }
            }

            return new ODataCollectionValue { Items = items.Cast<object>(), TypeName = typeName };
        }
    }
}
