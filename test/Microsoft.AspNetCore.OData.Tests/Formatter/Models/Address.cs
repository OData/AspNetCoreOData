//-----------------------------------------------------------------------------
// <copyright file="Address.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Models;

public class Address
{
    public string Street { get; set; }

    public string City { get; set; }

    public string State { get; set; }

    public string ZipCode { get; set; }

    public string CountryOrRegion { get; set; }
}

public class UsAddress : Address
{
    public string UsProp { get; set; }
}

public class CnAddress : Address
{
    public Guid CnProp { get; set; }
}

public class Location
{
    public string Name { get; set; }

    public Address Address { get; set; }
}

public class SimpleOpenAddress
{
    public string Street { get; set; }
    public string City { get; set; }
    public IDictionary<string, object> Properties { get; set; }
}
