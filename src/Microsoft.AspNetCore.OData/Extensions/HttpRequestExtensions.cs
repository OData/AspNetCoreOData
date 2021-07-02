// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData
{

    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequest"/>.
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Returns the <see cref="IODataFeature"/> from the DI container.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <returns>The <see cref="IODataFeature"/> from the DI container.</returns>
        public static IODataFeature ODataFeature(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            return request.HttpContext.ODataFeature();
        }

        /// <summary>
        /// Returns the <see cref="IODataBatchFeature"/> instance from the DI container.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <returns>The <see cref="IODataBatchFeature"/> from the services container.</returns>
        public static IODataBatchFeature ODataBatchFeature(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            return request.HttpContext.ODataBatchFeature();
        }

        /// <summary>
        /// Returns the <see cref="ODataOptions"/> instance from the DI container.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <returns>The <see cref="ODataOptions"/> instance from the DI container.</returns>
        public static ODataOptions ODataOptions(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            return request.HttpContext.ODataOptions();
        }

        /// <summary>
        /// Gets the <see cref="IEdmModel"/> from the request container.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <returns>The <see cref="IEdmModel"/> from the request container.</returns>
        public static IEdmModel GetModel(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            return request.ODataFeature().Model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <remarks>In situations where </remarks>
        public static LinkedServiceProvider GetLinkedServiceProvider(this HttpRequest request)
        {
            return new LinkedServiceProvider(request.GetRouteServices(), request.HttpContext.RequestServices);
        }

        /// <summary>
        /// Returns the service <typeparamref name="T"/> from the available DI containers. Optionally checks the route-specific 
        /// services first, and then falls back to the application-wide container if <typeparamref name="T"/> was not found.
        /// </summary>
        /// <typeparam name="T">The type of service to return from the DI containers.</typeparam>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <param name="checkRouteServices">
        /// A boolean indicating whether or not to check the route-specific DI container first. Defaults to true.
        /// </param>
        /// <returns></returns>
        public static T GetService<T>(this HttpRequest request, bool checkRouteServices = true)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            T service = default;
            if (checkRouteServices)
            {
                var routeServices = request.GetRouteServices();
                if (routeServices != null)
                {
                    service = routeServices.GetService<T>();
                }
            }

            if (service is not null)
            {
                return service;
            }

            if (request.HttpContext.RequestServices == null)
            {
                throw new ODataException(SRResources.RequestServicesOnHttpContextIsNull);
            }

            return request.HttpContext.RequestServices.GetService<T>();
        }

        /// <summary>
        /// Returns the services <typeparamref name="T"/> from the available DI containers. Optionally checks the route-specific 
        /// services first, and then falls back to the application-wide container if <typeparamref name="T"/> was not found.
        /// </summary>
        /// <typeparam name="T">The type of services to return from the DI containers.</typeparam>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <param name="checkRouteServices">
        /// A boolean indicating whether or not to check the route-specific DI container first. Defaults to true.
        /// </param>
        /// <returns></returns>
        public static IEnumerable<T> GetServices<T>(this HttpRequest request, bool checkRouteServices = true)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            IEnumerable<T> services = default;
            if (checkRouteServices)
            {
                var routeServices = request.GetRouteServices();
                if (routeServices != null)
                {
                    services = routeServices.GetServices<T>();
                }
            }

            if (services is not null)
            {
                return services;
            }

            return request.HttpContext.RequestServices.GetServices<T>();
        }

        /// <summary>
        /// Returns the service <typeparamref name="T"/> from the available DI containers. Optionally checks the route-specific 
        /// services first, and then falls back to the application-wide container if <typeparamref name="T"/> was not found.
        /// </summary>
        /// <typeparam name="T">The type of service to return from the DI containers.</typeparam>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <param name="checkRouteServices">
        /// A boolean indicating whether or not to check the route-specific DI container first. Defaults to true.
        /// </param>
        /// <returns></returns>
        public static T GetRequiredService<T>(this HttpRequest request, bool checkRouteServices = true)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            T service = default;
            if (checkRouteServices)
            {
                var routeServices = request.GetRouteServices();
                if (routeServices != null)
                {
                    service = routeServices.GetService<T>();
                }
            }

            if (service is not null)
            {
                return service;
            }

            return request.HttpContext.RequestServices.GetRequiredService<T>();
        }

        /// <summary>
        /// Gets the <see cref="TimeZoneInfo"/> setting.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <returns>null or the time zone info.</returns>
        public static TimeZoneInfo GetTimeZoneInfo(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            return request.ODataOptions().TimeZone;
        }

        /// <summary>
        /// Gets the bool value indicating whether the non-dollar prefix query option.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <returns>True/false.</returns>
        public static bool IsNoDollarQueryEnabled(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            return request.ODataOptions().EnableNoDollarQueryOptions;

        }

        /// <summary>
        /// Gets a value indicating if this is a count request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <returns></returns>
        public static bool IsCountRequest(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            ODataPath path = request.ODataFeature().Path;
            return path != null && path.LastSegment is CountSegment;
        }

        /// <summary>
        /// Gets the <see cref="ODataMessageWriterSettings"/> from the request container.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <returns>The <see cref="ODataMessageWriterSettings"/> from the request container.</returns>
        public static ODataMessageReaderSettings GetReaderSettings(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            return request.GetRequiredService<ODataMessageReaderSettings>();
        }

        /// <summary>
        /// Gets the <see cref="ODataMessageWriterSettings"/> from the request container.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <returns>The <see cref="ODataMessageWriterSettings"/> from the request container.</returns>
        public static ODataMessageWriterSettings GetWriterSettings(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            return request.GetRequiredService<ODataMessageWriterSettings>();
        }

        /// <summary>
        /// get the deserializer provider associated with the request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <returns></returns>
        public static IODataDeserializerProvider GetDeserializerProvider(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            return request.GetRequiredService<IODataDeserializerProvider>();
        }

        /// <summary>
        /// Creates a link for the next page of results; To be used as the value of @odata.nextLink.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <param name="pageSize">The number of results allowed per page.</param>
        /// <param name="instance">Object which can be used to generate the skiptoken value.</param>
        /// <param name="objectToSkipTokenValue">Function that takes in the last object and returns the skiptoken value string.</param>
        /// <returns>A next page link.</returns>
        public static Uri GetNextPageLink(this HttpRequest request, int pageSize, object instance, Func<object, string> objectToSkipTokenValue)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            UriBuilder uriBuilder = new UriBuilder(request.Scheme, request.Host.Host)
            {
                Path = (request.PathBase + request.Path).ToUriComponent()
            };
            if (request.Host.Port.HasValue)
            {
                uriBuilder.Port = request.Host.Port.Value;
            }

            IEnumerable<KeyValuePair<string, string>> queryParameters = request.Query.SelectMany(kvp => kvp.Value, (kvp, value) => new KeyValuePair<string, string>(kvp.Key, value));
            return GetNextPageHelper.GetNextPageLink(uriBuilder.Uri, queryParameters, pageSize, instance, objectToSkipTokenValue);
        }

        /// <summary>
        /// Creates an ETag from concurrency property names and values.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <param name="properties">The input property names and values.</param>
        /// <param name="timeZone">The Time zone info.</param>
        /// <returns>The generated ETag string.</returns>
        public static string CreateETag(this HttpRequest request, IDictionary<string, object> properties, TimeZoneInfo timeZone = null)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            return request.GetETagHandler().CreateETag(properties, timeZone)?.ToString();
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
                throw Error.ArgumentNull(nameof(request));
            }

            return request.GetService<IETagHandler>();
        }

        /// <summary>
        /// Checks whether the request is a POST targeted at a resource path ending in /$query.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <returns>true if the request path has $query segment.</returns>
        internal static bool IsODataQueryRequest(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            // Requests to paths ending in /$query MUST use the POST verb.
            if (!string.Equals(request.Method, HttpMethods.Post, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string path = request.Path.Value.TrimEnd('/');
            return path.EndsWith("/$query", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the dependency injection container for the OData request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <returns>The dependency injection container.</returns>
        public static IServiceProvider GetRouteServices(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            IServiceProvider requestContainer = request.ODataFeature().Services;
            if (requestContainer != null)
            {
                return requestContainer;
            }

            // if the prefixName == null, it's a non-model scenario
            if (request.ODataFeature().PrefixName == null)
            {
                return null;
            }

            // HTTP routes will not have chance to call CreateRequestContainer. We have to call it.
            return request.CreateRouteServices(request.ODataFeature().PrefixName);
        }

        /// <summary>
        /// Creates a request container that associates with the <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <param name="prefixName">The name of the route.</param>
        /// <returns>The request container created.</returns>
        public static IServiceProvider CreateRouteServices(this HttpRequest request, string prefixName)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            if (request.ODataFeature().Services != null)
            {
                return request.ODataFeature().Services;
            }

            IServiceScope requestScope = request.CreateRequestScope(prefixName);
            IServiceProvider requestContainer = requestScope.ServiceProvider;

            request.ODataFeature().RequestScope = requestScope;
            request.ODataFeature().Services = requestContainer;

            return requestContainer;
        }

        /// <summary>
        /// Deletes the request container from the <paramref name="request"/> and disposes
        /// the container if <paramref name="dispose"/> is <c>true</c>.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <param name="dispose">
        /// Returns <c>true</c> to dispose the request container after deletion; <c>false</c> otherwise.
        /// </param>
        public static void DeleteRouteServices(this HttpRequest request, bool dispose)
        {
            if (request.ODataFeature().RequestScope != null)
            {
                IServiceScope requestScope = request.ODataFeature().RequestScope;
                request.ODataFeature().RequestScope = null;
                request.ODataFeature().Services = null;

                if (dispose)
                {
                    requestScope.Dispose();
                }
            }
        }

        /// <summary>
        /// Create a scoped request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <param name="prefixName">The prefix name.</param>
        /// <returns></returns>
        private static IServiceScope CreateRequestScope(this HttpRequest request, string prefixName)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            IServiceProvider rootContainer = request.ODataOptions().GetRouteServices(prefixName);
            IServiceScope scope = rootContainer.GetRequiredService<IServiceScopeFactory>().CreateScope();

            // Bind scoping request into the OData container.
            if (!string.IsNullOrEmpty(prefixName))
            {
                scope.ServiceProvider.GetRequiredService<HttpRequestScope>().HttpRequest = request;
            }

            return scope;
        }

        /// <summary>
        /// Gets the OData version from the request context.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <returns>The OData version.</returns>
        public static ODataVersion GetODataVersion(this HttpRequest request)
        {
            return request.ODataMaxServiceVersion() ??
                request.ODataMinServiceVersion() ??
                request.ODataServiceVersion() ??
                ODataVersionConstraint.DefaultODataVersion;
        }

        internal static ODataQueryOptions GetQueryOptions(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            ODataFeature feature = request.ODataFeature() as ODataFeature;

            return feature.QueryOptions;
        }

        internal static ODataVersion? ODataServiceVersion(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            return GetODataVersionFromHeader(request.Headers, ODataVersionConstraint.ODataServiceVersionHeader);
        }

        internal static ODataVersion? ODataMaxServiceVersion(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            return GetODataVersionFromHeader(request.Headers, ODataVersionConstraint.ODataMaxServiceVersionHeader);
        }

        internal static ODataVersion? ODataMinServiceVersion(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
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
