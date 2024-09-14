//-----------------------------------------------------------------------------
// <copyright file="AllowedLogicalOperatorsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Query;
using System;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query;

public class AllowedLogicalOperatorsTests
{
    [Fact]
    public void None_MatchesNone()
    {
        Assert.Equal(AllowedLogicalOperators.None, AllowedLogicalOperators.All & AllowedLogicalOperators.None);
    }

    [Fact]
    public void All_Contains_AllLogicalOperators()
    {
        AllowedLogicalOperators allLogicalOperators = 0;
        foreach (AllowedLogicalOperators allowedLogicalOperator in Enum.GetValues(typeof(AllowedLogicalOperators)))
        {
            if (allowedLogicalOperator != AllowedLogicalOperators.All)
            {
                allLogicalOperators = allLogicalOperators | allowedLogicalOperator;
            }
        }

        Assert.Equal(AllowedLogicalOperators.All, allLogicalOperators);
    }
}
