// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.Query.Container
{
    internal class AutoSelectedNamedProperty<T> : NamedProperty<T>
    {
        public AutoSelectedNamedProperty()
        {
            AutoSelected = true;
        }
    }
}
