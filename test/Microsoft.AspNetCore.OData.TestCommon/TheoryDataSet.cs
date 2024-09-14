//-----------------------------------------------------------------------------
// <copyright file="TheoryDataSet.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.TestCommon;

/// <summary>
/// Base class for <c>TheoryDataSet</c> classes.
/// </summary>
public abstract class TheoryDataSet : IEnumerable<object[]>
{
    private readonly List<object[]> data = new List<object[]>();

    protected void AddItem(params object[] values)
    {
        data.Add(values);
    }

    public IEnumerator<object[]> GetEnumerator()
    {
        return data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
