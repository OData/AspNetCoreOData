// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Tests.Edm
{
    internal static class EdmTestHelpers
    {
        public static IEdmEntityTypeReference AsReference(this IEdmEntityType entity)
        {
            return new EdmEntityTypeReference(entity, isNullable: false);
        }

        public static IEdmComplexTypeReference AsReference(this IEdmComplexType complex)
        {
            return new EdmComplexTypeReference(complex, isNullable: false);
        }
    }
}
