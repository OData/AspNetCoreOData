//-----------------------------------------------------------------------------
// <copyright file="SkipTokenDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.SkipToken;

public class StCustomer
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int Age { get; set; }

    public DateTimeOffset Birthday { get; set; }

    public int MagicNumber { get; set; } // for negative value test

    public StAddress FavoritePlace { get; set; }

    public IList<string> PhoneNumbers { get; set; }

    public IList<StOrder> Orders { get; set; }

    public IDictionary<string, object> DynamicProperties { get; set; }
}

public class StOrder
{
    public string RegId { get; set; }

    public int Id { get; set; }

    public int Amount { get; set; }

    public StAddress Location { get; set; }

    public IDictionary<string, object> DynamicProperties { get; set; }
}

public class StAddress
{
    public string State { get; set; }

    public string City { get; set; }

    public int ZipCode { get; set; }

    public IDictionary<string, object> DynamicProperties { get; set; }
}
