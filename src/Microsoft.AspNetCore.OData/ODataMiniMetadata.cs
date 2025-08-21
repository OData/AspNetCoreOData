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
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData;

/// <summary>
/// Metadata that specifies the OData minimal API.
/// </summary>
// Or seperate them into pieces?
public class ODataMiniMetadata
{
    private IServiceProvider _serviceProvider = null;

    /// <summary>
    /// Gets or sets the model
    /// </summary>
    public IEdmModel Model { get; set; }

    /// <summary>
    /// Gets or sets a boolean value indicating to generate OData response.
    /// </summary>
    public bool IsODataFormat { get; set; }

    /// <summary>
    /// Gets or sets the path factory.
    /// </summary>
    public Func<HttpContext, Type, ODataPath> PathFactory { get; set; }

    /// <summary>
    /// Gets or sets the OData version.
    /// </summary>
    public ODataVersion Version
    {
        get => Options.Version;
        set => Options.SetVersion(value);
    }

    /// <summary>
    /// Gets or sets the base address factory.
    /// </summary>
    public Func<HttpContext, Uri> BaseAddressFactory { get; set; }

    /// <summary>
    /// Gets or sets the minimal options.
    /// </summary>
    public ODataMiniOptions Options { get; } = new ODataMiniOptions();

    /// <summary>
    /// Gets or sets the services
    /// </summary>
    public Action<IServiceCollection> Services { get; set; }

    /// <summary>
    /// Gets or sets the service provider associated to this metadata.
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
            }

            return _serviceProvider;
        }
        set => _serviceProvider = value;
    }

    internal IServiceProvider BuildRouteContainer()
    {
        IServiceCollection services = new ServiceCollection();

        // Inject the core odata services.
        services.AddDefaultODataServices(Version);

        // Inject the default query configuration from this options.
        services.AddSingleton(sp => this.Options.QueryConfigurations);

        // Inject the default Web API OData services.
        services.AddDefaultWebApiServices(Version);

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