//-----------------------------------------------------------------------------
// <copyright file="TradesController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ParameterAlias
{
    public class TradesController : ODataController
    {
        public TradesController()
        {
            if (null == Trades)
            {
                InitCustomers();
            }
        }

        private static List<Trade> Trades = null;

        private void InitCustomers()
        {
            Trades = new List<Trade>()
                {
                    new Trade()
                        {
                            TradeID = 1,
                            ProductName = "Rice",
                            Description = "Export Rice to USA",
                            PortingCountryOrRegion = CountryOrRegion.USA,
                            TradingVolume = 1000,
                            TradeLocation = new TradeLocation()
                                {
                                    City = "Guangzhou",
                                    ZipCode = 010
                                }
                        },
                    new Trade()
                        {
                            TradeID = 2,
                            ProductName = "Wheat",
                            Description = "Export Wheat to USA",
                            PortingCountryOrRegion = CountryOrRegion.USA,
                            TradingVolume = null,
                            TradeLocation = new TradeLocation()
                                {
                                    City = "Shenzhen",
                                    ZipCode = 100
                                }
                        },
                    new Trade()
                        {
                            TradeID = 3,
                            ProductName = "Wheat",
                            Description = "Export Wheat to Italy",
                            PortingCountryOrRegion = CountryOrRegion.Italy,
                            TradingVolume = 2000,
                            TradeLocation = new TradeLocation()
                                {
                                    City = "Shanghai",
                                    ZipCode = 001
                                }
                        },
                    new Trade()
                        {
                            TradeID = 4,
                            ProductName = "Corn",
                            Description = "Import Corn from USA",
                            PortingCountryOrRegion = CountryOrRegion.USA,
                            TradingVolume = 8000,
                            TradeLocation = new TradeLocation()
                                {
                                    City = "Beijing",
                                    ZipCode = 000
                                }
                        },
                    new Trade()
                        {
                            TradeID = 5,
                            ProductName = "Corn",
                            Description = "Import Corn from Australia",
                            PortingCountryOrRegion = CountryOrRegion.Australia,
                            TradingVolume = 8000,
                            TradeLocation = new TradeLocation()
                                {
                                    City = "Beijing",
                                    ZipCode = 000
                                }
                        },
                    new Trade()
                        {
                            TradeID = 6,
                            ProductName = "Corn",
                            Description = "Import Corn from Canada",
                            PortingCountryOrRegion = CountryOrRegion.Canada,
                            TradingVolume = 6000,
                            TradeLocation = new TradeLocation()
                                {
                                    City = "Beijing",
                                    ZipCode = 000
                                }
                        }
                };
        }

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(Trades.AsQueryable());
        }

        [HttpGet]
        public IActionResult HandleUnmappedRequest(ODataPath odataPath)
        {
            var functionSegment = odataPath.ElementAt(1) as OperationSegment;
            if (functionSegment != null)
            {
                return Ok(GetParameterValue(functionSegment, "productName") as string);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet("Trades/Microsoft.AspNetCore.OData.E2E.Tests.ParameterAlias.GetTradingVolume(productName={productName},PortingCountryOrRegion={PortingCountryOrRegion})")]
        public IActionResult GetTradingVolume([FromODataUri]string productName, [FromODataUri]CountryOrRegion portingCountryOrRegion)
        {
            var trades = Trades.Where(t => t.ProductName == productName && t.PortingCountryOrRegion == portingCountryOrRegion).ToArray();
            long? tradingVolume = 0;

            foreach (var trade in trades)
            {
                tradingVolume += trade.TradingVolume;
            }
            return Ok(tradingVolume);
        }

        [EnableQuery]
        [HttpGet("GetTradeByCountry(PortingCountryOrRegion={CountryOrRegion})")]
        public IActionResult GetTradeByCountry([FromODataUri] CountryOrRegion countryOrRegion)
        {
            var trades = Trades.Where(t => t.PortingCountryOrRegion == countryOrRegion).ToList();
            return Ok(trades);
        }

        private static object GetParameterValue(OperationSegment segment, string paramName)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            if (string.IsNullOrEmpty(paramName))
            {
                throw Error.ArgumentNullOrEmpty("parameterName");
            }

            if (!segment.Operations.Any() || !segment.Operations.First().IsFunction())
            {
                throw Error.Argument("segment");
            }

            OperationSegmentParameter parameter = segment.Parameters.FirstOrDefault(p => p.Name == paramName);
            Assert.NotNull(parameter);

            ConstantNode node = parameter.Value as ConstantNode;
            if (node != null)
            {
                return node.Value;
            }

            return TranslateNode(parameter.Value);
        }

        internal static object TranslateNode(object value)
        {
            if (value == null)
            {
                return null;
            }

            ConstantNode node = value as ConstantNode;
            if (node != null)
            {
                return node.Value;
            }

            ConvertNode convertNode = value as ConvertNode;
            if (convertNode != null)
            {
                object source = TranslateNode(convertNode.Source);
                return source;
            }

            ParameterAliasNode parameterAliasNode = value as ParameterAliasNode;
            if (parameterAliasNode != null)
            {
                return parameterAliasNode.Alias;
            }

            throw new NotSupportedException();
        }
    }
}
