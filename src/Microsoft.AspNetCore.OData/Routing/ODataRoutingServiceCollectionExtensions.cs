// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Extension methods to add odata routing services to the OData builder.
    /// </summary>
    internal static class ODataBuilderRoutingServicesExtensions
    {
        /// <summary>
        /// Adds the core odata routing services.
        /// </summary>
        /// <param name="builder">The OData builder.</param>
        /// <returns>The IODataBuilder itself.</returns>
        public static IODataBuilder AddODataRouting(this IODataBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            IServiceCollection services = builder.Services;

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApplicationModelProvider, ODataRoutingApplicationModelProvider>());

            // for debug only
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApplicationModelProvider, ODataRoutingApplicationModelDebugProvider>());

            services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, ODataRoutingMatcherPolicy>());

            // OData Routing conventions
            // ~/$metadata
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, MetadataRoutingConvention>());

            // ~/EntitySet
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, EntitySetRoutingConvention>());

            // ~/EntitySet/{key}
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, EntityRoutingConvention>());

            // ~/Singleton
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, SingletonRoutingConvention>());

            // ~/EntitySet|Singleton/.../NS.Function
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, FunctionRoutingConvention>());

            // ~/EntitySet|Singleton/.../NS.Action
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, ActionRoutingConvention>());

            // ~/OperationImport
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, OperationImportRoutingConvention>());

            // ~/EntitySet{key}|Singleton/{Property}
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, PropertyRoutingConvention>());

            // ~/EntitySet{key}|Singleton/{NavigationProperty}
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, NavigationRoutingConvention>());

            // ~/EntitySet{key}|Singleton/{NavigationProperty}/$ref
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, RefRoutingConvention>());

            // Attribute routing
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, AttributeRoutingConvention>());

            services.TryAddSingleton<IODataTemplateTranslator, DefaultODataTemplateTranslator>();
            services.TryAddSingleton<IODataPathTemplateParser, DefaultODataPathTemplateParser>();

            return builder;
        }
    }
}
