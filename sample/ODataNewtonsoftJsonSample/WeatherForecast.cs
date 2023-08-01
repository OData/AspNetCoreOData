//-----------------------------------------------------------------------------
// <copyright file="WeatherForecast.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;

namespace ODataNewtonsoftJsonSample
{
    public class WeatherForecast
    {
        [Key]
        public int Key { get; set; }

        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string Summary { get; set; }

        // This property is hold the Mac value for OData side
        public string ODataMac
        {
            get => Mac.ToString();
            set => PhysicalAddress.Parse(value);
        }

        public PhysicalAddress Mac { get; set; }
    }
}
