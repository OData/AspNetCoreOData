//-----------------------------------------------------------------------------
// <copyright file="ODataSerializerHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
    internal static class ODataSerializerHelper
    {
        /// <summary>
        /// Appends instance annotations to the destination.
        /// </summary>
        /// <param name="annotations">The annotations to write.</param>
        /// <param name="destination">The destination to hold the instance annotation created.</param>
        /// <param name="writeContext">The serializer context.</param>
        /// <param name="serializerProvider">The SerializerProvider to use to write annotations</param>
        internal static void AppendInstanceAnnotations(IDictionary<string, object> annotations,
            ICollection<ODataInstanceAnnotation> destination,
            ODataSerializerContext writeContext,
            IODataSerializerProvider serializerProvider)
        {
            if (destination == null || annotations == null || writeContext == null || serializerProvider == null)
            {
                return;
            }

            foreach (var annotation in annotations)
            {
                string name = annotation.Key;
                object value = annotation.Value;
                if (value == null || value is ODataNullValue)
                {
                    destination.Add(new ODataInstanceAnnotation(name, ODataNullValueExtensions.NullValue));
                    continue;
                }

                Type valueType = value.GetType();
                IEdmTypeReference edmTypeReference = writeContext.GetEdmType(value, valueType, isUntyped: true);
                if (edmTypeReference.IsUntypedOrCollectionUntyped())
                {
                    if (TypeHelper.IsEnum(valueType))
                    {
                        // we don't have the Edm enum type in the model, let's write it as string.
                        destination.Add(new ODataInstanceAnnotation(name, new ODataPrimitiveValue(value.ToString())));
                        continue;
                    }

                    // Important!! For the collection of untyped, we need to generate the 'ODataCollectionValue', not the ODataResourceSet.
                    // So, Let's switch to using Collection(Edm.Untyped) for primitive, not for resource.
                    edmTypeReference = edmTypeReference.IsCollectionUntyped() ? EdmUntypedHelpers.NullablePrimitiveUntypedCollectionReference : edmTypeReference;
                }

                IODataEdmTypeSerializer propertySerializer = serializerProvider.GetEdmTypeSerializer(edmTypeReference);
                if (propertySerializer == null)
                {
                    throw Error.NotSupported(SRResources.TypeCannotBeSerialized, edmTypeReference.FullName());
                }

                destination.Add(new ODataInstanceAnnotation(name,
                    propertySerializer.CreateODataValue(value, edmTypeReference, writeContext)));
            }
        }
    }
}
