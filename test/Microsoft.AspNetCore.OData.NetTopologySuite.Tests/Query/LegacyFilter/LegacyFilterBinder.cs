using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.OData.UriParser;
using MsGeometry = Microsoft.Spatial.Geometry;
using MsSpatialImplementation = Microsoft.Spatial.SpatialImplementation;
using MsSpatialOperations = Microsoft.Spatial.SpatialOperations;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Query.LegacyFilter
{
    public class LegacyFilterBinder : FilterBinder
    {
        private const string GeoDistanceFunctionName = "geo.distance";
        private static readonly MethodInfo GeometryDistanceMethod = typeof(MsSpatialOperations).GetMethod(nameof(MsSpatialOperations.Distance), new[] { typeof(MsGeometry), typeof(MsGeometry) });
        private static readonly PropertyInfo CurrentImplementationProp = typeof(MsSpatialImplementation).GetProperty(nameof(MsSpatialImplementation.CurrentImplementation), BindingFlags.Public | BindingFlags.Static);
        private static readonly PropertyInfo OperationsProp = typeof(MsSpatialImplementation).GetProperty(nameof(MsSpatialImplementation.Operations), BindingFlags.Public | BindingFlags.Instance);

        public override Expression BindSingleValueFunctionCallNode(SingleValueFunctionCallNode node, QueryBinderContext context)
        {
            var arguments = BindArguments(node.Parameters, context);

            if (node.Name == GeoDistanceFunctionName)
            {
                return BindGeoDistance(node, context);
            }

            return base.BindSingleValueFunctionCallNode(node, context);
        }

        private Expression BindGeoDistance(SingleValueFunctionCallNode node, QueryBinderContext context)
        {
            // Expect exactly two parameters: (geomA, geomB)
            var arguments = BindArguments(node.Parameters, context);
            if (arguments == null || arguments.Length != 2)
            {
                throw new NotSupportedException($"The function '{GeoDistanceFunctionName}' must have exactly two parameters.");
            }

            var currentImplementation = Expression.Property(null, CurrentImplementationProp);
            var operations = Expression.Property(currentImplementation, OperationsProp);

            // Ensure both are NTS Geometry expressions
            Expression left = EnsureMsGeometry(arguments[0]);
            Expression right = EnsureMsGeometry(arguments[1]);

            // Emit left.Distance(right)
            return Expression.Call(operations, GeometryDistanceMethod, left, right);
        }

        private static Expression EnsureMsGeometry(Expression expression)
        {
            if (typeof(MsGeometry).IsAssignableFrom(expression.Type))
            {
                return expression;
            }

            if (expression is ConstantExpression constantExpression)
            {
                return constantExpression;
            }

            // As a last resort, try a convert (will fail at runtime if incompatible)
            return Expression.Convert(expression, typeof(MsGeometry));
        }
    }
}
