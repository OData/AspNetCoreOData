//-----------------------------------------------------------------------------
// <copyright file="ListsDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Lists
{
    public class Product
    {
        [Key]
        public string ProductId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public IList<string> ListTestString { get; set; } = new List<string>();
        public IList<bool> ListTestBool { get; set; }
        public IList<int> ListTestInt { get; set; }
        public IList<double> ListTestDouble { get; set; }
        public IList<float> ListTestFloat { get; set; }
        public IList<DateTimeOffset> ListTestDateTime { get; set; }
        public IList<Uri> ListTestUri { get; set; }
        public uint[] ListTestUint { get; set; }
    }
}
