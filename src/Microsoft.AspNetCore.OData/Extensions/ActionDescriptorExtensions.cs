//-----------------------------------------------------------------------------
// <copyright file="ActionDescriptorExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Extensions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class ActionDescriptorExtensions
    {
        // Maintain the Microsoft.AspNetCore.OData. prefix in any new properties to avoid conflicts with user properties
        // and those of the v3 assembly.  Concern is reduced here due to addition of user type name but prefix
        // also clearly ties the property to code in this assembly.
        private const string ModelKeyPrefix = "Microsoft.AspNetCore.OData.Model+";

        private static readonly object SyncLock = new object();

        internal static IEdmModel GetEdmModel(this ActionDescriptor actionDescriptor, HttpRequest request, Type entityClrType)
        {
            if (actionDescriptor == null)
            {
                throw Error.ArgumentNull(nameof(actionDescriptor));
            }

            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            if (entityClrType == null)
            {
                throw Error.ArgumentNull(nameof(entityClrType));
            }

            IEdmModel model;

            string key = ModelKeyPrefix + entityClrType.FullName;
            object modelAsObject;
            if (actionDescriptor.Properties.TryGetValue(key, out modelAsObject))
            {
                model = modelAsObject as IEdmModel;
            }
            else
            {
                IAssemblyResolver resolver = request.HttpContext.RequestServices.GetService<IAssemblyResolver>();
                ODataConventionModelBuilder builder;
                if (resolver != null)
                {
                    builder = new ODataConventionModelBuilder(resolver, isQueryCompositionMode: true);
                }
                else
                {
                    // need the model builder same as here????
                    // TODO:
                    builder = new ODataConventionModelBuilder(/*DefaultAssemblyResolver.Default, isQueryCompositionMode: true*/);
                }

                EntityTypeConfiguration entityTypeConfiguration = builder.AddEntityType(entityClrType);
                builder.AddEntitySet(entityClrType.Name, entityTypeConfiguration);
                model = builder.GetEdmModel();

                lock (SyncLock)
                {
                    if (!actionDescriptor.Properties.ContainsKey(key))
                    {
                        actionDescriptor.Properties.Add(key, model);
                    }
                }
            }

            return model;
        }
    }
}
