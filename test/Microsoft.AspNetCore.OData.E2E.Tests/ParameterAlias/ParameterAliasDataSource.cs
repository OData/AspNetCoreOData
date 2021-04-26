// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ParameterAlias
{
    /// <summary>
    /// entity type
    /// </summary>
    public class Trade
    {
        [Key]
        public int TradeID { get; set; }

        public string ProductName { get; set; }

        public string Description { get; set; }

        public long? TradingVolume { get; set; }

        public CountryOrRegion PortingCountryOrRegion { get; set; }

        public TradeLocation TradeLocation { get; set; }
    }

    /// <summary>
    /// enum type
    /// </summary>
    public enum CountryOrRegion
    {
        Australia,

        USA,

        Canada,

        Italy
    }

    /// <summary>
    /// complex type
    /// </summary>
    public class TradeLocation
    {
        public string City { get; set; }

        public int ZipCode { get; set; }
    }
}
