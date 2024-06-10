//-----------------------------------------------------------------------------
// <copyright file="HttpRequestExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
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
        private static readonly string ODataInstanceAnnotationContainerKey = "odataInstanceAnnotation_14802D58-69EF-4B28-9BDC-963D3648F06A";

        /// <summary>
        /// Returns the <see cref="IODataFeature"/> from the DI container.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <returns>The <see cref="IODataFeature"/> from the services container.</returns>
        public static IODataFeature ODataFeature(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            return request.HttpContext.ODataFeature();
        }

        /// <summary>
        /// Gets the <see cref="IODataBatchFeature"/> from the DI container.
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
        /// Set the top-level instance annotations for the request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <param name="instanceAnnotations">The instance annotations</param>
        public static HttpRequest SetInstanceAnnotations(this HttpRequest request, IDictionary<string, object> instanceAnnotations)
        {
            IODataFeature odataFeature = request.ODataFeature();

            // The last wins.
            odataFeature.RoutingConventionsStore[ODataInstanceAnnotationContainerKey] = instanceAnnotations;

            return request;
        }

        /// <summary>
        /// Get the top-level instance annotations for the request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <returns>null or top-level instance annotations.</returns>
        public static IDictionary<string, object> GetInstanceAnnotations(this HttpRequest request)
        {
            if (request == null)
            {
                return null;
            }

            IODataFeature odataFeature = request.ODataFeature();
            if (!odataFeature.RoutingConventionsStore.TryGetValue(ODataInstanceAnnotationContainerKey, out object annotations))
            {
                return null;
            }

            return annotations as IDictionary<string, object>;
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

            return request.ODataOptions()?.TimeZone;
        }

        /// <summary>
        /// Gets the boolean value indicating whether the non-dollar prefix query option.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <returns>True/false.</returns>
        public static bool IsNoDollarQueryEnable(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            return request.ODataOptions()?.EnableNoDollarQueryOptions ?? false;
        }

        /// <summary>
        /// Gets a value indicating if this is a count request.
        /// </summary>
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

            return request.GetRouteServices().GetRequiredService<ODataMessageReaderSettings>();
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

            return request.GetRouteServices().GetRequiredService<ODataMessageWriterSettings>();
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

            return request.GetRouteServices().GetRequiredService<IODataDeserializerProvider>();
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
            return request.GetETagHandler().CreateETag(properties, timeZone)?.ToString();
        }

        /// <summary>
        /// Gets the <see cref="IETagHandler"/> from the services container.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <returns>The <see cref="IETagHandler"/> from the services container.</returns>
        public static IETagHandler GetETagHandler(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            return request.GetRouteServices().GetService<IETagHandler>();
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
            if (request.ODataFeature().RoutePrefix == null)
            {
                return null;
            }

            // HTTP routes will not have chance to call CreateRequestContainer. We have to call it.
            return request.CreateRouteServices(request.ODataFeature().RoutePrefix);
        }

        /// <summary>
        /// Creates a request container that associates with the <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <param name="routePrefix">The route prefix for this request. Should match an entry in <see cref="ODataOptions.RouteComponents"/>.</param>
        /// <returns>The request container created.</returns>
        public static IServiceProvider CreateRouteServices(this HttpRequest request, string routePrefix)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            if (request.ODataFeature().Services != null)
            {
                throw Error.InvalidOperation(SRResources.RouteServicesAlreadyExist);
            }

            IServiceScope requestScope = request.CreateRequestScope(routePrefix);
            IServiceProvider requestContainer = requestScope.ServiceProvider;

            request.ODataFeature().RequestScope = requestScope;
            request.ODataFeature().Services = requestContainer;

            return requestContainer;
        }

        /// <summary>
        /// Removes the <see cref="ODataFeature.RequestScope"/> and <see cref="ODataFeature.Services"/> from 
        /// the <paramref name="request"/> and optionally disposes of the <see cref="ODataFeature.RequestScope"/>.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> instance to extend.</param>
        /// <param name="dispose">
        /// Specifies whether or not to dispose of the <see cref="ODataFeature.RequestScope"/>. Defaults to false.
        /// </param>
        public static void ClearRouteServices(this HttpRequest request, bool dispose = false)
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
        /// <param name="routePrefix">The route prefix for this request. Should match an entry in <see cref="ODataOptions.RouteComponents"/>.</param>
        /// <returns></returns>
        private static IServiceScope CreateRequestScope(this HttpRequest request, string routePrefix)
        {
            ODataOptions options = request.ODataOptions();

            IServiceProvider rootContainer = options.GetRouteServices(routePrefix);
            IServiceScope scope = rootContainer.GetRequiredService<IServiceScopeFactory>().CreateScope();

            // Bind scoping request into the OData container.
            if (!string.IsNullOrEmpty(routePrefix))
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
