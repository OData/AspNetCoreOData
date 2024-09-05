//-----------------------------------------------------------------------------
// <copyright file="MyQueryProvider.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Linq;

public class MyQueryProvider : IQueryProvider
{
    public IQueryable CreateQuery(Expression expression)
    {
        throw new NotImplementedException();
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        throw new NotImplementedException();
    }

    public object Execute(Expression expression)
    {
        throw new NotImplementedException();
    }

    public TResult Execute<TResult>(Expression expression)
    {
        throw new NotImplementedException();
    }
}
