//-----------------------------------------------------------------------------
// <copyright file="DeserializationServiceProviderHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Deserialization;

internal static class DeserializationServiceProviderHelper
{
    public static IServiceProvider GetServiceProvider()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddSingleton<IODataDeserializerProvider, ODataDeserializerProvider>();

        // Deserializers.
        services.AddSingleton<ODataResourceDeserializer>();
        services.AddSingleton<ODataEnumDeserializer>();
        services.AddSingleton<ODataPrimitiveDeserializer>();
        services.AddSingleton<ODataResourceSetDeserializer>();
        services.AddSingleton<ODataCollectionDeserializer>();
        services.AddSingleton<ODataEntityReferenceLinkDeserializer>();
        services.AddSingleton<ODataActionPayloadDeserializer>();

        services.AddSingleton<IAssemblyResolver, DefaultAssemblyResolver>();

        return services.BuildServiceProvider();
    }
}
