//-----------------------------------------------------------------------------
// <copyright file="ModelBoundQuerySettingsDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ModelBoundQuerySettings;

[Filter("Books")]
public class Author
{
    public long AuthorId { get; set; }
    public List<Book> Books { get; set; }
}

[Filter("BookId")]
public class Book
{
    public long BookId { get; set; }
}
