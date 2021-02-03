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

            // parameters should include all required parameter, but maybe include the optional parameter.
            ParameterMappings = functionImport.Function.VerifyAndBuildParameterMappings(parameters);

            string routeKey = ParameterMappings.BuildRouteKey();
            string parameterStr = ParameterMappings.Count == 0 ? "()" : $"({{{routeKey}}})";
            Literal = functionImport.Name + parameterStr;

            IsSingle = functionImport.Function.ReturnType.TypeKind() != EdmTypeKind.Collection;
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

            ParameterMappings = OperationHelper.BuildParameterMappings(segment.Parameters, operationImport.Name);

            // join the parameters as p1={p1}
            string routeKey = ParameterMappings.BuildRouteKey();
            string parameterStr = ParameterMappings.Count == 0 ? "()" : $"({{{routeKey}}})";
            Literal = FunctionImport.Name + parameterStr;

            IsSingle = FunctionImport.Function.ReturnType.TypeKind() != EdmTypeKind.Collection;
        }

        /// <summary>
        /// Gets the dictionary representing the mappings from the parameter names in the current function segment to the 
        /// parameter names in route data.
        /// </summary>
        public IDictionary<string, string> ParameterMappings { get; }

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
                throw Error.ArgumentNull(nameof(context));
            }

            // If the function has no parameter, we don't need to do anything and just return an operation import segment.
            if (ParameterMappings.Count == 0)
            {
                return new OperationImportSegment(FunctionImport, NavigationSource as IEdmEntitySetBase);
            }

            if (!SegmentTemplateHelpers.TryParseRouteKey(context.RouteValues, context.UpdatedValues, ParameterMappings))
            {
                return null;
            }

            IList<OperationSegmentParameter> parameters = SegmentTemplateHelpers.Match(context, FunctionImport.Function, ParameterMappings);
            if (parameters == null)
            {
                return null;
            }

            return new OperationImportSegment(FunctionImport, NavigationSource as IEdmEntitySetBase, parameters);
        }
    }
}
