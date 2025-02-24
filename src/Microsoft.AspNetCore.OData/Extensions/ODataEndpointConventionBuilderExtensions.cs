//-----------------------------------------------------------------------------
// <copyright file="ODataEndpointConventionBuilderExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Edm;

namespace Microsoft.AspNetCore.OData.Extensions
{
    /// <summary>
    /// Extension methods for annotating OData metadata on an <see cref="Endpoint" />.
    /// </summary>
    public static class ODataEndpointConventionBuilderExtensions
    {
        /// <summary>
        /// Adds an OData prefix metadata to <see cref="Endpoint.Metadata" /> associated with the current endpoint.
        /// The prefix should be same as the defined prefix when calling 'AddRouteComponents'
        /// This method typically is used in Minimal API scenarios.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
        /// <param name="prefix">The route component prefix.</param>
        /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
        /// <remarks>
        /// When we target on .NET 7 or above, we can get the 'ServiceProvider' from Endpoint builder,
        /// Then, we should check the 'AddOData' is called and associated route with given prefix registered.
        /// </remarks>
        public static TBuilder UseOData<TBuilder>(this TBuilder builder, string prefix) where TBuilder : IEndpointConventionBuilder
            => builder.WithMetadata(new ODataPrefixMetadata(prefix ?? string.Empty));

        /// <summary>
        /// Adds an OData model configuration metadata to <see cref="Endpoint.Metadata" /> associated with the current endpoint.
        /// This method typically is used in Minimal API scenarios.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
        /// <param name="config">The model configuration.</param>
        /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
        public static TBuilder UseOData<TBuilder>(this TBuilder builder, IODataModelConfiguration config) where TBuilder : IEndpointConventionBuilder
            => builder.WithMetadata(config);
    }
}
