//-----------------------------------------------------------------------------
// <copyright file="ODataEndpointConventionBuilderExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData;

/// <summary>
/// Extension methods for adding <see cref="IODataQueryEndpointFilter"/> to a route handler.
/// </summary>
public static class ODataEndpointConventionBuilderExtensions
{
    /// <summary>
    /// Adds a <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/> that matches HTTP GET requests to get the OData metadata.
    /// It uses the Request.Header.ContentType or $format to identify whether it's CSDL-XML or CSDL-JSON.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="model">The related Edm model.</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public static IEndpointConventionBuilder MapODataServiceDocument(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string pattern,
        IEdmModel model)
        => endpoints.MapGet(pattern, () => ODataResultExtensions.OData(model.GenerateServiceDocument()));

    /// <summary>
    /// Adds a <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/> that matches HTTP GET requests to get the OData metadata.
    /// It uses the Request.Header.ContentType or $format to identify whether it's CSDL-XML or CSDL-JSON.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="model">The related Edm model.</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public static IEndpointConventionBuilder MapODataMetadata(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string pattern,
        IEdmModel model)
        => endpoints.MapODataMetadata(pattern, model, new ODataMetadataHandler());

    /// <summary>
    /// Adds a <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/> that matches HTTP GET requests to get the OData metadata.
    /// It uses the Request.Header.ContentType or $format to identify whether it's CSDL-XML or CSDL-JSON.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="model">The related Edm model.</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public static IEndpointConventionBuilder MapODataMetadata1(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string pattern,
        IEdmModel model)
        => endpoints.MapGet(pattern, () => ODataResultExtensions.OData(model));

    /// <summary>
    /// Adds a <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/> that matches HTTP GET requests to get the OData metadata based on <see cref="IODataMetadataHandler"/>.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="model">The related Edm model.</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public static IEndpointConventionBuilder MapODataMetadata(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string pattern,
        IEdmModel model,
        IODataMetadataHandler metadataHandler)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(metadataHandler);

        // Let's focus on the metadata CSDL only now.
        return endpoints.MapGet(pattern, async httpContext => await metadataHandler.InvokeAsync(httpContext, model));
    }

    // It's better to add ODataQueryFilter as early as possible,
    // So, we can do the validation as early as possible,
    // but will do the applyTo as later as possbile.
    public static TBuilder AddODataQueryEndpointFilter<TBuilder>(this TBuilder builder, IODataQueryEndpointFilter queryFilter) where TBuilder : IEndpointConventionBuilder =>
        builder.AddEndpointFilter(queryFilter);

    public static TBuilder AddODataQueryEndpointFilter<TBuilder, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFilterType>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
        where TFilterType : IODataQueryEndpointFilter =>
        builder.AddEndpointFilter<TBuilder, TFilterType>();

    /// <summary>
    /// Registers the default OData query filter onto the route handler.
    /// </summary>
    /// <param name="builder">The <see cref="RouteHandlerBuilder"/>.</param>
    /// <param name="validationSetup">The action to configure validataion settings.</param>
    /// <param name="querySetup">The action to configure query settings.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customize the route handler.</returns>
    public static RouteHandlerBuilder AddODataQueryEndpointFilter(this RouteHandlerBuilder builder,
        Action<ODataValidationSettings> validationSetup = default,
        Action<ODataQuerySettings> querySetup = default)
        => builder.AddODataQueryEndpointFilterInternal(new ODataQueryEndpointFilter(), validationSetup, querySetup);

    /// <summary>
    /// Registers the default OData query filter onto the route group.
    /// </summary>
    /// <param name="builder">The <see cref="RouteGroupBuilder"/>.</param>
    /// <param name="validationSetup">The action to configure validataion settings.</param>
    /// <param name="querySetup">The action to configure query settings.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customize the route handler.</returns>
    public static RouteGroupBuilder AddODataQueryEndpointFilter(this RouteGroupBuilder builder,
        Action<ODataValidationSettings> validationSetup = default,
        Action<ODataQuerySettings> querySetup = default) =>
        builder.AddODataQueryEndpointFilterInternal(new ODataQueryEndpointFilter(), validationSetup, querySetup);

    private static TBuilder AddODataQueryEndpointFilterInternal<TBuilder>(this TBuilder builder, ODataQueryEndpointFilter queryFilter,
        Action<ODataValidationSettings> validationSetup,
        Action<ODataQuerySettings> querySetup)
        where TBuilder : IEndpointConventionBuilder
    {
        validationSetup?.Invoke(queryFilter.ValidationSettings);
        querySetup?.Invoke(queryFilter.QuerySettings);
        builder.AddEndpointFilter(queryFilter);
        return builder;
    }

    /// <summary>
    /// Registers an OData query filter onto the route handler.
    /// </summary>
    /// <param name="builder">The <see cref="RouteHandlerBuilder"/>.</param>
    /// <param name="queryFilter">The <see cref="IODataQueryEndpointFilter"/>.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customize the route handler.</returns>
    public static RouteHandlerBuilder AddODataQueryEndpointFilter(this RouteHandlerBuilder builder, IODataQueryEndpointFilter queryFilter)=>
        builder.AddEndpointFilter(queryFilter);

    /// <summary>
    /// Registers an OData query filter onto the route group.
    /// </summary>
    /// <param name="builder">The <see cref="RouteGroupBuilder"/>.</param>
    /// <param name="queryFilter">The <see cref="IODataQueryEndpointFilter"/>.</param>
    /// <returns>A <see cref="RouteGroupBuilder"/> that can be used to further customize the route handler.</returns>
    public static RouteGroupBuilder AddODataQueryEndpointFilter(this RouteGroupBuilder builder, IODataQueryEndpointFilter queryFilter) =>
        builder.AddEndpointFilter(queryFilter);

    /// <summary>
    /// Registers an OData query filter of type <typeparamref name="TFilterType"/> onto the route handler.
    /// </summary>
    /// <typeparam name="TFilterType">The type of the <see cref="IODataQueryEndpointFilter"/> to register.</typeparam>
    /// <param name="builder">The <see cref="RouteHandlerBuilder"/>.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customize the route handler.</returns>
    public static RouteHandlerBuilder AddODataQueryEndpointFilter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFilterType>(this RouteHandlerBuilder builder)
        where TFilterType : IODataQueryEndpointFilter =>
        builder.AddEndpointFilter<TFilterType>();

    /// <summary>
    /// Registers an OData query filter of type <typeparamref name="TFilterType"/> onto the route group.
    /// </summary>
    /// <typeparam name="TFilterType">The type of the <see cref="IODataQueryEndpointFilter"/> to register.</typeparam>
    /// <param name="builder">The <see cref="RouteGroupBuilder"/>.</param>
    /// <returns>A <see cref="RouteGroupBuilder"/> that can be used to further customize the route handler.</returns>
    public static RouteGroupBuilder AddODataQueryEndpointFilter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFilterType>(this RouteGroupBuilder builder)
        where TFilterType : IODataQueryEndpointFilter =>
        builder.AddEndpointFilter<TFilterType>();


    public static TBuilder AddODataQueryEndpointFilterFactory<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        return builder.AddODataQueryEndpointFilterFactory(new ODataQueryEndpointFilter());
    }

    public static TBuilder AddODataQueryEndpointFilterFactory<TBuilder>(this TBuilder builder, IODataQueryEndpointFilter queryFilter) where TBuilder : IEndpointConventionBuilder
    {
        return builder.AddEndpointFilterFactory((filterFactoryContext, next) =>
        {
            MethodInfo methodInfo = filterFactoryContext.MethodInfo;

            return async invocationContext =>
            {
                ILoggerFactory loggerFactory = invocationContext.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                ILogger logger = loggerFactory.CreateLogger("ODataQuery");
                logger.LogInformation("Starting OdataQuery. ..");

                var odataFilterContext = new ODataQueryFilterInvocationContext { MethodInfo = methodInfo, InvocationContext = invocationContext };

                await queryFilter.OnFilterExecutingAsync(odataFilterContext);

                var result = await next(invocationContext);

                logger.LogInformation("Ending OdataQuery. ..");

                return await queryFilter.OnFilterExecutedAsync(result, odataFilterContext);
            };
        });
    }

    public static IServiceProvider BuildServiceProvider(Action<IServiceCollection> servicesSetup)
    {
        return null;
    }

    /// <summary>
    /// Adds an OData Edm model metadata to <see cref="Endpoint.Metadata" /> associated with the current endpoint.
    /// This method typically is used in Minimal API scenarios.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <param name="model">The Edm model.</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public static TBuilder WithModel<TBuilder>(this TBuilder builder, IEdmModel model) where TBuilder : IEndpointConventionBuilder
        => builder.WithMetadata(new EdmModelMetadata(model));

    public static TBuilder WithModel1<TBuilder>(this TBuilder builder, IEdmModel model, Action<IServiceCollection> setupAction) where TBuilder : IEndpointConventionBuilder
        => builder.WithMetadata(new EdmModelMetadata(model));

    public static TBuilder WithOData<TBuilder>(this TBuilder builder, ODataMiniMetadata metadata) where TBuilder : IEndpointConventionBuilder
    {
        if (metadata.IsODataFormat)
        {
            builder.AddEndpointFilter(async (invocationContext, next) =>
            {
                object result = await next(invocationContext);

                // If it's null or if it's already the ODataResult, simply do nothing
                if (result is null || result is ODataResult)
                {
                    return result;
                }

                // Maybe we have a scenario like:
                // Enable OData formatter in Group first,
                // Then Disable OData formatter for a certain Routehandler.
                var endpoint = invocationContext.HttpContext.GetEndpoint();
                ODataMiniMetadata metadata = endpoint?.Metadata?.GetMetadata<ODataMiniMetadata>();
                if (metadata is not null && metadata.IsODataFormat)
                {
                    return new ODataResult(result/*, options*/);
                }

                return result;
            });
        }

        //builder.AddEndpointFilterFactory((filterFactoryContext, next) =>
        //{
        //    MethodInfo methodInfo = filterFactoryContext.MethodInfo;

        //    return async invocationContext =>
        //    {
        //        ILoggerFactory loggerFactory = invocationContext.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
        //        ILogger logger = loggerFactory.CreateLogger("ODataQuery");
        //        logger.LogInformation("Starting OdataQuery. ..");

        //        var odataFilterContext = new ODataQueryFilterInvocationContext { MethodInfo = methodInfo, InvocationContext = invocationContext };

        //        await queryFilter.OnFilterExecutingAsync(odataFilterContext);

        //        var result = await next(invocationContext);

        //        logger.LogInformation("Ending OdataQuery. ..");

        //        return await queryFilter.OnFilterExecutedAsync(result, odataFilterContext);
        //    };

        //    return invocationContext => next(invocationContext);
        //});

        builder.Add(b =>
        {
            // Remove the existing, the last wins
            var existings = b.Metadata.OfType<ODataMiniMetadata>().ToList();
            foreach (var m in existings)
            {
                b.Metadata.Remove(m);
            }

            b.Metadata.Add(metadata);
        });

        return builder;
    }

    public static TBuilder AddOData<TBuilder>(this TBuilder builder, Action<ODataMiniMetadata> setupAction) where TBuilder : IEndpointConventionBuilder
    {
        
        ODataMiniMetadata options = new ODataMiniMetadata();
        setupAction?.Invoke(options);

        // builder.Finally(c => c.Metadata)
        return builder.WithMetadata(options);
    }

    //public static TBuilder WithOData<TBuilder>(this TBuilder builder, Action<ODataMiniOptions> setupAction = null) where TBuilder : IEndpointConventionBuilder
    //{
    //    builder.AddEndpointFilter(async (invocationContext, next) =>
    //    {
    //        object result = await next(invocationContext);

    //        // If it's null or if it's already the ODataResult, simply do nothing
    //        if (result is null || result is ODataResult)
    //        {
    //            return result;
    //        }

    //        ODataMiniOptions options = invocationContext.HttpContext.RequestServices.GetService<IOptions<ODataMiniOptions>>()?.Value;
    //        if (options is null)
    //        {
    //            options = new ODataMiniOptions();
    //        }
    //        setupAction?.Invoke(options);

    //        return new ODataResult(result, options);
    //    });

    //    builder.Add(b => b.ApplicationServices)


    //    builder.Finally(c => AddAndConfigureODataForEndpoint(c));
    //    return builder.WithMetadata(options);
    //}

    public static TBuilder WithOData<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        => builder.WithOData(opt => opt.IsODataFormat = true);

    // Provide an EdmModel for endpoint
    public static TBuilder WithOData<TBuilder>(this TBuilder builder, IEdmModel model, bool isODataFormat = false) where TBuilder : IEndpointConventionBuilder
        => builder.WithOData(opt =>
        {
            opt.Model = model;
            opt.IsODataFormat = isODataFormat;
        });


    public static TBuilder WithOData<TBuilder>(this TBuilder builder, Action<ODataMiniMetadata> setupAction = null) where TBuilder : IEndpointConventionBuilder
    {
        // builder.WithOrder
        builder.AddEndpointFilter(async (invocationContext, next) =>
        {
            object result = await next(invocationContext);

            // If it's null or if it's already the ODataResult, simply do nothing
            if (result is null || result is ODataResult)
            {
                return result;
            }

            var endpoint = invocationContext.HttpContext.GetEndpoint();
            ODataMiniMetadata metadata = endpoint?.Metadata?.GetMetadata<ODataMiniMetadata>();
            if (metadata is not null && metadata.IsODataFormat)
            {
                return new ODataResult(result/*, options*/);
            }

            return result;
        });

        builder.Finally(b =>
        {
            ODataMiniMetadata metadata = b.Metadata.OfType<ODataMiniMetadata>().FirstOrDefault();
            if (metadata is not null)
            {
                b.Metadata.Remove(metadata);
            }
            else 
            {
                metadata = new ODataMiniMetadata();
            }

            // retrieve the global configuration
            ODataMiniOptions miniOptions = b.ApplicationServices?.GetService<IOptions<ODataMiniOptions>>()?.Value;

            if (miniOptions is not null)
            {
                metadata.UpdateOptions(miniOptions);
            }

            setupAction?.Invoke(metadata);
            b.Metadata.Add(metadata);
        });

        return builder;
    }

    public static TBuilder WithOData2<TBuilder>(this TBuilder builder,
        Action<ODataMiniMetadata> metadataSetup = null,
        Action<IServiceCollection> servicesSetup = null) where TBuilder : IEndpointConventionBuilder
    {
        // builder.WithOrder
        builder.AddEndpointFilter(async (invocationContext, next) =>
        {
            object result = await next(invocationContext);

            // If it's null or if it's already the ODataResult, simply do nothing
            if (result is null || result is ODataResult)
            {
                return result;
            }

            var endpoint = invocationContext.HttpContext.GetEndpoint();
            ODataMiniMetadata metadata = endpoint?.Metadata?.GetMetadata<ODataMiniMetadata>();
            if (metadata is not null && metadata.IsODataFormat)
            {
                return new ODataResult(result/*, options*/);
            }

            return result;
        });

        builder.Finally(b =>
        {
            ODataMiniMetadata metadata = b.Metadata.OfType<ODataMiniMetadata>().FirstOrDefault();
            if (metadata is not null)
            {
                b.Metadata.Remove(metadata);
            }
            else
            {
                metadata = new ODataMiniMetadata();
            }

            // retrieve the global configuration
            ODataMiniOptions miniOptions = b.ApplicationServices?.GetService<IOptions<ODataMiniOptions>>()?.Value;

            if (miniOptions is not null)
            {
                metadata.UpdateOptions(miniOptions);
            }

            // call update route container before the 'metadata setup' so it's ok to update/retrieve the service in metadata setup.
            metadata.UpdateRouteContainer(servicesSetup);

            metadataSetup?.Invoke(metadata);

            b.Metadata.Add(metadata);
        });

        return builder;
    }


    // Be noted: 
    // Model is provided using WithModel()
    // 
    public static TBuilder AddODataResult<TBuilder>(this TBuilder builder, Action<ODataMiniOptions> setupAction = null) where TBuilder : IEndpointConventionBuilder
    {
       // builder.WithOrder
        builder.AddEndpointFilter(async (invocationContext, next) =>
        {
            object result = await next(invocationContext);

            // If it's null or if it's already the ODataResult, simply do nothing
            if (result is null || result is ODataResult)
            {
                return result;
            }

            //ODataMiniOptions options = invocationContext.HttpContext.RequestServices.GetService<IOptions<ODataMiniOptions>>()?.Value;
            //if (options is null)
            //{
            //    options = new ODataMiniOptions();
            //}
            //setupAction?.Invoke(options);
            var endpoint = invocationContext.HttpContext.GetEndpoint();
            ODataMiniMetadata odataMetadata = endpoint?.Metadata?.GetMetadata<ODataMiniMetadata>();
            if (odataMetadata is not null && odataMetadata.IsODataFormat)
            {
                return new ODataResult(result/*, options*/);
            }

            return result;
        });

     //   builder.Add(b => b.ApplicationServices)


        builder.Finally(c => AddAndConfigureODataForEndpoint(c, setupAction));
        return builder;
    }

    private static void AddAndConfigureODataForEndpoint(EndpointBuilder endpointBuilder, Action<ODataMiniOptions> setupAction = null)
    {
        // retrieve the global configuration
        ODataMiniOptions options = endpointBuilder.ApplicationServices.GetService<IOptions<ODataMiniOptions>>()?.Value;
        if (options is null)
        {
            options = new ODataMiniOptions();
        }
        else
        {
            //options = options.Clone();
        }

        setupAction?.Invoke(options);

        endpointBuilder.Metadata.Add(options);

        if (!endpointBuilder.Metadata.Any(m => m is IODataServiceProvider))
        {
            endpointBuilder.Metadata.Add(new ODataServiceProvider());
        }

        if (!endpointBuilder.Metadata.Any(m => m is IEdmModelMetadata))
        {
            // Add an empty model metadata, so we can override it
            endpointBuilder.Metadata.Add(new EdmModelMetadata());
        }
    }

    public static TBuilder WithODataResult<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        // builder.WithOrder
        builder.AddEndpointFilter(async (invocationContext, next) =>
        {
            object result = await next(invocationContext);

            // If it's null or if it's already the ODataResult, simply do nothing
            if (result is null || result is ODataResult)
            {
                return result;
            }

            // Maybe we have a scenario like:
            // Enable OData result in app.MapGroup first,
            // Then Disable OData result for a certain Routehandler.
            var endpoint = invocationContext.HttpContext.GetEndpoint();
            ODataMiniMetadata odataMetadata = endpoint?.Metadata?.GetMetadata<ODataMiniMetadata>();
            if (odataMetadata is not null && odataMetadata.IsODataFormat)
            {
                return new ODataResult(result/*, options*/);
            }

            return result;
        });

        builder.Add(b => ConfigureODataMetadata(b, m => m.IsODataFormat = true));
        return builder;
    }

    public static TBuilder WithODataOptions<TBuilder>(this TBuilder builder, Action<ODataMiniOptions> setupAction) where TBuilder : IEndpointConventionBuilder
    {
        builder.Add(b => ConfigureODataMetadata(b, m =>
        {
            ODataMiniOptions options = b.ApplicationServices.GetService<IOptions<ODataMiniOptions>>()?.Value;
            if (options is not null)
            {
                m.Options.Update(options);
            }

            setupAction.Invoke(m.Options);
        }));

        return builder;
    }

    public static TBuilder WithODataServices<TBuilder>(this TBuilder builder, Action<IServiceCollection> services) where TBuilder : IEndpointConventionBuilder
    {
        builder.Add(b => ConfigureODataMetadata(b, m => m.Services = services));
        return builder;
    }

    public static TBuilder WithODataModel<TBuilder>(this TBuilder builder, IEdmModel model) where TBuilder : IEndpointConventionBuilder
    {
        builder.Add(b => ConfigureODataMetadata(b, m => m.Model = model));
        return builder;
    }

    public static TBuilder WithODataBaseAddressFactory<TBuilder>(this TBuilder builder, Func<HttpContext, Uri> baseAddressFactory) where TBuilder : IEndpointConventionBuilder
    {
        builder.Add(b => ConfigureODataMetadata(b, m => m.BaseAddressFactory = baseAddressFactory));
        return builder;
    }

    public static TBuilder WithODataVersion<TBuilder>(this TBuilder builder, ODataVersion version) where TBuilder : IEndpointConventionBuilder
    {
        builder.Add(b => ConfigureODataMetadata(b, m => m.Version = version));
        return builder;
    }

    internal static void ConfigureODataMetadata(EndpointBuilder endpointBuilder, Action<ODataMiniMetadata> setupAction)
    {
        // retrieve the previous configuration
        var metadata = endpointBuilder.Metadata.OfType<ODataMiniMetadata>().FirstOrDefault();
        if (metadata is null)
        {
            metadata = new ODataMiniMetadata();

            ODataMiniOptions options = endpointBuilder.ApplicationServices.GetService<IOptions<ODataMiniOptions>>()?.Value;
            if (options is not null)
            {
                metadata.Options.Update(options);
            }

            endpointBuilder.Metadata.Add(metadata);
        }

        setupAction.Invoke(metadata);
    }
}

