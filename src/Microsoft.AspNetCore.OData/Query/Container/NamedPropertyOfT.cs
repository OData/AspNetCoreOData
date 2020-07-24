// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Microsoft.AspNetCore.OData.Query.Container
{
    internal class NamedProperty<T> : PropertyContainer
    {
        public string Name { get; set; }

        public T Value { get; set; }

        public bool AutoSelected { get; set; }

        public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
            bool includeAutoSelected)
        {
            Contract.Assert(dictionary != null);

            if (Name != null && (includeAutoSelected || !AutoSelected))
            {
                string mappedName = propertyMapper.MapProperty(Name);
                if (String.IsNullOrEmpty(mappedName))
                {
                    throw Error.InvalidOperation(SRResources.InvalidPropertyMapping, Name);
                }

                dictionary.Add(mappedName, GetValue());
            }
        }

        public virtual object GetValue()
        {
            return Value;
        }
    }
}
