using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.Authorization
{

    /// <summary>
    /// Provides authorization extensions for <see cref="IServiceCollection"/>
    /// </summary>
    public static class ODataAuthorizationServiceCollectionExtensions
    {
        /// <summary>
        /// Adds OData model-based authorization services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configureOptions">Action to configure the authorization options</param>
        /// <returns></returns>
        public static IServiceCollection AddODataAuthorization(this IServiceCollection services, Action<ODataAuthorizationOptions> configureOptions = null)
        {
            var options = new ODataAuthorizationOptions();
            configureOptions?.Invoke(options);
            services.AddSingleton<IAuthorizationHandler, ODataAuthorizationHandler>(_ => new ODataAuthorizationHandler(options.ScopesFinder));
            return services;
        }
    }
}