public interface IODataServiceProvider
{
    IServiceProvider ServiceProvider { get; }
}

public class ODataServiceProvider : IODataServiceProvider
{
    public IServiceProvider ServiceProvider => throw new NotImplementedException();
}




public class ODataMiniMetadata1 : IODataMiniMetadata
{
    // True: OData payload
    // False: Normal JSON payload
    public bool IsODataFormat { get; set; }

    public IEdmModel EdmModel { get; set; }

    public IServiceProvider ServiceProvider { get; set; }

    public IEdmModel Model => throw new NotImplementedException();
}

public interface IODataMiniMetadata
{
    IEdmModel Model { get; }

    bool IsODataFormat { get; set; }
}

public class ODataMiniMetadata
{
    private IServiceProvider _serviceProvider = null;
    //private readonly List<Action<IServiceCollection>> _conventions = new();

    public IEdmModel Model { get; set; }

    public bool IsODataFormat { get; set; }

    public Func<HttpContext, Type, ODataPath> PathFactory { get; set; }

    public ODataVersion Version { get; set; } = ODataVersionConstraint.DefaultODataVersion;

    public Func<HttpContext, Uri> BaseAddressFactory { get; set; }

    public ODataMiniOptions Options { get; } = new ODataMiniOptions();

    //internal bool IsReadOnly { get; set; } = false;

