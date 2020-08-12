// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Tests.Extensions
{
    /// <summary>
    /// A class to create HttpRequest.
    /// </summary>
    public class RequestFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static HttpRequest Create()
        {
            HttpContext context = new DefaultHttpContext();
            return context.Request;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static HttpRequest Create(IEdmModel model)
        {
            HttpContext context = new DefaultHttpContext();
            context.ODataFeature().Model = model;
            return context.Request;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static HttpRequest Create(Action<IODataFeature> setupAction)
        {
            HttpContext context = new DefaultHttpContext();
            IODataFeature odataFeature = context.ODataFeature();
            setupAction?.Invoke(odataFeature);
            return context.Request;
        }
    }
}
