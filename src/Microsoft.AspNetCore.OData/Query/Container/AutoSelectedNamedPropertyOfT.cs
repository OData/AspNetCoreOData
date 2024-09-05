//-----------------------------------------------------------------------------
// <copyright file="AutoSelectedNamedPropertyOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.Query.Container;

internal class AutoSelectedNamedProperty<T> : NamedProperty<T>
{
    public AutoSelectedNamedProperty()
    {
        AutoSelected = true;
    }
}
