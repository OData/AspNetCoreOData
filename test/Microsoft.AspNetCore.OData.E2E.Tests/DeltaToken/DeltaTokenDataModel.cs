//-----------------------------------------------------------------------------
// <copyright file="DeltaTokenDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DeltaToken;

public class TestCustomer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public virtual IList<string> PhoneNumbers { get; set; }
    public virtual IList<TestOrder> Orders { get; set; }
    public virtual IList<TestAddress> FavoritePlaces { get; set; }
    public IDictionary<string, object> DynamicProperties { get; set; }
}

public class TestCustomerWithAddress : TestCustomer
{
    public virtual TestAddress Address { get; set; }
}

public class TestOrder
{
    public int Id { get; set; }
    public int Amount { get; set; }

    public TestAddress Location { get; set; }
}

public class TestAddress
{
    public string State { get; set; }
    public string City { get; set; }
    public int? ZipCode { get; set; }
    public IDictionary<string, object> DynamicProperties { get; set; }
}
