// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that could match an <see cref="IEdmFunctionImport"/>.
    /// </summary>
    public class FunctionImportSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionImportSegmentTemplate" /> class.
        /// </summary>
        /// <param name="functionImport">The Edm function import.</param>
        /// <param name="navigationSource">The target navigation source, it could be null.</param>
        public FunctionImportSegmentTemplate(IEdmFunctionImport functionImport, IEdmNavigationSource navigationSource)
        {
            FunctionImport = functionImport ?? throw new ArgumentNullException(nameof(functionImport));
            NavigationSource = navigationSource;

            IDictionary<string, string> keyMappings = new Dictionary<string, string>();
            foreach (var parameter in functionImport.Function.Parameters)
            {
                keyMappings[parameter.Name] = $"{{{parameter.Name}}}";
            }

            Literal = functionImport.Name + "(" + string.Join(",", keyMappings.Select(a => $"{a.Key}={a.Value}")) + ")";

            IsSingle = functionImport.Function.ReturnType.TypeKind() != EdmTypeKind.Collection;
        }

        /// <inheritdoc />
        public override string Literal { get; }

        /// <inheritdoc />
        public override IEdmType EdmType => FunctionImport.Function.ReturnType.Definition;

        /// <summary>
        /// Gets the target Navigation source of this segment.
        /// </summary>
        public override IEdmNavigationSource NavigationSource { get; }

        /// <summary>
        /// Gets the wrapped Edm function import.
        /// </summary>
        public IEdmFunctionImport FunctionImport { get; }

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.FunctionImport;

        /// <inheritdoc />
        public override bool IsSingle { get; }

        /// <inheritdoc />
        public override ODataPathSegment Translate(ODataTemplateTranslateContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            IEdmModel model = context.Model;
            RouteValueDictionary routeValues = context.RouteValues;
            // TODO: process the parameter alias
            IList<OperationSegmentParameter> parameters = new List<OperationSegmentParameter>();
            foreach (var parameter in FunctionImport.Function.Parameters)
            {
                if (routeValues.TryGetValue(parameter.Name, out object rawValue))
                {
                    // for resource or collection resource, this method will return "ODataResourceValue, ..." we should support it.
                    if (parameter.Type.IsResourceOrCollectionResource())
                    {
                        // For FromODataUri
                        string prefixName = ODataParameterValue.ParameterValuePrefix + parameter.Name;
                        routeValues[prefixName] = new ODataParameterValue(rawValue, parameter.Type);

                        parameters.Add(new OperationSegmentParameter(parameter.Name, rawValue));
                    }
                    else
                    {
                        string strValue = rawValue as string;
                        object newValue = ODataUriUtils.ConvertFromUriLiteral(strValue, ODataVersion.V4, model, parameter.Type);

                        // for without FromODataUri, so update it, for example, remove the single quote for string value.
                        routeValues[parameter.Name] = newValue;

                        // For FromODataUri
                        string prefixName = ODataParameterValue.ParameterValuePrefix + parameter.Name;
                        routeValues[prefixName] = new ODataParameterValue(newValue, parameter.Type);

                        parameters.Add(new OperationSegmentParameter(parameter.Name, newValue));
                    }
                }
            }

            IEdmNavigationSource targetset = null; // todo

            return new OperationImportSegment(FunctionImport, targetset as IEdmEntitySetBase, parameters);
        }
    }
}
