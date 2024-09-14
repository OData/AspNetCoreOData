//-----------------------------------------------------------------------------
// <copyright file="ModelBoundQuerySettingsDataSource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ModelBoundQuerySettings;

public static class ModelBoundQuerySettingsDataSource
{
    private const int TargetSize = 3;
    private static readonly List<Author> authors = new List<Author>(
        Enumerable.Range(1, TargetSize).Select(idx => new Author
        {
            AuthorId = idx,
            Books = new List<Book>(
                Enumerable.Range(1, 3).Select(dx => new Book
                {
                    BookId = (idx - 1) * TargetSize + dx
                }))
        }));

    public static List<Author> Authors => authors;
}
