// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Query.Container;

namespace Microsoft.AspNetCore.OData.NewtonsoftJson.Tests
{
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
}
