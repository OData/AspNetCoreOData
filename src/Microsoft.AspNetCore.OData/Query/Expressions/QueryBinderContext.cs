//-----------------------------------------------------------------------------
// <copyright file="QueryBinderContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    public class QueryBinderContext
    {
        private const string ODataItParameterName = "$it";
        private const string ODataThisParameterName = "$this";

        private Stack<IDictionary<string, ParameterExpression>> _parametersStack = new Stack<IDictionary<string, ParameterExpression>>();
        private IDictionary<string, ParameterExpression> _lambdaParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryBinderContext" /> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="querySettings">The query setting.</param>
        /// <param name="clrType">The element CLR type.</param>
        public QueryBinderContext(IEdmModel model, ODataQuerySettings querySettings, Type clrType)
        {
            Model = model ?? throw Error.ArgumentNull(nameof(model));

            QuerySettings = querySettings ?? throw Error.ArgumentNull(nameof(querySettings));

            ElementClrType = clrType ?? throw Error.ArgumentNull(nameof(clrType));

            ParameterExpression filterParameter = Expression.Parameter(clrType, ODataItParameterName);
            AddlambdaParameters(ODataItParameterName, filterParameter);
        }

        /// <summary>
        /// Gets the Edm model.
        /// </summary>
        public IEdmModel Model { get; }

        /// <summary>
        /// Gets the query settings.
        /// </summary>
        public ODataQuerySettings QuerySettings { get; }

        /// <summary>
        /// Gets the Element Clr type.
        /// </summary>
        public Type ElementClrType { get; private set; }

        // public virtual ODataQueryContext QueryContext { get; set; }

        public IAssemblyResolver AssembliesResolver { get; set; }

        /// <summary>
        /// Flattened list of properties from base query, for case when binder is applied for aggregated query.
        /// Or the properties from $compute query options.
        /// </summary>
        public IDictionary<string, Expression> ComputedProperties { get; set; }

        /// <summary>
        /// Gets or sets the property that indicates if an expression has already been ordered.
        /// </summary>
        public bool AlreadyOrdered { get; set; }

        private IFilterBinder _filterBinder;
        internal IFilterBinder FilterBinder
        {
            get
            {
                if (_filterBinder == null)
                {
                    // use the default filter binder
                    _filterBinder = new FilterBinder2();
                }

                return _filterBinder;
            }
            set
            {
                _filterBinder = value;
            }
        }

        public virtual ParameterExpression Parameter => _lambdaParameters[ODataItParameterName];

        public virtual ParameterExpression GetParameter(string name)
        {
            return _lambdaParameters[name];
        }

        public void AddlambdaParameters(string name, ParameterExpression parameter)
        {
            if (_lambdaParameters == null)
            {
                _lambdaParameters = new Dictionary<string, ParameterExpression>();
            }

            _lambdaParameters[name] =  parameter;
        }

        private Stack<Type> _elementTypeStack = new Stack<Type>();
        public void EnterNextBinderScope(Type elementType)
        {
            Contract.Assert(_elementTypeStack != null);
            _elementTypeStack.Push(ElementClrType);
            ElementClrType = elementType;
        }

        public void ExitNextBinderScope()
        {
            if (_elementTypeStack.Count != 0)
            {
                ElementClrType = _elementTypeStack.Pop();
            }
            else
            {
                ElementClrType = null;
            }
        }

        public void EnterLambdaScope()
        {
            Contract.Assert(_lambdaParameters != null);
            _parametersStack.Push(_lambdaParameters);
        }

        public void ExitLamdbaScope()
        {
            if (_parametersStack.Count != 0)
            {
                _lambdaParameters = _parametersStack.Pop();
            }
            else
            {
                _lambdaParameters = null;
            }
        }

        public ParameterExpression HandleLambdaParameters(IEnumerable<RangeVariable> rangeVariables)
        {
            ParameterExpression lambdaIt = null;

            IDictionary<string, ParameterExpression> newParameters = new Dictionary<string, ParameterExpression>();
            foreach (RangeVariable rangeVariable in rangeVariables)
            {
                ParameterExpression parameter;

                // Create a Parameter Expression for rangeVariables which are not $it Lambda parameters or $this.
                if (!_lambdaParameters.TryGetValue(rangeVariable.Name, out parameter) && rangeVariable.Name != ODataThisParameterName)
                {
                    // Work-around issue 481323 where UriParser yields a collection parameter type
                    // for primitive collections rather than the inner element type of the collection.
                    // Remove this block of code when 481323 is resolved.
                    IEdmTypeReference edmTypeReference = rangeVariable.TypeReference;
                    IEdmCollectionTypeReference collectionTypeReference = edmTypeReference as IEdmCollectionTypeReference;
                    if (collectionTypeReference != null)
                    {
                        IEdmCollectionType collectionType = collectionTypeReference.Definition as IEdmCollectionType;
                        if (collectionType != null)
                        {
                            edmTypeReference = collectionType.ElementType;
                        }
                    }

                    parameter = Expression.Parameter(Model.GetClrType(edmTypeReference, AssembliesResolver), rangeVariable.Name);
                    Contract.Assert(lambdaIt == null, "There can be only one parameter in an Any/All lambda");
                    lambdaIt = parameter;
                }
                newParameters.Add(rangeVariable.Name, parameter);
            }

            _lambdaParameters = newParameters;
            return lambdaIt;
        }
    }
}
