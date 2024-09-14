//-----------------------------------------------------------------------------
// <copyright file="ConventionHelpers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Conventions;

internal static class ConventionHelpers
{
    public static T CreateConvention<T>() where T: class, IODataControllerActionConvention
    {
        var services = new ServiceCollection()
            .AddLogging();

        services.AddSingleton<T>();
        return services.BuildServiceProvider().GetRequiredService<T>();
    }
}
