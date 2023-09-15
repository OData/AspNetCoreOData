//-----------------------------------------------------------------------------
// <copyright file="ODataRoutingEndpointConventionBuilderExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Provides the <see cref="IEndpointConventionBuilder"/> extension methods to add OData routing metadata.
    /// </summary>
    public static class ODataRoutingEndpointConventionBuilderExtensions
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <typeparam name="TBuilder">TODO</typeparam>
        /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
        /// <param name="serviceProvider">TODO</param>
        /// <returns>The <see cref="IEndpointConventionBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">TODO</exception>
        public static TBuilder WithODataRoutingMetadata<TBuilder>(this TBuilder builder, IServiceProvider serviceProvider) where TBuilder : IEndpointConventionBuilder
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var odataOptions = serviceProvider.GetRequiredService<IOptions<ODataOptions>>().Value;
            var odataPathTemplateParser = serviceProvider.GetRequiredService<IODataPathTemplateParser>();

            builder.Add(endpoint =>
            {
                if (endpoint.Metadata.OfType<ODataRoutingMetadata>().FirstOrDefault() == null)
                {
                    var routePrefixes = odataOptions.RouteComponents.Keys;

                    var routeTemplate = (endpoint as RouteEndpointBuilder).RoutePattern.RawText;
                    var routePrefix = ODataRoutingHelpers.FindRoutePrefix(routeTemplate, routePrefixes, out string sanitizedRouteTemplate);

                    var model = odataOptions.RouteComponents[routePrefix].EdmModel;
                    var serviceProvider = odataOptions.RouteComponents[routePrefix].ServiceProvider;

                    var odataPathTemplate = odataPathTemplateParser.Parse(model, sanitizedRouteTemplate, serviceProvider);

                    if (odataPathTemplate != null)
                    {
                        endpoint.Metadata.Add(new ODataRoutingMetadata(routePrefix, model, odataPathTemplate));
                    }
                }
            });

            return builder;
        }
    }
}
