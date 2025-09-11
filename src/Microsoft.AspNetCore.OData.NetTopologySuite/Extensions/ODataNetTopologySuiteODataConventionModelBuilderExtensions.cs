//-----------------------------------------------------------------------------
// <copyright file="ODataNetTopologySuiteODataConventionModelBuilderExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.NetTopologySuite.Common;
using Microsoft.AspNetCore.OData.NetTopologySuite.Conventions;
using Microsoft.AspNetCore.OData.NetTopologySuite.Providers;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Extensions;

/// <summary>
/// Extension methods to configure NetTopologySuite integration on an <see cref="ODataConventionModelBuilder"/>.
/// </summary>
public static class ODataNetTopologySuiteODataConventionModelBuilderExtensions
{
    /// <summary>
    /// Adds the NetTopologySuite Edm type mapping provider and necessary spatial conventions.
    /// </summary>
    public static ODataConventionModelBuilder UseNetTopologySuite(this ODataConventionModelBuilder builder)
    {
        if (builder == null)
        {
            throw Error.ArgumentNull(nameof(builder));
        }

        // Add Edm type mapping provider for NetTopologySuite spatial types
        builder.AddEdmTypeMappingProvider(new ODataNetTopologySuiteEdmTypeMappingProvider());

        // Add geography conventions for properties and types
        builder.AddModelConventions(
            new GeographyAttributeEdmPropertyConvention(),
            new GeographyAttributeEdmTypeConvention());

        return builder;
    }
}
