//-----------------------------------------------------------------------------
// <copyright file="AutoExpandDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.E2E.Tests.Routing;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.AutoExpand
{
    public class Finding : Customer
    {
        public AwsResource Resource { get; set; }
    }

    [AutoExpand]
    public class Customer
    {
        public int Id { get; set; }

        public Address HomeAddress { get; set; }

        public Order Order { get; set; }

        public Customer Friend { get; set; }
    }

    public class Resource
    {
        public string Id { get; set; }
    }

    public class AwsResource : Resource
    {
        public Service Service { get; set; }
    }

    public class Service
    {
        public string Id { get; set; }
    }

    public class Address
    {
        public string Street { get; set; }

        public string City { get; set; }

        [AutoExpand]
        public CountryOrRegion CountryOrRegion { get; set; }
    }

    public class CnAddress : Address
    {
        public PostCodeInfo PostCode { get; set; }
    }

    public class UsAddress : Address
    {
        [AutoExpand]
        public ZipCodeInfo ZipCode { get; set; }
    }

    public class CountryOrRegion
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class PostCodeInfo
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class ZipCodeInfo
    {
        public int Id { get; set; }

        public string Code { get; set; }
    }

    public class People
    {
        public int Id { get; set; }

        [AutoExpand]
        public Order Order { get; set; }

        public People Friend { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }

        [AutoExpand]
        public ChoiceOrder Choice { get; set; }
    }

    public class ChoiceOrder
    {
        public int Id { get; set; }

        public double Amount { get; set; }
    }

    public class SpecialOrder : Order
    {
        [AutoExpand]
        public ChoiceOrder SpecialChoice { get; set; }
    }

    public class VipOrder : SpecialOrder
    {
        [AutoExpand]
        public ChoiceOrder VipChoice { get; set; }
    }

    public class NormalOrder
    {
        public int Id { get; set; }

        public NormalOrder LinkOrder { get; set; }
    }

    public class DerivedOrder : NormalOrder
    {
        [AutoExpand]
        public OrderDetail OrderDetail { get; set; }

        [AutoExpand(DisableWhenSelectPresent = true)]
        public OrderDetail NotShownDetail { get; set; }
    }

    [AutoExpand(DisableWhenSelectPresent = true)]
    public class DerivedOrder2 : NormalOrder
    {
        public OrderDetail NotShownDetail { get; set; }
    }

    public class OrderDetail
    {
        public int Id { get; set; }

        public string Description { get; set; }
    }

    public class Menu
    {
        public int Id { get; set; }
        [AutoExpand]
        public List<Tab> Tabs { get; set; }
    }

    public class Tab
    {
        public int Id { get; set; }
        [AutoExpand]
        public List<Item> Items { get; set; }
    }

    public class Item
    {
        public int Id { get; set; }
        [AutoExpand]
        public List<Note> Notes { get; set; }
    }

    public class Note
    {
        public int Id { get; set; }
    }
}
