// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.Routing.Tests.Conventions
{
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
}
