//-----------------------------------------------------------------------------
// <copyright file="AutoExpandDataSource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.AutoExpand;

public class AutoExpandDataSource
{
    private static IList<Customer> _customers;
    private static IList<People> _people;
    private static IList<NormalOrder> _normalOrders;

    static AutoExpandDataSource()
    {
        GenerateCustomers();

        GeneratePeople();

        GenerateOrders();
    }

    public static IList<Customer> Customers => _customers;

    public static IList<People> People => _people;

    public static IList<NormalOrder> NormalOrders => _normalOrders;

    private static void GenerateCustomers()
    {
        _customers = new List<Customer>();
        Customer previousCustomer = null;
        for (int i = 1; i < 10; i++)
        {
            Address address;
            if (i % 2 == 0)
            {
                address = new CnAddress
                {
                    Street = $"CnStreet {i}",
                    City = $"CnCity {i}",
                    CountryOrRegion = new CountryOrRegion { Id = i + 100, Name = $"C and R {i + 100}" },
                    PostCode = new PostCodeInfo { Id = i + 1000, Name = $"PostCode {i}" }
                };
            }
            else
            {
                address = new UsAddress
                {
                    Street = $"UsStreet {i}",
                    City = $"UsCity {i}",
                    CountryOrRegion = new CountryOrRegion { Id = i + 100, Name = $"C and R {i + 100}" },
                    ZipCode = new ZipCodeInfo { Id = i + 2000, Code = $"Code {i}" }
                };
            }

            // Order.Id is from 1 ~ 9
            var customer = new Customer
            {
                Id = i,
                HomeAddress = address,
                Order = new Order
                {
                    Id = i,
                    Choice = new ChoiceOrder
                    {
                        Id = i,
                        Amount = i * 1000
                    }
                },
            };

            if (i > 1)
            {
                customer.Friend = previousCustomer;
            }

            // For customer whose id is 8 will have SpecialOrder with SpecialChoice.
            if (i == 8)
            {
                customer.Order = new SpecialOrder
                {
                    Id = i,
                    Choice = new ChoiceOrder
                    {
                        Id = i,
                        Amount = i * 1000
                    },
                    SpecialChoice = new ChoiceOrder()
                    {
                        Id = i * 100,
                        Amount = i * 2000
                    }
                };
            }

            // For customer whose id is 9 will have VipOrder with SpecialChoice and VipChoice.
            if (i == 9)
            {
                customer.Order = new VipOrder
                {
                    Id = i,
                    Choice = new ChoiceOrder
                    {
                        Id = i,
                        Amount = i * 1000
                    },
                    SpecialChoice = new ChoiceOrder()
                    {
                        Id = i * 100,
                        Amount = i * 2000
                    },
                    VipChoice = new ChoiceOrder()
                    {
                        Id = i * 1000,
                        Amount = i * 3000
                    }
                };
            }

            _customers.Add(customer);
            previousCustomer = customer;
        }
    }

    public static void GeneratePeople()
    {
        _people = new List<People>();

        People previousPeople = null;
        for (int i = 1; i < 10; i++)
        {
            var people = new People
            {
                Id = i,
                Order = new Order
                {
                    Id = i + 10,  // Order Id is from 10~19
                    Choice = new ChoiceOrder
                    {
                        Id = i + 10,
                        Amount = (i + 10) * 1000
                    }
                },
            };

            if (i > 1)
            {
                people.Friend = previousPeople;
            }

            _people.Add(people);
            previousPeople = people;
        }
    }

    private static void GenerateOrders()
    {
        _normalOrders = new List<NormalOrder>();

        var order2 = new DerivedOrder
        {
            Id = 2,
            OrderDetail = new OrderDetail
            {
                Id = 3,
                Description = "OrderDetail"
            },
            NotShownDetail = new OrderDetail
            {
                Id = 4,
                Description = "NotShownOrderDetail4"
            }
        };

        var order1 = new DerivedOrder
        {
            Id = 1,
            OrderDetail = new OrderDetail
            {
                Id = 1,
                Description = "OrderDetail"
            },
            NotShownDetail = new OrderDetail
            {
                Id = 2,
                Description = "NotShownOrderDetail2"
            }
        };

        var order3 = new DerivedOrder2
        {
            Id = 3,
            NotShownDetail = new OrderDetail
            {
                Id = 5,
                Description = "NotShownOrderDetail4"
            }
        };

        order2.LinkOrder = order1;
        _normalOrders.Add(order2);
        _normalOrders.Add(order3);
    }
}
