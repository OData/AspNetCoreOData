﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// This defines some context information used to perform query composition.
    /// </summary>
    public class ODataQueryContext
    {
        private DefaultQuerySettings _defaultQuerySettings;
        private ODataQueryableOptions _queryableOptions;

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
                throw Error.ArgumentNull("model");
            }

            if (elementClrType == null)
            {
                throw Error.ArgumentNull("elementClrType");
            }

            ElementType = model.GetTypeMappingCache().GetEdmType(elementClrType, model)?.Definition;

            if (ElementType == null)
            {
                throw Error.Argument("elementClrType", SRResources.ClrTypeNotInModel, elementClrType.FullName);
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
                throw Error.ArgumentNull("model");
            }
            if (elementType == null)
            {
                throw Error.ArgumentNull("elementType");
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

        /// <summary>
        /// Gets the given <see cref="DefaultQuerySettings"/>.
        /// </summary>
        public DefaultQuerySettings DefaultQuerySettings
        {
            get
            {
                if (_defaultQuerySettings == null)
                {
                    _defaultQuerySettings = RequestContainer == null
                        ? GetDefaultQuerySettings()
                        : RequestContainer.GetRequiredService<DefaultQuerySettings>();
                }

                return _defaultQuerySettings;
            }
        }

        /// <summary>
        /// Gets the given <see cref="ODataQueryableOptions"/>.
        /// TODO: it seems this one is never been used??
        /// </summary>
        public ODataQueryableOptions QueryableOptions
        {
            get
            {
                if (_queryableOptions == null)
                {
                    _queryableOptions = RequestContainer == null
                        ? new ODataQueryableOptions()
                        : RequestContainer.GetRequiredService<IOptions<ODataQueryableOptions>>().Value;
                }

                return _queryableOptions;
            }
        }

        /// <summary>
        /// Gets the given <see cref="IEdmModel"/> that contains the EntitySet.
        /// </summary>
        public IEdmModel Model { get; private set; }

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

        internal IEdmProperty TargetProperty { get; private set; }

        internal IEdmStructuredType TargetStructuredType { get; private set; }

        internal string TargetName { get; private set; }

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
                entityContainer.EntitySets().Where(e => e.EntityType() == elementType).ToList();

            return (matchedNavigationSources.Count != 1) ? null : matchedNavigationSources[0];
        }

        private void GetPathContext()
        {
            if (Path != null)
            {
                IEdmProperty property;
                IEdmStructuredType structuredType;
                string name;
                EdmHelpers.GetPropertyAndStructuredTypeFromPath(
                    Path,
                    out property,
                    out structuredType,
                    out name);

                TargetProperty = property;
                TargetStructuredType = structuredType;
                TargetName = name;
            }
            else
            {
                TargetStructuredType = ElementType as IEdmStructuredType;
            }
        }

        private DefaultQuerySettings GetDefaultQuerySettings()
        {
            if (Request is null)
            {
                return new DefaultQuerySettings();
            }

            IOptions<ODataOptions> odataOptions = Request.HttpContext?.RequestServices?.GetService<IOptions<ODataOptions>>();
            if (odataOptions is  null || odataOptions.Value is null)
            {
                return new DefaultQuerySettings();
            }

            return odataOptions.Value.BuildDefaultQuerySettings();
        }
    }
}
