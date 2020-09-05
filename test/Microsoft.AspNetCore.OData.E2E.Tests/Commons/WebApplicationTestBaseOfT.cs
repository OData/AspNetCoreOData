// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Commons
{
    public static class ApplicationBuilderExtensions
    {
        public static void AddControllers(this IApplicationBuilder app, params Type[] controllers)
        {
            ApplicationPartManager partManager = app.ApplicationServices.GetRequiredService<ApplicationPartManager>();

            IList<ApplicationPart> parts = partManager.ApplicationParts;
            IList<ApplicationPart> nonAssemblyParts = parts.Where(p => p.GetType() != typeof(IApplicationPartTypeProvider)).ToList();
            partManager.ApplicationParts.Clear();
            partManager.ApplicationParts.Concat(nonAssemblyParts);

            // Add a new AssemblyPart with the controllers.
            AssemblyPart part = new AssemblyPart(new TestAssembly(controllers));
            partManager.ApplicationParts.Add(part);
        }
    }
}
