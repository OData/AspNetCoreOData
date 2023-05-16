//using System;
//using System.Collections.Generic;
//using System.Linq;
////using Microsoft.AspNetCore.Http;
////using Microsoft.AspNetCore.OData.Abstracts;
////using Microsoft.AspNetCore.OData.Common;
////using Microsoft.AspNetCore.OData.Formatter.Deserialization;
////using Microsoft.AspNetCore.OData.Query;
////using Microsoft.Extensions.DependencyInjection;
////using Microsoft.Extensions.Primitives;
//using Microsoft.OData;
//using Microsoft.OData.Edm;
//using Microsoft.OData.UriParser;
//using QueryBuilder.Abstracts;
//using QueryBuilder;

//namespace QueryBuilder.Extensions
//{
//    /// <summary>
//    /// Provides extension methods for the <see cref="HttpRequest"/>.
//    /// </summary>
//    public static class HttpRequestExtensions
//    {
//        /// <summary>
//        /// Returns the <see cref="IODataFeature"/> from the DI container.
//        /// </summary>
//        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
//        /// <returns>The <see cref="IODataFeature"/> from the services container.</returns>
//        public static IODataFeature ODataFeature(this HttpRequest request)
//        {
//            if (request == null)
//            {
//                throw Error.ArgumentNull(nameof(request));
//            }

//            return request.HttpContext.ODataFeature();
//        }
//    }
//}
