// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.Query.Container
{
    internal class SingleExpandedProperty<T> : NamedProperty<T>
    {
        public bool IsNull { get; set; }

        public override object GetValue()
        {
            return IsNull ? (object)null : Value;
        }
    }
}
