// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Sets up default options for <see cref="ODataRoutingOptions"/>.
    /// </summary>
    internal class ODataRoutingOptionsSetup : IConfigureOptions<ODataRoutingOptions>
    {
        private readonly IServiceProvider _serviceProvider;

        public ODataRoutingOptionsSetup(IServiceProvider sp)
        {
            _serviceProvider = sp;
        }

        public void Configure(ODataRoutingOptions options)
        {
            // Set up ModelBinding
            //options.ModelBinderProviders.Add(new BinderTypeModelBinderProvider());
            //options.ModelBinderProviders.Add(new ServicesModelBinderProvider());
            //options.ModelBinderProviders.Add(new BodyModelBinderProvider(options.InputFormatters, _readerFactory, _loggerFactory, options));
        }
    }
}
