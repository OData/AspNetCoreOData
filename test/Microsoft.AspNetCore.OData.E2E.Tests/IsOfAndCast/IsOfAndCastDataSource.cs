//-----------------------------------------------------------------------------
// <copyright file="IsOfAndCastDataSource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast;

public class IsOfAndCastDataSource
{
    public IsOfAndCastDataSource()
    {
        ResetData();
        InitializeData();
    }

    private void ResetData()
    {
        this.Products?.Clear();
        this.Orders?.Clear();
    }

    public IList<Product> Products { get; private set; }
    public IList<Order> Orders { get; private set; }

    private void InitializeData()
    {
        this.Products = new List<Product>
        {
            new Product
            {
                ID = 1,
                Name = "Product1",
                Domain = Domain.Civil,
                Weight = 1000
            },
            new Product 
            {
                ID = 2,
                Name = "Product2",
                Domain = Domain.Military,
                Weight = 2000,
            },
            new AirPlane
            {
                ID = 3,
                Name = "Product3",
                Domain = Domain.Both,
                Weight = 1500,
                Speed = 900,
                Model = "Boeing 737"
            },
            new JetPlane
            {
                ID = 4,
                Name = "Product4",
                Domain = Domain.Civil,
                Weight = 1200,
                Speed = 1000,
                Model = "Airbus A320",
                JetType = "Turbofan"
            },
            new JetPlane
            {
                ID = 5,
                Name = "Product5",
                Domain = Domain.Military,
                Weight = 1800,
                Speed = 1500,
                Model = "F-22 Raptor",
                JetType = "Afterburning Turbofan"
            }
        };

        this.Orders = new List<Order>
        {
            new Order
            {
                OrderID = 1,
                Location = new Address { City = "City1" },
                Products = new List<Product> { this.Products[0], this.Products[2] }
            },
            new Order
            {
                OrderID = 2,
                Location = new HomeAddress { City = "City2", HomeNo = "100NO" },
                Products = new List<Product> { this.Products[1], this.Products[3], this.Products[4] }
            },
            new Order
            {
                OrderID = 3,
                Location = new OfficeAddress { City = "City3", OfficeNo = "300NO" },
                Products = new List<Product> { this.Products[0], this.Products[2], this.Products[3] }
            },
            new Order
            {
                OrderID = 4,
                Location = new HomeAddress { City = "City4", HomeNo = "200NO" },
                Products = new List<Product> { this.Products[1], this.Products[4] }
            }
        };
    }
}
