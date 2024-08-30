//-----------------------------------------------------------------------------
// <copyright file="DefaultODataBatchHandlerModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Batch;

public class DefaultBatchCustomer
{
    public int Id { get; set; }

    public string Name { get; set; }

    public IList<DefaultBatchOrder> Orders { get; set; }
}

public class DefaultBatchOrder
{
    public int Id { get; set; }

    public DateTimeOffset PurchaseDate { get; set; }
}
