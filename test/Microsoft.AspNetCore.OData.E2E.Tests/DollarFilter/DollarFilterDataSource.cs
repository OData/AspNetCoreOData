//-----------------------------------------------------------------------------
// <copyright file="DollarFilterDataSource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter
{
    public class DollarFilterDataSource
    {
        private static IList<Person> people;

        static DollarFilterDataSource()
        {
            people = new List<Person>
            {
                new Person { Id = 1, SSN = "a'bc" },
                new Person { Id = 2, SSN = "'def" },
                new Person { Id = 3, SSN = "xyz'" },
                new Person { Id = 4, SSN = "'pqr'" }
            };
        }

        public static IList<Person> People => people;
    }
}
