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
    /// Represents a template that could match a bound <see cref="IEdmFunction"/>.
    /// </summary>
    public class FunctionSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionSegmentTemplate" /> class.
        /// </summary>
        /// <param name="function">The Edm function, it should be bound function.</param>
        /// <param name="navigationSource">The Edm navigation source of this function return. It could be null.</param>
        public FunctionSegmentTemplate(IEdmFunction function, IEdmNavigationSource navigationSource)
            : this(function.GetFunctionParamterMappings(), function, navigationSource)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionSegmentTemplate" /> class.
        /// </summary>
        /// <param name="parameters">The function parameter template mappings.The key string is case-sensitive, the value string should wrapper with { and }.</param>
        /// <param name="function">The Edm function, it should be bound function.</param>
        /// <param name="navigationSource">The Edm navigation source of this function return. It could be null.</param>
        public FunctionSegmentTemplate(IDictionary<string, string> parameters, IEdmFunction function, IEdmNavigationSource navigationSource)
        {
            if (parameters == null)
            {
                throw Error.ArgumentNull(nameof(parameters));
            }

            Function = function ?? throw Error.ArgumentNull(nameof(function));

            NavigationSource = navigationSource;

            // Only accept the bound function
            if (!function.IsBound)
            {
                throw new ODataException(Error.Format(SRResources.FunctionIsNotBound, function.Name));
            }

            // Parameters should include all required parameter, but maybe include the optional parameter.
            ParameterMappings = function.VerifyAndBuildParameterMappings(parameters);

            string routeKey = ParameterMappings.BuildRouteKey();

            string parameterStr = ParameterMappings.Count == 0 ? "()" : $"({{{routeKey}}})";

            UnqualifiedIdentifier = function.Name + parameterStr;

            Literal = function.FullName() + parameterStr;

            // Function will always have the return type
            IsSingle = function.ReturnType.TypeKind() != EdmTypeKind.Collection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionSegmentTemplate" /> class.
        /// </summary>
        /// <param name="operationSegment">The operation segment, it should be a function segment and the parameters are template.</param>
        public FunctionSegmentTemplate(OperationSegment operationSegment)
        {
            if (operationSegment == null)
            {
                throw Error.ArgumentNull(nameof(operationSegment));
            }

            IEdmOperation operation = operationSegment.Operations.FirstOrDefault();
            if (!operation.IsFunction())
            {
                throw new ODataException(Error.Format(SRResources.SegmentShouldBeKind, "Function", "FunctionSegmentTemplate"));
            }

            Function = (IEdmFunction)operation;

            NavigationSource = operationSegment.EntitySet;

            ParameterMappings = OperationHelper.BuildParameterMappings(operationSegment.Parameters, operation.FullName());

            // join the parameters as p1={p1}
            string routeKey = ParameterMappings.BuildRouteKey();

            string parameterStr = ParameterMappings.Count == 0 ? "()" : $"({{{routeKey}}})";

            UnqualifiedIdentifier = Function.Name + parameterStr;

            Literal = Function.FullName() + parameterStr;

            // Function will always have the return type
            IsSingle = Function.ReturnType.TypeKind() != EdmTypeKind.Collection;
        }

        /// <summary>
        /// Gets the dictionary representing the mappings from the parameter names in the current function segment to the 
        /// parameter names in route data.
        /// </summary>
        public IDictionary<string, string> ParameterMappings { get; private set; }

        /// <inheritdoc />
        public override string Literal { get; }

        /// <inheritdoc />
        public override IEdmType EdmType => Function.ReturnType.Definition;

        /// <summary>
        /// Gets the wrapped Edm function.
        /// </summary>
        public IEdmFunction Function { get; }

        /// <inheritdoc />
        public override IEdmNavigationSource NavigationSource { get; }

        /// <summary>
        /// Key=value, Key=value
        /// </summary>
        internal string UnqualifiedIdentifier { get; }

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.Function;

        /// <inheritdoc />
        public override bool IsSingle { get; }

        /// <inheritdoc />
        public override ODataPathSegment Translate(ODataTemplateTranslateContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            // If the function has no parameter, we don't need to do anything and just return an operation segment.
            if (ParameterMappings.Count == 0)
            {
                return new OperationSegment(Function, NavigationSource as IEdmEntitySetBase);
            }

            if (!SegmentTemplateHelpers.TryParseRouteKey(context.RouteValues, context.UpdatedValues, ParameterMappings))
            {
                return null;
            }

            IList<OperationSegmentParameter> parameters = SegmentTemplateHelpers.Match(context, Function, ParameterMappings);
            if (parameters == null)
            {
                return null;
            }

            return new OperationSegment(Function, parameters, NavigationSource as IEdmEntitySetBase);
        }
    }
}
