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
        private const string DollarIt = "$it";
        private const string DollarThis = "$this";

        private Stack<IDictionary<string, ParameterExpression>> _parametersStack = new Stack<IDictionary<string, ParameterExpression>>();

        private IDictionary<string, ParameterExpression> _lambdaParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryBinderContext" /> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="querySettings">The query setting.</param>
        /// <param name="clrType">The current element CLR type in this context (scope).</param>
        public QueryBinderContext(IEdmModel model, ODataQuerySettings querySettings, Type clrType)
        {
            Model = model ?? throw Error.ArgumentNull(nameof(model));

            QuerySettings = querySettings ?? throw Error.ArgumentNull(nameof(querySettings));

            ElementClrType = clrType ?? throw Error.ArgumentNull(nameof(clrType));

            // Customers?$select=EmailAddresses($filter=endswith($this,'.com') and starswith($it/Name, 'Sam'))
            // Here:
            // $this -> instance in EmailAddresses
            // $it -> instance in Customers
            // When we process $select=..., we create QueryBindContext, the input clrType is "Customer".
            // When we process nested $filter, we create another QueryBindContext, the input clrType is "string".
            ParameterExpression thisParameters = Expression.Parameter(clrType, DollarIt);
            _lambdaParameters = new Dictionary<string, ParameterExpression>();

            // So, from top level, $it and $this are the same parameters
            _lambdaParameters[DollarIt] = thisParameters;
            _lambdaParameters[DollarThis] = thisParameters;
            // Categories?$expand=Products($filter=OrderItems/any(oi:oi/UnitPrice ne UnitPrice)
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryBinderContext" /> class.
        /// </summary>
        /// <param name="context">The parent query binder context.</param>
        /// <param name="clrType">The The current element CLR type in this context (scope).</param>
        public QueryBinderContext(QueryBinderContext context, Type clrType)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            // ~/Customers?$select=Addresses($orderby=$this/ZipCode,$it/Age;$select=Codes($orderby=$this desc,$it/Name))

            ElementClrType = clrType ?? throw Error.ArgumentNull(nameof(clrType));

            Model = context.Model;

            QuerySettings = context.QuerySettings;

            // Inherit the lambda parameters, $it, $this, etc.
            _lambdaParameters = new Dictionary<string, ParameterExpression>(context._lambdaParameters);

            // Only update $this parameter.
            ParameterExpression thisParameters = Expression.Parameter(clrType, DollarIt);
            _lambdaParameters[DollarThis] = thisParameters;

            IsNested = true;
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
        public Type ElementClrType { get; }

        // public virtual ODataQueryContext QueryContext { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IAssemblyResolver AssembliesResolver { get; set; }

        /// <summary>
        /// Flattened list of properties from base query, for case when binder is applied for aggregated query.
        /// Or the properties from $compute query options.
        /// </summary>
        public IDictionary<string, Expression> ComputedProperties { get; set; }

        internal bool IsNested { get; } = false;


        public ParameterExpression ParameterExpression { get; }

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

        public ParameterExpression TopExpression { get; }


        /// <summary>
        /// Gets the current parameter. Current parameter is the parameter at root of this context.
        /// </summary>
        public ParameterExpression CurrentParameter => _lambdaParameters[DollarThis];

        public ParameterExpression GetParameter(string name)
        {
            return _lambdaParameters[name];
        }

        public bool ContainsParameter(string name)
        {
            return _lambdaParameters.ContainsKey(name);
        }

        public bool TryGetParameter(string name, out ParameterExpression parameter)
        {
            return _lambdaParameters.TryGetValue(name, out parameter);
        }

        public void AddlambdaParameters(string name, ParameterExpression parameter)
        {
            if (_lambdaParameters == null)
            {
                _lambdaParameters = new Dictionary<string, ParameterExpression>();
            }

            _lambdaParameters[name] =  parameter;
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
                if (!_lambdaParameters.TryGetValue(rangeVariable.Name, out parameter) && rangeVariable.Name != DollarThis)
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
