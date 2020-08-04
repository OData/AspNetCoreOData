// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
        // Maintain the Microsoft.AspNet.OData. prefix in any new properties to avoid conflicts with user properties
        // and those of the v3 assembly.  Concern is reduced here due to addition of user type name but prefix
        // also clearly ties the property to code in this assembly.
        private const string ModelKeyPrefix = "Microsoft.AspNet.OData.Model+";

        private static readonly object SyncLock = new object();

        internal static IEdmModel GetEdmModel(this ActionDescriptor actionDescriptor, HttpRequest request, Type entityClrType)
        {
            if (actionDescriptor == null)
            {
                throw Error.ArgumentNull("actionDescriptor");
            }

            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (entityClrType == null)
            {
                throw Error.ArgumentNull("entityClrType");
            }

            IEdmModel model = null;

            string key = ModelKeyPrefix + entityClrType.FullName;
            object modelAsObject = null;
            if (actionDescriptor.Properties.TryGetValue(key, out modelAsObject))
            {
                model = modelAsObject as IEdmModel;
            }
            else
            {
                Microsoft.OData.ModelBuilder.IAssemblyResolver resolver = request.HttpContext.RequestServices.GetService<Microsoft.OData.ModelBuilder.IAssemblyResolver>() as Microsoft.OData.ModelBuilder.IAssemblyResolver;
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
