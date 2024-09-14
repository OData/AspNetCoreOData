//-----------------------------------------------------------------------------
// <copyright file="MediaTypesDataSource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.MediaTypes;

internal static class MediaTypesDataSource
{
    private readonly static List<Order> orders;

    static MediaTypesDataSource()
    {
        orders = new List<Order>
        {
            new Order { Id = 1, Amount = 130, TrackingNumber = 9223372036854775807L }
        };
    }

    public static List<Order> Orders => orders;
}
