//-----------------------------------------------------------------------------
// <copyright file="MediaTypesDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.E2E.Tests.MediaTypes;

public class Order
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public long TrackingNumber { get; set; }
}
