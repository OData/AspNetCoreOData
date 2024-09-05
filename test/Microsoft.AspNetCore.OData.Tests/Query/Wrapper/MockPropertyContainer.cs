//-----------------------------------------------------------------------------
// <copyright file="MockPropertyContainer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Query.Container;

namespace Microsoft.AspNetCore.OData.Tests.Query.Wrapper;

internal class MockPropertyContainer : PropertyContainer
{
    public MockPropertyContainer()
    {
        Properties = new Dictionary<string, object>();
    }

    public Dictionary<string, object> Properties { get; private set; }

    public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
        bool includeAutoSelected)
    {
        foreach (var kvp in Properties)
        {
            dictionary.Add(kvp.Key, kvp.Value);
        }
    }
}
