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
using ODataQueryBuilder.Query;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// This defines some context information used to perform query composition.
    /// </summary>
    public class ODataQueryContext : ODataQueryFundamentalsContext
    {
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
            : base(model, elementClrType, path, new RequestContext())
        {
            // set RequestContext properties
        }

        /// <summary>
        /// Constructs an instance of <see cref="ODataQueryContext"/> with <see cref="IEdmModel" />, element EDM type,
        /// and <see cref="ODataPath" />.
        /// </summary>
        /// <param name="model">The EDM model the given EDM type belongs to.</param>
        /// <param name="elementType">The EDM type of the element of the collection being queried.</param>
        /// <param name="path">The parsed <see cref="ODataPath"/>.</param>
        public ODataQueryContext(IEdmModel model, IEdmType elementType, ODataPath path)
            : base(model, elementType, path, new RequestContext())
        {
            // set RequestContext properties
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
        /// Gets the given <see cref="DefaultQueryConfigurations"/>.
        /// </summary>
        public override DefaultQueryConfigurations DefaultQueryConfigurations
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

        internal DefaultQueryConfigurations GetDefaultQuerySettings()
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

        public override IComputeQueryValidator GetComputeQueryValidator()
        {
            return RequestContainer.GetService<ComputeQueryValidator>();
        }
    }
}
