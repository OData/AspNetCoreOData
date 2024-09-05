//-----------------------------------------------------------------------------
// <copyright file="ODataQueryContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query;

/// <summary>
/// This defines some context information used to perform query composition.
/// </summary>
public class ODataQueryContext
{
    internal static readonly ODataUriResolver DefaultCaseInsensitiveResolver = new ODataUriResolver { EnableCaseInsensitive = true };

    private DefaultQueryConfigurations _defaultQueryConfigurations;

    /// <summary>
    /// Constructs an instance of <see cref="ODataQueryContext"/> with <see cref="IEdmModel" />, element CLR type,
    /// and <see cref="ODataPath" />.
    /// </summary>
    /// <param name="model">The EdmModel that includes the <see cref="IEdmType"/> corresponding to
    /// the given <paramref name="elementClrType"/>.</param>
    /// <param name="elementClrType">The CLR type of the element of the collection being queried.</param>
    /// <param name="path">The parsed <see cref="ODataPath"/>.</param>
    /// <remarks>
    /// This is a public constructor used for stand-alone scenario; in this case, the services
    /// container may not be present.
    /// </remarks>
    public ODataQueryContext(IEdmModel model, Type elementClrType, ODataPath path)
    {
        if (model == null)
        {
            throw Error.ArgumentNull(nameof(model));
        }

        if (elementClrType == null)
        {
            throw Error.ArgumentNull(nameof(elementClrType));
        }

        ElementType = model.GetEdmTypeReference(elementClrType)?.Definition;

        if (ElementType == null)
        {
            throw Error.Argument(nameof(elementClrType), SRResources.ClrTypeNotInModel, elementClrType.FullName);
        }

        ElementClrType = elementClrType;
        Model = model;
        Path = path;
        NavigationSource = GetNavigationSource(Model, ElementType, path);
        GetPathContext();
    }

    /// <summary>
    /// Constructs an instance of <see cref="ODataQueryContext"/> with <see cref="IEdmModel" />, element EDM type,
    /// and <see cref="ODataPath" />.
    /// </summary>
    /// <param name="model">The EDM model the given EDM type belongs to.</param>
    /// <param name="elementType">The EDM type of the element of the collection being queried.</param>
    /// <param name="path">The parsed <see cref="ODataPath"/>.</param>
    public ODataQueryContext(IEdmModel model, IEdmType elementType, ODataPath path)
    {
        if (model == null)
        {
            throw Error.ArgumentNull(nameof(model));
        }

        if (elementType == null)
        {
            throw Error.ArgumentNull(nameof(elementType));
        }

        Model = model;
        ElementType = elementType;
        Path = path;
        NavigationSource = GetNavigationSource(Model, ElementType, path);
        GetPathContext();
    }

    internal ODataQueryContext(IEdmModel model, Type elementClrType)
        : this(model, elementClrType, path: null)
    {
    }

    internal ODataQueryContext(IEdmModel model, IEdmType elementType)
        : this(model, elementType, path: null)
    {
    }

    internal ODataQueryContext()
    { }

    /// <summary>
    /// Gets the given <see cref="DefaultQueryConfigurations"/>.
    /// </summary>
    public DefaultQueryConfigurations DefaultQueryConfigurations
    {
        get
        {
            if (_defaultQueryConfigurations == null)
            {
                _defaultQueryConfigurations = RequestContainer == null
                    ? GetDefaultQuerySettings()
                    : RequestContainer.GetRequiredService<DefaultQueryConfigurations>();
            }

            return _defaultQueryConfigurations;
        }
    }

    /// <summary>
    /// Gets the given <see cref="IEdmModel"/> that contains the EntitySet.
    /// </summary>
    public IEdmModel Model { get; internal set; }

    /// <summary>
    /// Gets the <see cref="IEdmType"/> of the element.
    /// </summary>
    public IEdmType ElementType { get; private set; }

    /// <summary>
    /// Gets the <see cref="IEdmNavigationSource"/> that contains the element.
    /// </summary>
    public IEdmNavigationSource NavigationSource { get; private set; }

    /// <summary>
    /// Gets the CLR type of the element.
    /// </summary>
    public Type ElementClrType { get; internal set; }

    /// <summary>
    /// Gets the <see cref="ODataPath"/>.
    /// </summary>
    public ODataPath Path { get; private set; }

    /// <summary>
    /// Gets the request container.
    /// </summary>
    /// <remarks>
    /// The services container may not be present. See the constructor in this file for
    /// use in stand-alone scenarios.
    /// </remarks>
    public IServiceProvider RequestContainer { get; internal set; }

    internal HttpRequest Request { get; set; }

    internal IEdmProperty TargetProperty { get; set; }

    internal IEdmStructuredType TargetStructuredType { get; set; }

    internal string TargetName { get; set; }

    internal ODataValidationSettings ValidationSettings { get; set; }

    private static IEdmNavigationSource GetNavigationSource(IEdmModel model, IEdmType elementType, ODataPath odataPath)
    {
        Contract.Assert(model != null);
        Contract.Assert(elementType != null);

        IEdmNavigationSource navigationSource = (odataPath != null) ? odataPath.GetNavigationSource() : null;
        if (navigationSource != null)
        {
            return navigationSource;
        }

        IEdmEntityContainer entityContainer = model.EntityContainer;
        if (entityContainer == null)
        {
            return null;
        }

        List<IEdmEntitySet> matchedNavigationSources =
            entityContainer.EntitySets().Where(e => e.EntityType == elementType).ToList();

        return (matchedNavigationSources.Count != 1) ? null : matchedNavigationSources[0];
    }

    private void GetPathContext()
    {
        if (Path != null)
        {
            (TargetProperty, TargetStructuredType, TargetName) = Path.GetPropertyAndStructuredTypeFromPath();
        }
        else
        {
            TargetStructuredType = ElementType as IEdmStructuredType;
        }
    }

    private DefaultQueryConfigurations GetDefaultQuerySettings()
    {
        if (Request is null)
        {
            return new DefaultQueryConfigurations();
        }

        IOptions<ODataOptions> odataOptions = Request.HttpContext?.RequestServices?.GetService<IOptions<ODataOptions>>();
        if (odataOptions is  null || odataOptions.Value is null)
        {
            return new DefaultQueryConfigurations();
        }

        return odataOptions.Value.QueryConfigurations;
    }
}
