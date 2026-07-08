//-----------------------------------------------------------------------------
// <copyright file="MatchesPatternTimeoutEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.MatchesPatternTimeout;

public class MatchesPatternTimeoutEdmModel
{
    public static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();

        // The same entity type is exposed through several sets: one served with default settings, one
        // served with an explicitly configured matchesPattern time span, and one that relies on the
        // default time span while paging (so the collection is materialized during query execution).
        builder.EntitySet<MatchesPatternProduct>("Products");
        builder.EntitySet<MatchesPatternProduct>("BoundedProducts");
        builder.EntitySet<MatchesPatternProduct>("DefaultBoundedProducts");

        return builder.GetEdmModel();
    }
}
