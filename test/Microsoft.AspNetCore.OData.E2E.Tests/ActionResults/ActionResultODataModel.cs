//-----------------------------------------------------------------------------
// <copyright file="ActionResultODataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ActionResults
{
    public class Customer
    {
        public string Id { get; set; }

        public IEnumerable<Book> Books { get; set; }
    }

    public class Book
    {
        public string Id { get; set; }
    }

    public class Weather
    {
        public int Id { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string Summary { get; set; }
    }
}