    public Action<IServiceCollection> Services { get; set; }

    public IServiceProvider ServiceProvider
    {
        get
        {
            if (_serviceProvider == null)
            {
                _serviceProvider = BuildRouteContainer();
    //               IsReadOnly = true;
            }

            return _serviceProvider;
        }
    }

    //public void Add(Action<IServiceCollection> convention)
    //{
    //    if (IsReadOnly)
    //    {
    //        throw new InvalidOperationException("Services cannot be registered after running.");
    //    }

    //    _conventions.Add(convention);
    //}

    internal ODataMiniMetadata UpdateOptions(ODataMiniOptions other)
    {
        Options.QueryConfigurations.UpdateAll(other.QueryConfigurations);
        Options.EnableNoDollarQueryOptions = other.EnableNoDollarQueryOptions;
        Options.EnableCaseInsensitive = other.EnableCaseInsensitive;
        return this;
    }

    internal void UpdateRouteContainer(Action<IServiceCollection> servicesSetup = null)
    {
        IServiceCollection services = new ServiceCollection();

        // Inject the core odata services.
        services.AddDefaultODataServices(Version);

        // Inject the default query configuration from this options.
        services.AddSingleton(sp => this.Options.QueryConfigurations);

        // Inject the default Web API OData services.
        services.AddDefaultWebApiServices();

        // Set Uri resolver to by default enabling unqualified functions/actions and case insensitive match.
        services.AddSingleton<ODataUriResolver>(sp =>
            new UnqualifiedODataUriResolver
            {
                EnableCaseInsensitive = this.Options.EnableCaseInsensitive, // by default to enable case insensitive
                EnableNoDollarQueryOptions = this.Options.EnableNoDollarQueryOptions // retrieve it from global setting
            });

        // Inject the Edm model.
        // From Current ODL implement, such injection only be used in reader and writer if the input
        // model is null.
        services.AddSingleton(sp => Model);

        // Inject the customized services.
        //foreach (var setupConfig in _conventions)
        //{
        //    setupConfig?.Invoke(services);
        //}
        servicesSetup?.Invoke(services);

       // ServiceProvider = services.BuildServiceProvider();
    }

