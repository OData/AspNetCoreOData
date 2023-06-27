using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using ODataQueryBuilder.Query.Container;
using ODataQueryBuilder.Query.Wrapper;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;

namespace ODataQueryBuilder.Query.Expressions
{
    internal class ComputeBinder : TransformationBinderBase
    {
        private ComputeTransformationNode _transformation;
        private IEdmModel _model;

        internal ComputeBinder(ODataQuerySettings settings, IAssemblyResolver assembliesResolver, Type elementType,
            IEdmModel model, ComputeTransformationNode transformation)
            : base(settings, assembliesResolver, elementType, model)
        {
            Contract.Assert(transformation != null);

            _transformation = transformation;
            _model = model;

            this.ResultClrType = typeof(ComputeWrapper<>).MakeGenericType(this.ElementType);
        }

        public IQueryable Bind(IQueryable query)
        {
            PreprocessQuery(query);
            // compute(X add Y as Z, A mul B as C) adds new properties to the output
            // Should return following expression
            // .Select($it => new ComputeWrapper<T> {
            //      Instance = $it,
            //      Model = parametrized(IEdmModel),
            //      Container => new AggregationPropertyContainer() {
            //          Name = "Z", 
            //          Value = $it.X + $it.Y, 
            //          Next = new LastInChain() {
            //              Name = "C",
            //              Value = $it.A * $it.B
            //      }
            // })

            List<MemberAssignment> wrapperTypeMemberAssignments = new List<MemberAssignment>();

            // Set Instance property
            var wrapperProperty = this.ResultClrType.GetProperty("Instance");
            wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, this.LambdaParameter));
            var properties = new List<NamedPropertyExpression>();
            foreach (var computeExpression in this._transformation.Expressions)
            {
                properties.Add(new NamedPropertyExpression(Expression.Constant(computeExpression.Alias), CreateComputeExpression(computeExpression)));
            }

            // Initialize property 'Model' on the wrapper class.
            // source = new Wrapper { Model = parameterized(a-edm-model) }
            // Always parameterize as EntityFramework does not let you inject non primitive constant values (like IEdmModel).
            wrapperProperty = this.ResultClrType.GetProperty("Model");
            var wrapperPropertyValueExpression = LinqParameterContainer.Parameterize(typeof(IEdmModel), _model);
            wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, wrapperPropertyValueExpression));

            // Set new compute properties
            wrapperProperty = ResultClrType.GetProperty("Container");
            wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, AggregationPropertyContainer.CreateNextNamedPropertyContainer(properties)));

            var initilizedMember =
                Expression.MemberInit(Expression.New(ResultClrType), wrapperTypeMemberAssignments);
            var selectLambda = Expression.Lambda(initilizedMember, this.LambdaParameter);

            var result = ExpressionHelpers.Select(query, selectLambda, this.ElementType);
            return result;
        }

        private Expression CreateComputeExpression(ComputeExpression expression)
        {
            Expression body = BindAccessor(expression.Expression);
            return WrapConvert(body);
        }
    }
}
