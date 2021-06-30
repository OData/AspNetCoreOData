// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
    internal static class ODataSerializerPropertyHelper
    {
        /// <summary>
        /// Creates an <see cref="ODataProperty"/> with name <paramref name="elementName"/> and value
        /// based on the object represented by <paramref name="graph"/>.
        /// </summary>
        /// <param name="serializer">The <see cref="IODataEdmTypeSerializer"/> writing the property value.</param>
        /// <param name="graph">The object to base the value of the property on.</param>
        /// <param name="expectedType">The expected EDM type of the object represented by <paramref name="graph"/>.</param>
        /// <param name="elementName">The name of the property.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        /// <returns>The <see cref="ODataProperty"/> created.</returns>
        public static ODataProperty CreateProperty(this IODataEdmTypeSerializer serializer, object graph, IEdmTypeReference expectedType, string elementName,
            ODataSerializerContext writeContext)
        {
            if (serializer is ODataCollectionSerializer collectionSerializer)
            {
                return CreateCollectionProperty(collectionSerializer, graph, expectedType, elementName, writeContext);
            }

            Contract.Assert(elementName != null);
            return new ODataProperty
            {
                Name = elementName,
                Value = serializer.CreateODataValue(graph, expectedType, writeContext)
            };
        }

        private static ODataProperty CreateCollectionProperty(ODataCollectionSerializer serializer, object graph, IEdmTypeReference expectedType, string elementName,
            ODataSerializerContext writeContext)
        {
            Contract.Assert(elementName != null);
            var property = serializer.CreateODataValue(graph, expectedType, writeContext);
            if (property != null)
            {
                return new ODataProperty
                {
                    Name = elementName,
                    Value = property
                };
            }
            else
            {
                return null;
            }
        }
    }
}
