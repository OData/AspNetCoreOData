//-----------------------------------------------------------------------------
// <copyright file="ExampleQueryOptionsBindingExtension.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Extension;
using System.Linq;

namespace ODataRoutingSample
{
    public class ExampleQueryOptionsBindingExtension : IODataQueryOptionsBindingExtension
    {
        public IQueryable ApplyTo(IQueryable query, ODataQueryOptions queryOptions, ODataQuerySettings querySettings)
        {
            //Do something here
            return query;
        }
    }
}
