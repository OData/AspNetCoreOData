﻿//-----------------------------------------------------------------------------
// <copyright file="QueryBinderContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    /// <summary>
    /// Encapsulates all binder information about an individual OData query option binding.
    /// </summary>
    public class QueryBinderContext
    {
        /// <summary>
        /// The parameter name for root type.(it could be renamed as $root).
        /// </summary>
        private const string DollarIt = "$it";

        /// <summary>
        /// The parameter name for current type.
        /// </summary>
        private const string DollarThis = "$this";

        /// <summary>
        /// All parameters present in current context.
        /// </summary>
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

            ElementType = Model.GetEdmTypeReference(ElementClrType)?.Definition;

            LambdaParameter = Expression.Parameter(ElementClrType, DollarIt);

            if (ElementType == null)
            {
                throw new ODataException(Error.Format(SRResources.ClrTypeNotInModel, ElementClrType.FullName));
            }

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
        /// <param name="querySettings">The query setting.</param>
        /// <param name="clrType">The current element CLR type in this context (scope).</param>
        public QueryBinderContext(QueryBinderContext context, ODataQuerySettings querySettings, Type clrType)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            QuerySettings = querySettings ?? throw Error.ArgumentNull(nameof(querySettings));

            // ~/Customers?$select=Addresses($orderby=$this/ZipCode,$it/Age;$select=Codes($orderby=$this desc,$it/Name))

            ElementClrType = clrType ?? throw Error.ArgumentNull(nameof(clrType));

            Model = context.Model;

            ElementType = Model.GetEdmTypeReference(ElementClrType)?.Definition;

            if (ElementType == null)
            {
                throw new ODataException(Error.Format(SRResources.ClrTypeNotInModel, ElementClrType.FullName));
            }

            // Inherit the lambda parameters, $it, $this, etc.
            _lambdaParameters = new Dictionary<string, ParameterExpression>(context._lambdaParameters);

            // Only update $this parameter.
            ParameterExpression thisParameters = Expression.Parameter(clrType, DollarIt);
            _lambdaParameters[DollarThis] = thisParameters;

            GetNestedFilterBinder = context.GetNestedFilterBinder;
            GetNestedOrderByBinder = context.GetNestedOrderByBinder;

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

        /// <summary>
        /// Gets or sets the assembly resolver.
        /// </summary>
        public IAssemblyResolver AssembliesResolver { get; set; }

        /// <summary>
        /// Gets or sets the nested filter binder.
        /// For example: we do a orderby and a nested $filter.
        /// $orderby=Addresses/$count($filter=HouseNumber gt 8)   or
        /// $filter=collectionProp/$count($filter=Name eq 'abc') gt 2
        /// </summary>
        public Func<IFilterBinder> GetNestedFilterBinder { get; set; }

        /// <summary>
        /// Gets or sets the nested orderby binder.
        /// </summary>
        public Func<IOrderByBinder> GetNestedOrderByBinder { get; set; }

        /// <summary>
        /// Flattened list of properties from base query, for case when binder is applied for aggregated query.
        /// Or the properties from $compute query options.
        /// </summary>
        public IDictionary<string, Expression> ComputedProperties { get; } = new Dictionary<string, Expression>();

        /// <summary>
        /// Gets the <see cref="IEdmType"/> of the element type.
        /// </summary>
        public IEdmType ElementType { get; }

        /// <summary>
        /// Gets the <see cref="IEdmNavigationSource"/> that contains the element.
        /// </summary>
        public IEdmNavigationSource NavigationSource { get; set; }

        internal bool IsNested { get; } = false;

        /// <summary>
        /// Gets the current parameter. Current parameter is the parameter at root of this context.
        /// </summary>
        public ParameterExpression CurrentParameter => _lambdaParameters[DollarThis];

        /// <summary>
        /// Gets the parameter using parameter name.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <returns>The parameter expression.</returns>
        public ParameterExpression GetParameter(string name)
        {
            return _lambdaParameters[name];
        }

        /// <summary>
        /// Remove the parameter.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        public void RemoveParameter(string name)
        {
            if (name != null)
            {
                _lambdaParameters.Remove(name);
            }
        }

        #region AggregationBinder
        public TransformationNode Transformation { get; set; }

        public IEnumerable<AggregateExpressionBase> AggregateExpressions { get; set; }
        public IEnumerable<GroupByPropertyNode> GroupingProperties { get; set; }

        public Type GroupByClrType { get; set; }
        public Type ResultClrType { get; set; }
        public ParameterExpression LambdaParameter { get; set; }
        public Type TransformationElementType { get { return this.LambdaParameter.Type; } }
        public bool ClassicEF { get; set; }
        public bool HasInstancePropertyContainer;

        /// <summary>
        /// Base query used for the binder.
        /// </summary>
        public IQueryable BaseQuery;

        /// <summary>
        /// Flattened list of properties from base query, for case when binder is applied for aggregated query.
        /// </summary>
        public IDictionary<string, Expression> FlattenedPropertyContainer;
        #endregion

        internal (string, ParameterExpression) HandleLambdaParameters(IEnumerable<RangeVariable> rangeVariables)
        {
            ParameterExpression lambdaIt = null;
            string name = null;
            foreach (RangeVariable rangeVariable in rangeVariables)
            {
                // If the range variable exists, it's from upper layer, skip it.
                if (_lambdaParameters.ContainsKey(rangeVariable.Name))
                {
                    continue;
                }

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

                ParameterExpression parameter = Expression.Parameter(Model.GetClrType(edmTypeReference, AssembliesResolver), rangeVariable.Name);
                Contract.Assert(lambdaIt == null, "There can be only one parameter in an Any/All lambda");
                lambdaIt = parameter;

                _lambdaParameters.Add(rangeVariable.Name, parameter);
                name = rangeVariable.Name;
            }

            // OData spec supports any() / all()
            //if (lambdaIt == null)
            //{
            //    throw new ODataException("TODO: There can be only one parameter in an Any/All lambda");
            //}

            return (name, lambdaIt);
        }
    }
}