    internal IServiceProvider BuildRouteContainer()
    {
        IServiceCollection services = new ServiceCollection();

        // Inject the core odata services.
        services.AddDefaultODataServices(Version);

        // Inject the default query configuration from this options.
        services.AddSingleton(sp => this.Options.QueryConfigurations);

        // Inject the default Web API OData services.
        services.AddDefaultWebApiServices();

        // Set Uri resolver to by default enabling unqualified functions/actions and case insensitive match.
        services.AddSingleton<ODataUriResolver>(sp =>
            new UnqualifiedODataUriResolver
            {
                EnableCaseInsensitive = this.Options.EnableCaseInsensitive, // by default to enable case insensitive
                EnableNoDollarQueryOptions = this.Options.EnableNoDollarQueryOptions // retrieve it from global setting
            });

        // Inject the Edm model.
        // From Current ODL implement, such injection only be used in reader and writer if the input
        // model is null.
        // How about the model is null?
        services.AddSingleton(sp => Model);

        // Inject the customized services.
        //foreach (var setupConfig in _conventions)
        //{
        //    setupConfig?.Invoke(services);
        //}
        Services?.Invoke(services);

        return services.BuildServiceProvider();
    }

    internal static ODataPath DefaultPathFactory(HttpContext context, Type elementType)
    {
        IEdmModel model = context.GetOrCreateEdmModel(elementType);
        IEdmType edmType = model.GetEdmType(elementType);

        var entitySet = model.EntityContainer?.EntitySets().FirstOrDefault(e => e.EntityType == edmType);
        if (entitySet != null)
        {
            return new ODataPath(new EntitySetSegment(entitySet));
        }
        else
        {
            entitySet = new EdmEntitySet(model.EntityContainer, elementType.Name, edmType as IEdmEntityType);
            return new ODataPath(new EntitySetSegment(entitySet));
        }
    }
}