//-----------------------------------------------------------------------------
// <copyright file="ODataOptionsSetup.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Sets up default options for <see cref="ODataOptions"/>.
    /// </summary>
    public class ODataOptionsSetup : IConfigureOptions<ODataOptions>
    {
        private readonly ILoggerFactory _loggerFactory;
        private IODataPathTemplateParser _templateParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataOptionsSetup" /> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="parser">The OData path template parser.</param>
        public ODataOptionsSetup(ILoggerFactory loggerFactory, IODataPathTemplateParser parser)
        {
            _loggerFactory = loggerFactory;
            _templateParser = parser;
        }

        /// <summary>
        /// Configure the default <see cref="ODataOptions"/>
        /// </summary>
        /// <param name="options">The OData options.</param>
        public void Configure(ODataOptions options)
        {
            if (options == null)
            {
                throw Error.ArgumentNull(nameof(options));
            }

            // Setup built-in routing conventions
            options.Conventions.Add(new MetadataRoutingConvention());
            options.Conventions.Add(new EntitySetRoutingConvention());
            options.Conventions.Add(new EntityRoutingConvention());
            options.Conventions.Add(new SingletonRoutingConvention());
            options.Conventions.Add(new FunctionRoutingConvention());
            options.Conventions.Add(new ActionRoutingConvention());
            options.Conventions.Add(new OperationImportRoutingConvention());
            options.Conventions.Add(new PropertyRoutingConvention());
            options.Conventions.Add(new NavigationRoutingConvention(_loggerFactory.CreateLogger<NavigationRoutingConvention>()));
            options.Conventions.Add(new RefRoutingConvention());
            options.Conventions.Add(new AttributeRoutingConvention(_loggerFactory.CreateLogger<AttributeRoutingConvention>(), _templateParser));
        }
    }
}
