//-----------------------------------------------------------------------------
// <copyright file="EdmUntypedHelpers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Edm
{
    internal class EdmUntypedHelpers
    {
        // Collection(Edm.Untyped) for resource set
        public readonly static EdmCollectionTypeReference NullableUntypedCollectionReference
            = new EdmCollectionTypeReference(
                new EdmCollectionType(EdmUntypedStructuredTypeReference.NullableTypeReference));

        // Collection(Edm.Untyped) for collection of (Primitive, enum)
        public readonly static EdmCollectionTypeReference NullablePrimitiveUntypedCollectionReference
            = new EdmCollectionTypeReference(new EdmCollectionType(EdmCoreModel.Instance.GetUntyped()));
    }
}
