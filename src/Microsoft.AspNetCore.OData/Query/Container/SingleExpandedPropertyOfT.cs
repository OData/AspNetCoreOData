//-----------------------------------------------------------------------------
// <copyright file="SingleExpandedPropertyOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.Query.Container;

internal class SingleExpandedProperty<T> : NamedProperty<T>
{
    public bool IsNull { get; set; }

    public override object GetValue()
    {
        return IsNull ? (object)null : Value;
    }
}
