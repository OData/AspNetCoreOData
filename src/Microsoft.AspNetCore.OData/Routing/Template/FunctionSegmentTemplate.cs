// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
                throw new ODataException(Error.Format(SRResources.OperationIsNotBound, function.Name, "function"));
            }

            // Parameters should include all required parameter, but maybe include the optional parameter.
            ParameterMappings = function.VerifyAndBuildParameterMappings(parameters);
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

            ParameterMappings = operationSegment.Parameters.BuildParameterMappings(operation.FullName());
        }

        /// <summary>
        /// Gets the dictionary representing the mappings from the parameter names in the current function segment to the 
        /// parameter names in route data.
        /// </summary>
        public IDictionary<string, string> ParameterMappings { get; private set; }

        /// <summary>
        /// Gets the wrapped Edm function.
        /// </summary>
        public IEdmFunction Function { get; }

        /// <summary>
        /// Gets the wrapped navigation source.
        /// </summary>
        public IEdmNavigationSource NavigationSource { get; }

        /// <inheritdoc />
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            options = options ?? ODataRouteOptions.Default;
            Contract.Assert(options.EnableQualifiedOperationCall || options.EnableUnqualifiedOperationCall);

            string unqualifiedIdentifier, qualifiedIdentifier;
            if (ParameterMappings.Count == 0 && options.EnableNonParenthesisForEmptyParameterFunction)
            {
                unqualifiedIdentifier = "/" + Function.Name;
                qualifiedIdentifier = "/" + Function.FullName();
            }
            else
            {
                string parameterStr = "(" + string.Join(",", ParameterMappings.Select(a => $"{a.Key}={{{a.Value}}}")) + ")";
                unqualifiedIdentifier = "/" + Function.Name + parameterStr;
                qualifiedIdentifier = "/" + Function.FullName() + parameterStr;
            }

            if (options.EnableQualifiedOperationCall && options.EnableUnqualifiedOperationCall)
            {
                // "/NS.Function(...)"
                yield return qualifiedIdentifier;

                // "/Function(...)"
                yield return unqualifiedIdentifier;
            }
            else if (options.EnableQualifiedOperationCall)
            {
                // "/NS.Function(...)"
                yield return qualifiedIdentifier;
            }
            else
            {
                // "/Function(...)"
                yield return unqualifiedIdentifier;
            }
        }

        /// <inheritdoc />
        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            // If the function has no parameter, we don't need to do anything and just return an operation segment.
            if (ParameterMappings.Count == 0)
            {
                context.Segments.Add(new OperationSegment(Function, NavigationSource as IEdmEntitySetBase));
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

            IList<OperationSegmentParameter> parameters = SegmentTemplateHelpers.Match(context, Function, ParameterMappings);
            if (parameters == null)
            {
                return false;
            }

            context.Segments.Add(new OperationSegment(Function, parameters, NavigationSource as IEdmEntitySetBase));
            return true;
        }

        private bool HasOptionalMissing()
        {
            return ParameterMappings.Count != Function.Parameters.Count() - 1;
        }
    }
}
