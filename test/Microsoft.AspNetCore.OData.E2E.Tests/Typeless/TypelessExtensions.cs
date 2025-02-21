//-----------------------------------------------------------------------------
// <copyright file="TypelessExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Typeless;

internal static class TypelessExtensions
{
    public static void Process(this HttpRequest request)
    {
        var path = request.ODataFeature().Path;
        var elementType = path.EdmType().Definition.AsElementType();
        var model = request.ODataFeature().Model;
        var queryContext = new ODataQueryContext(model, elementType, path);
        var queryOptions = new ODataQueryOptions(queryContext, request);

        request.ODataFeature().SelectExpandClause = queryOptions.SelectExpand?.SelectExpandClause;
        request.ODataFeature().ApplyClause = queryOptions.Apply?.ApplyClause;
    }
}
