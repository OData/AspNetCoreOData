//-----------------------------------------------------------------------------
// <copyright file="ODataMiniMetadata.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData;

/// <summary>
/// Metadata that specifies the OData minimal API.
/// Or seperate them into pieces?
/// </summary>
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

    /// <summary>
    /// Gets the service provider associated to this metadata.
    /// Be noted, it seems we build and cache the service provider per endpoint.
    /// If it's over-built, let's figure out a better solution for this.
    /// </summary>
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
        // From Current ODL implement, such injection only be used in reader and writer.
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