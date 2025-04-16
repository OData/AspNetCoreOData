//-----------------------------------------------------------------------------
// <copyright file="DollarApplyExtensionMethods.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarApply;

internal static class DollarApplyCustomMethods
{
    public static double StdDev(IEnumerable<decimal> values)
    {
        var count = values.Count();
        if (count <= 0)
        {
            return 0;
        }

        var average = values.Average();
        var sumOfSquaresOfDifferences = values.Sum(value => (value - average) * (value - average));

        return Math.Sqrt((double)sumOfSquaresOfDifferences / (count - 1));
    }
}
