// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Edm;
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
            : this(functionImport.GetFunctionParamterMappings(), functionImport, navigationSource)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionImportSegmentTemplate" /> class.
        /// </summary>
        /// <param name="parameters">The function parameter template mappings.The key string is case-sensitive, the value string should wrapper with { and }.</param>
        /// <param name="functionImport">The Edm function import.</param>
        /// <param name="navigationSource">The target navigation source, it could be null.</param>
        public FunctionImportSegmentTemplate(IDictionary<string, string> parameters, IEdmFunctionImport functionImport, IEdmNavigationSource navigationSource)
        {
            if (parameters == null)
            {
                throw Error.ArgumentNull(nameof(parameters));
            }

            FunctionImport = functionImport ?? throw Error.ArgumentNull(nameof(functionImport));
            NavigationSource = navigationSource;

            // parameters should include all required parameters, but maybe include the optional parameters.
            ParameterMappings = functionImport.Function.VerifyAndBuildParameterMappings(parameters);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionImportSegmentTemplate" /> class.
        /// </summary>
        /// <param name="segment">The input function import segment.</param>
        public FunctionImportSegmentTemplate(OperationImportSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull(nameof(segment));
            }

            IEdmOperationImport operationImport = segment.OperationImports.First();
            if (!operationImport.IsFunctionImport())
            {
                throw new ODataException(Error.Format(SRResources.SegmentShouldBeKind, "FunctionImport", "FunctionImportSegmentTemplate"));
            }

            FunctionImport = (IEdmFunctionImport)operationImport;

            NavigationSource = segment.EntitySet;

            ParameterMappings = segment.Parameters.BuildParameterMappings(operationImport.Name);
        }

        /// <summary>
        /// Gets the dictionary representing the mappings from the parameter names in the current function segment to the 
        /// parameter names in route data.
        /// </summary>
        public IDictionary<string, string> ParameterMappings { get; }

        /// <summary>
        /// Gets the target Navigation source of this segment.
        /// </summary>
        public IEdmNavigationSource NavigationSource { get; }

        /// <summary>
        /// Gets the wrapped Edm function import.
        /// </summary>
        public IEdmFunctionImport FunctionImport { get; }

        /// <inheritdoc />
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            options = options ?? ODataRouteOptions.Default;

            if (ParameterMappings.Count == 0 && options.EnableNonParenthsisForEmptyParameterFunction)
            {
                yield return $"/{FunctionImport.Name}";
            }
            else
            {
                string parameters = string.Join(",", ParameterMappings.Select(a => $"{a.Key}={{{a.Value}}}"));
                yield return $"/{FunctionImport.Name}({parameters})";
            }
        }

        /// <inheritdoc />
        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            // If the function has no parameter, we don't need to do anything and just return an operation import segment.
            if (ParameterMappings.Count == 0)
            {
                context.Segments.Add(new OperationImportSegment(FunctionImport, NavigationSource as IEdmEntitySetBase));
                return true;
            }

            if (HasOptionalMissing())
            {
                // If this function template has the optional parameter missing,
                // for example: ~/GetSalary(min={min},max={max}), without ave={ave}
                // We should avoid this template matching with "~/GetSalary(min=1,max=2,ave=3)"
                // Because, In this request, the comming route data has the following:
                // min = 1
                // max = 2,ave=3
                // Therefore, we need to combine the route data together and separate them using "," again.
                if (!SegmentTemplateHelpers.IsMatchParameters(context.RouteValues, ParameterMappings))
                {
                    return false;
                }
            }

            IList<OperationSegmentParameter> parameters = SegmentTemplateHelpers.Match(context, FunctionImport.Function, ParameterMappings);
            if (parameters == null)
            {
                return false;
            }

            context.Segments.Add(new OperationImportSegment(FunctionImport, NavigationSource as IEdmEntitySetBase, parameters));
            return true;
        }

        private bool HasOptionalMissing()
        {
            return ParameterMappings.Count != FunctionImport.Function.Parameters.Count();
        }
    }
}
