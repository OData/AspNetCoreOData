//-----------------------------------------------------------------------------
// <copyright file="ListsContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Lists
{
    public class ListsContext : DbContext
    {
        public ListsContext(DbContextOptions<ListsContext> options)
            : base(options)
        {
       
        }

        public DbSet<Product> Products{ get; set; }
        public DbSet<Order> Orders{ get; set; }
    }
}

