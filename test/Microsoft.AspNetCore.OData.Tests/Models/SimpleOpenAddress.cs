//-----------------------------------------------------------------------------
// <copyright file="SimpleOpenAddress.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.Tests.Models;

public class SimpleOpenAddress
{
    public string Street { get; set; }
    public string City { get; set; }
    public IDictionary<string, object> Properties { get; set; }
}
