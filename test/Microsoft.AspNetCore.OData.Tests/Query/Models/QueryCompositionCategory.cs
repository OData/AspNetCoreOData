//-----------------------------------------------------------------------------
// <copyright file="QueryCompositionCategory.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.Tests.Query.Models
{
    public class QueryCompositionCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public QueryCompositionAddress[] Locations { get; set; }
        public int[] AlternateIds { get; set; }
    }
}
