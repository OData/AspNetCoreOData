//-----------------------------------------------------------------------------
// <copyright file="DollarFilterDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter
{
    public class Person
    {
        public int Id { get; set; }
        public string SSN { get; set; }
    }

    public class Vendor
    {
        public int Id { get; set; }
        public VendorAddress DeclaredSingleValuedProperty { get; set; }
        public int DeclaredPrimitiveProperty { get; set; }
        public Dictionary<string, object> DynamicProperties { get; set; }
    }

    public class VendorAddress
    {
        public string Street { get; set; }
        public VendorCity City { get; set; }
        public Dictionary<string, object> DynamicProperties { get; set; }
    }

    public class VendorCity
    {
        public string Name { get; set; }
        public Dictionary<string, object> DynamicProperties { get; set; }
    }

    public class NonOpenVendorAddress
    {
        public string Street { get; set; }
    }

    public class NotInModelVendorAddress
    {
        public string Street { get; set; }
    }
}
