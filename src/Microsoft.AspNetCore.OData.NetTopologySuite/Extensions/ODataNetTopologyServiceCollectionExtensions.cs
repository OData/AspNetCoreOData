//-----------------------------------------------------------------------------
// <copyright file="ODataNetTopologyServiceCollectionExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.NetTopologySuite.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.NetTopologySuite.Formatter.Serialization;
using Microsoft.AspNetCore.OData.NetTopologySuite.Formatter.Serialization.Converters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Extensions;

/// <summary>
/// Extension methods to register NetTopologySuite spatial formatters for ASP.NET Core OData.
/// </summary>
/// <remarks>
/// This replaces the default OData spatial serializer/deserializer with implementations that read and write
/// NetTopologySuite geometry types.
/// </remarks>
public static class ODataNetTopologyServiceCollectionExtensions
{
    /// <summary>
    /// Registers NetTopologySuite spatial serializers and deserializers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <remarks>
    /// This method is idempotent: it removes existing spatial formatter registrations and adds the NTS-based ones.
    /// </remarks>
    public static IServiceCollection AddODataNetTopologySuite(this IServiceCollection services)
    {
        services.RemoveAll<ODataSpatialDeserializer>();
        services.AddSingleton<ODataSpatialDeserializer, ODataSpatialNetTopologySuiteDeserializer>();
        services.RemoveAll<ODataSpatialSerializer>();
        services.AddSingleton<ODataSpatialSerializer, ODataSpatialNetTopologySuiteSerializer>();

        services.TryAddSingleton<ISpatialConverterRegistry, SpatialConverterRegistry>();
        services.TryAddEnumerable(new[]
        {
            ServiceDescriptor.Singleton<ISpatialConverter, PointConverter>(),
            ServiceDescriptor.Singleton<ISpatialConverter, LineStringConverter>(),
            ServiceDescriptor.Singleton<ISpatialConverter, PolygonConverter>(),
            ServiceDescriptor.Singleton<ISpatialConverter, MultiPointConverter>(),
            ServiceDescriptor.Singleton<ISpatialConverter, MultiLineStringConverter>(),
            ServiceDescriptor.Singleton<ISpatialConverter, MultiPolygonConverter>(),
            ServiceDescriptor.Singleton<ISpatialConverter, GeometryCollectionConverter>(),
        });

        return services;
    }
}
