//-----------------------------------------------------------------------------
// <copyright file="OpenSkipTokenDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.SkipToken;

public class OpenSkipTokenCustomer
{
    public int Id { get; set; }

    public string Name { get; set; }

    public IDictionary<string, object> DynamicProperties { get; set; }
}
