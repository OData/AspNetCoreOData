//-----------------------------------------------------------------------------
// <copyright file="ODataResultExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using System;
using System.Diagnostics;

namespace Microsoft.AspNetCore.OData.Results;



public static class ODataResultExtensions
{
    public static IResult OData(this IResultExtensions resultExtensions, object value, ODataMiniOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(resultExtensions);

        return new ODataResult(value);
    }

    internal static ODataResult OData(object value, ODataMiniOptions? options = null)
    {
        return new ODataResult(value);
    }
}



