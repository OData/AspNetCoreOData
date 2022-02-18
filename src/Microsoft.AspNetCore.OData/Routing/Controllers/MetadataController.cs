//-----------------------------------------------------------------------------
// <copyright file="MetadataController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;

namespace Microsoft.AspNetCore.OData.Routing.Controllers
{
    /// <summary>
    /// Represents a controller for generating OData service and metadata ($metadata) documents.
    /// </summary>
    public class MetadataController : ControllerBase
    {
        private static readonly Version _defaultEdmxVersion = new Version(4, 0);

        /// <summary>
        /// Generates the OData $metadata document.
        /// </summary>
        /// <returns>The <see cref="IEdmModel"/> representing $metadata.</returns>
        [HttpGet]
        public IEdmModel GetMetadata()
        {
            return GetModel();
        }

        /// <summary>
        /// Generates the OData service document.
        /// </summary>
        /// <returns>The service document for the service.</returns>
        [HttpGet]
        public ODataServiceDocument GetServiceDocument()
        {
            IEdmModel model = GetModel();
            return model.GenerateServiceDocument();

#if false
            ODataServiceDocument serviceDocument = new ODataServiceDocument();
            IEdmEntityContainer container = model.EntityContainer;

            // Add EntitySets into service document
            serviceDocument.EntitySets = container.EntitySets().Select(
                e => GetODataEntitySetInfo(model.GetNavigationSourceUrl(e).ToString(), e.Name));

            // Add Singletons into the service document
            IEnumerable<IEdmSingleton> singletons = container.Elements.OfType<IEdmSingleton>();
            serviceDocument.Singletons = singletons.Select(
                e => GetODataSingletonInfo(model.GetNavigationSourceUrl(e).ToString(), e.Name));

            // Add FunctionImports into service document
            // ODL spec says:
            // The edm:FunctionImport for a parameterless function MAY include the IncludeInServiceDocument attribute
            // whose Boolean value indicates whether the function import is advertised in the service document.
            // If no value is specified for this attribute, its value defaults to false.

            // Find all parameterless functions with "IncludeInServiceDocument = true"
            IEnumerable<IEdmFunctionImport> functionImports = container.Elements.OfType<IEdmFunctionImport>()
                .Where(f => !f.Function.Parameters.Any() && f.IncludeInServiceDocument);

            serviceDocument.FunctionImports = functionImports.Distinct(new FunctionImportComparer())
                .Select(f => GetODataFunctionImportInfo(f.Name));

            return serviceDocument;
#endif
        }

        /*
        private static ODataEntitySetInfo GetODataEntitySetInfo(string url, string name)
        {
            ODataEntitySetInfo info = new ODataEntitySetInfo
            {
                Name = name, // Required for JSON support
                Url = new Uri(url, UriKind.Relative)
            };

            return info;
        }

        private static ODataSingletonInfo GetODataSingletonInfo(string url, string name)
        {
            ODataSingletonInfo info = new ODataSingletonInfo
            {
                Name = name,
                Url = new Uri(url, UriKind.Relative)
            };

            return info;
        }

        private static ODataFunctionImportInfo GetODataFunctionImportInfo(string name)
        {
            ODataFunctionImportInfo info = new ODataFunctionImportInfo
            {
                Name = name,
                Url = new Uri(name, UriKind.Relative) // Relative to the OData root
            };

            return info;
        }
        */

        private IEdmModel GetModel()
        {
            IEdmModel model = Request.GetModel();
            if (model == null)
            {
                throw new InvalidOperationException(SRResources.RequestMustHaveModel);
            }

            model.SetEdmxVersion(_defaultEdmxVersion);
            return model;
        }
    }
}
