//-----------------------------------------------------------------------------
// <copyright file="BulkOperationDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.AspNetCore.OData.E2E.Tests.BulkOperation
{
    internal class BulkOperationDataModel
    {
        public class Employee
        {
            [Key]
            public int ID { get; set; }
            public String Name { get; set; }
            public List<Friend> Friends { get; set; }
        }

        public class Friend
        {
            [Key]
            public int Id { get; set; }

            public string Name { get; set; }

            public List<Order> Orders { get; set; }
        }

        public class Order
        {
            [Key]
            public int Id { get; set; }

            public int Price { get; set; }
        }
    }
}
