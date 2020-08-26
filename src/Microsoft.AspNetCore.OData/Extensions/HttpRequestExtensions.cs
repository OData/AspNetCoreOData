// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequest"/>.
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Gets the <see cref="IODataFeature"/> from the services container.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="IODataFeature"/> from the services container.</returns>
        public static IODataFeature ODataFeature(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request.HttpContext.ODataFeature();
        }

        /// <summary>
        /// Gets the <see cref="IODataBatchFeature"/> from the services container.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="IODataBatchFeature"/> from the services container.</returns>
        public static IODataBatchFeature ODataBatchFeature(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request.HttpContext.ODataBatchFeature();
        }

        /// <summary>
        /// Gets the <see cref="IEdmModel"/> from the request container.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="IEdmModel"/> from the request container.</returns>
        public static IEdmModel GetModel(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request.ODataFeature().Model;
        }

        /// <summary>
        /// Gets a value indicating if this is a count request.
        /// </summary>
        /// <returns></returns>
        public static bool IsCountRequest(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ODataPath path = request.ODataFeature().Path;
            return path != null && path.LastSegment is CountSegment;
        }

        /// <summary>
        /// Extension method to return the <see cref="IUrlHelper"/> from the <see cref="HttpRequest"/>.
        /// </summary>
        /// <param name="request">The Http request.</param>
        /// <returns>The <see cref="IUrlHelper"/>.</returns>
        public static IUrlHelper GetUrlHelper(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            IODataFeature feature = request.ODataFeature();
            if (feature.UrlHelper == null)
            {
                // if not set, get it from global.
                feature.UrlHelper = request.HttpContext.GetUrlHelper();
            }

            return feature.UrlHelper;
        }

        /// <summary>
        /// Gets the <see cref="ODataMessageWriterSettings"/> from the request container.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="ODataMessageWriterSettings"/> from the request container.</returns>
        public static ODataMessageReaderSettings GetReaderSettings(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // TODO Maybe clone one???
            return request.HttpContext.RequestServices.GetRequiredService<ODataMessageReaderSettings>();
        }

        /// <summary>
        /// Gets the <see cref="ODataMessageWriterSettings"/> from the request container.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="ODataMessageWriterSettings"/> from the request container.</returns>
        public static ODataMessageWriterSettings GetWriterSettings(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // TODO Maybe clone one???
            return request.HttpContext.RequestServices.GetRequiredService<ODataMessageWriterSettings>();
        }

        /// <summary>
        /// Gets the <see cref="IODataBatchFeature"/> from the services container.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="IODataBatchFeature"/> from the services container.</returns>
        //public static IODataBatchFeature ODataBatchFeature(this HttpRequest request)
        //{
        //    if (request == null)
        //    {
        //        throw Error.ArgumentNull("request");
        //    }

        //    return request.HttpContext.ODataBatchFeature();
        //}

        /// <summary>
        /// get the deserializer provider associated with the request.
        /// </summary>
        /// <returns></returns>
        public static ODataDeserializerProvider GetDeserializerProvider(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request.HttpContext.RequestServices.GetRequiredService<ODataDeserializerProvider>();
        }

        /// <summary>
        /// Retrieves the Content-ID to Location mapping associated with the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The Content-ID to Location mapping associated with this request, or <c>null</c> if there isn't one.</returns>
        public static IDictionary<string, string> GetODataContentIdMapping(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return null;
            // return request.ODataBatchFeature().ContentIdMapping;
        }

        /// <summary>
        /// Creates a link for the next page of results; To be used as the value of @odata.nextLink.
        /// </summary>
        /// <param name="request">The request on which to base the next page link.</param>
        /// <param name="pageSize">The number of results allowed per page.</param>
        /// <param name="instance">Object which can be used to generate the skiptoken value.</param>
        /// <param name="objectToSkipTokenValue">Function that takes in the last object and returns the skiptoken value string.</param>
        /// <returns>A next page link.</returns>
        public static Uri GetNextPageLink(this HttpRequest request, int pageSize, object instance, Func<object, string> objectToSkipTokenValue)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            UriBuilder uriBuilder = new UriBuilder(request.Scheme, request.Host.Host)
            {
                Path = (request.PathBase + request.Path).ToUriComponent()
            };
            if (request.Host.Port.HasValue)
            {
                uriBuilder.Port = request.Host.Port.Value;
            }

            CompatibilityOptions compatibilityOptions = request.GetCompatibilityOptions();

            IEnumerable<KeyValuePair<string, string>> queryParameters = request.Query.SelectMany(kvp => kvp.Value, (kvp, value) => new KeyValuePair<string, string>(kvp.Key, value));
            return GetNextPageHelper.GetNextPageLink(uriBuilder.Uri, queryParameters, pageSize, instance, objectToSkipTokenValue, compatibilityOptions);
        }

        /// <summary>
        /// Gets the set of flags for <see cref="CompatibilityOptions"/> from ODataOptions. 
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Set of flags for <see cref="CompatibilityOptions"/> from ODataOptions.</returns>
        internal static CompatibilityOptions GetCompatibilityOptions(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.HttpContext == null)
            {
                return CompatibilityOptions.None;
            }

            ODataOptions options = request.HttpContext.RequestServices.GetRequiredService<ODataOptions>();

            if (options == null)
            {
                return CompatibilityOptions.None;
            }

            return options.CompatibilityOptions;
        }

        /// <summary>
        /// Creates an ETag from concurrency property names and values.
        /// </summary>
        /// <param name="request">The input property names and values.</param>
        /// <param name="properties">The input property names and values.</param>
        /// <returns>The generated ETag string.</returns>
        public static string CreateETag(this HttpRequest request, IDictionary<string, object> properties)
        {
            return request.GetETagHandler().CreateETag(properties)?.ToString();
        }

        /// <summary>
        /// Gets the <see cref="IETagHandler"/> from the services container.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="IETagHandler"/> from the services container.</returns>
        public static IETagHandler GetETagHandler(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request.HttpContext.RequestServices.GetRequiredService<IETagHandler>();
        }

        /// <summary>
        /// Gets the dependency injection container for the OData request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The dependency injection container.</returns>
        public static IServiceProvider GetSubServiceProvider(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            IServiceProvider requestContainer = request.ODataFeature().SubServiceProvider;
            if (requestContainer != null)
            {
                return requestContainer;
            }

            // HTTP routes will not have chance to call CreateRequestContainer. We have to call it.
            return request.CreateSubServiceProvider(request.ODataFeature().RouteName);
        }

        /// <summary>
        /// Creates a request container that associates with the <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="routeName">The name of the route.</param>
        /// <returns>The request container created.</returns>
        public static IServiceProvider CreateSubServiceProvider(this HttpRequest request, string routeName)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (request.ODataFeature().SubServiceProvider != null)
            {
                throw Error.InvalidOperation(SRResources.SubRequestServiceProviderAlreadyExists);
            }

            IServiceScope requestScope = request.CreateRequestScope(routeName);
            IServiceProvider requestContainer = requestScope.ServiceProvider;

            request.ODataFeature().RequestScope = requestScope;
            request.ODataFeature().SubServiceProvider = requestContainer;

            return requestContainer;
        }

        /// <summary>
        /// Deletes the request container from the <paramref name="request"/> and disposes
        /// the container if <paramref name="dispose"/> is <c>true</c>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="dispose">
        /// Returns <c>true</c> to dispose the request container after deletion; <c>false</c> otherwise.
        /// </param>
        public static void DeleteSubRequestProvider(this HttpRequest request, bool dispose)
        {
            if (request.ODataFeature().RequestScope != null)
            {
                IServiceScope requestScope = request.ODataFeature().RequestScope;
                request.ODataFeature().RequestScope = null;
                request.ODataFeature().SubServiceProvider = null;

                if (dispose)
                {
                    requestScope.Dispose();
                }
            }
        }

        /// <summary>
        /// Create a scoped request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="routeName">The route name.</param>
        /// <returns></returns>
        private static IServiceScope CreateRequestScope(this HttpRequest request, string routeName)
        {
            IPerRouteContainer perRouteContainer = request.HttpContext.RequestServices.GetRequiredService<IPerRouteContainer>();
            if (perRouteContainer == null)
            {
                throw Error.InvalidOperation(SRResources.MissingODataServices, nameof(IPerRouteContainer));
            }

            IServiceProvider rootContainer = perRouteContainer.GetServiceProvider(routeName);
            IServiceScope scope = rootContainer.GetRequiredService<IServiceScopeFactory>().CreateScope();

            // Bind scoping request into the OData container.
            //if (!string.IsNullOrEmpty(routeName))
            //{
            //    scope.ServiceProvider.GetRequiredService<HttpRequestScope>().HttpRequest = request;
            //}

            return scope;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static ODataVersion GetODataResponseVersion(this HttpRequest request)
        {
            Contract.Assert(request != null, "GetODataResponseVersion called with a null request");
            return request.ODataMaxServiceVersion() ??
                request.ODataMinServiceVersion() ??
                request.ODataServiceVersion() ??
                ODataVersionConstraint.DefaultODataVersion;
        }

        internal static ODataQueryOptions GetQueryOptions(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ODataFeature feature = request.ODataFeature() as ODataFeature;

            return feature.QueryOptions;
        }

        internal static ODataVersion? ODataServiceVersion(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return GetODataVersionFromHeader(request.Headers, ODataVersionConstraint.ODataServiceVersionHeader);
        }

        internal static ODataVersion? ODataMaxServiceVersion(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return GetODataVersionFromHeader(request.Headers, ODataVersionConstraint.ODataMaxServiceVersionHeader);
        }

        internal static ODataVersion? ODataMinServiceVersion(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return GetODataVersionFromHeader(request.Headers, ODataVersionConstraint.ODataMinServiceVersionHeader);
        }

        private static ODataVersion? GetODataVersionFromHeader(IHeaderDictionary headers, string headerName)
        {
            StringValues values;
            if (headers.TryGetValue(headerName, out values))
            {
                string value = values.FirstOrDefault();
                if (value != null)
                {
                    string trimmedValue = value.Trim(' ', ';');
                    try
                    {
                        return ODataUtils.StringToODataVersion(trimmedValue);
                    }
                    catch (ODataException)
                    {
                        // Parsing the odata version failed.
                    }
                }
            }

            return null;
        }
    }
}
