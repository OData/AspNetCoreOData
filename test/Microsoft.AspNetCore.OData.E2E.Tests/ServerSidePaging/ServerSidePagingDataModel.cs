//-----------------------------------------------------------------------------
// <copyright file="ServerSidePagingDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ServerSidePaging;

public class ServerSidePagingCustomer
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public IList<ServerSidePagingOrder> ServerSidePagingOrders { get; set; }
}

public class ServerSidePagingOrder
{
    [Key]
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public ServerSidePagingCustomer ServerSidePagingCustomer { get; set; }
}

public class ServerSidePagingEmployee
{
    public int Id { get; set; }
    public DateTime HireDate { get; set; }
}

public class SkipTokenPagingCustomer
{
    public int Id { get; set; }
    public string Grade { get; set; }
    public decimal? CreditLimit { get; set; }
    public DateTime? CustomerSince { get; set; }
}

public class SkipTokenPagingEdgeCase1Customer
{
    public int Id { get; set; }
    public decimal? CreditLimit { get; set; }
}

public class ContainmentPagingCustomer
{
    public int Id { get; set; }
    [Contained]
    public List<ContainedPagingOrder> Orders { get; set; }
}

public class ContainedPagingOrder
{
    public int Id { get; set; }
    [Contained]
    public List<ContainedPagingOrderItem> Items { get; set; }
}

public class ContainedPagingOrderItem
{
    public int Id { get; set; }
}

public class NoContainmentPagingCustomer
{
    public int Id { get; set; }
    public List<NoContainmentPagingOrder> Orders { get; set; }
}

public class UntypedPagingCustomerOrder
{
    public int Id { get; set; }

    public IEnumerable<NoContainmentPagingOrder> Orders { get; } = new TruncatedEnumerable(2);
    private class TruncatedEnumerable(int pageSize) : IEnumerable<NoContainmentPagingOrder>, ITruncatedCollection
    {
        public int PageSize => pageSize;

        public bool IsTruncated => enumerator.Position > pageSize;

        Enumerator enumerator = new(pageSize);
        public IEnumerator GetEnumerator()
        {
            return enumerator;
        }

        IEnumerator<NoContainmentPagingOrder> IEnumerable<NoContainmentPagingOrder>.GetEnumerator()
        {
            return enumerator;
        }

        private class Enumerator(int pageSize) : IEnumerator<NoContainmentPagingOrder>
        {
            public int Position { get; set; } = 0;

            public NoContainmentPagingOrder Current => new();

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                // This enumerator simply advances the cursor position and returns false if we have passed the pagesize to stop enumeration
                Position++;
                return Position <= pageSize;
            }

            public void Reset()
            {
                Position = 0;
            }
        }
    }

}

public class NoContainmentPagingOrder
{
    public int Id { get; set; }
    public List<NoContainmentPagingOrderItem> Items { get; set; }
}

public class NoContainmentPagingOrderItem
{
    public int Id { get; set; }
}

public class ContainmentPagingMenu
{
    public int Id { get; set; }
}

public class ContainmentPagingExtendedMenu : ContainmentPagingMenu
{
    [Contained]
    public List<ContainedPagingTab> Tabs { get; set; }
    // Non-contained
    public List<ContainmentPagingPanel> Panels { get; set; }
}

public class ContainedPagingTab
{
    public int Id { get; set; }
}

public class ContainedPagingExtendedTab : ContainedPagingTab
{
    [Contained]
    public List<ContainedPagingItem> Items { get; set; }
}

public class ContainedPagingItem
{
    public int Id { get; set; }
}

public class ContainedPagingExtendedItem : ContainedPagingItem
{
    [Contained]
    public List<ContainedPagingNote> Notes { get; set; }
}

public class ContainedPagingNote
{
    public int Id { get; set; }
}

public class ContainmentPagingPanel
{
    public int Id { get; set; }
}

public class ContainmentPagingExtendedPanel : ContainmentPagingPanel
{
    [Contained]
    public List<ContainedPagingItem> Items { get; set; }
}

public enum CollectionPagingCategory
{
    Retailer,
    Wholesaler,
    Distributor
}

public class CollectionPagingLocation
{
    public string Street { get; set; }
}

public class CollectionPagingCustomer
{
    public int Id { get; set; }
    public List<string> Tags { get; set; }
    public List<CollectionPagingCategory> Categories { get; set; }
    public List<CollectionPagingLocation> Locations { get; set; }
}
