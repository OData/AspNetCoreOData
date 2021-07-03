// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Sets up default OData options for <see cref="MvcOptions"/>.
    /// </summary>
    public class ODataMvcOptionsSetup : IConfigureOptions<MvcOptions>
    {
        /// <summary>
        /// Configure the default <see cref="MvcOptions"/>
        /// </summary>
        /// <param name="options">The <see cref="MvcOptions"/> to configure.</param>
        public void Configure(MvcOptions options)
        {
            if (options == null)
            {
                throw Error.ArgumentNull(nameof(options));
            }

            // Read formatters
            foreach (ODataInputFormatter inputFormatter in ODataInputFormatterFactory.Create().Reverse())
            {
                options.InputFormatters.Insert(0, inputFormatter);
            }

            // Write formatters
            foreach (ODataOutputFormatter outputFormatter in ODataOutputFormatterFactory.Create().Reverse())
            {
                options.OutputFormatters.Insert(0, outputFormatter);
            }
        }
    }
}
