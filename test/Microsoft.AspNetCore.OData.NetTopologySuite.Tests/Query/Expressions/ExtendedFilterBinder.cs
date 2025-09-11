//-----------------------------------------------------------------------------
// <copyright file="ExtendedFilterBinder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.OData.UriParser;
using NtsGeometry = NetTopologySuite.Geometries.Geometry;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Query.Expressions;

public class ExtendedFilterBinder : FilterBinder
{
    private const string GeoDistanceFunctionName = "geo.distance";
    private const string GeoLengthFunctionName = "geo.length";
    private const string GeoIntersectsFunctionName = "geo.intersects";
    private static readonly MethodInfo DistanceMethod = typeof(NtsGeometry).GetMethod("Distance", [typeof(NtsGeometry)]);
    private static readonly PropertyInfo LengthProp = typeof(NtsGeometry).GetProperty("Length");
    private static readonly MethodInfo IntersectsMethod = typeof(NtsGeometry).GetMethod("Intersects", [typeof(NtsGeometry)]);

    public override Expression BindSingleValueFunctionCallNode(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        if (node.Name == GeoDistanceFunctionName)
        {
            return BindGeoDistance(node, context);
        }
        else if (node.Name == GeoLengthFunctionName)
        {
            return BindGeoLength(node, context);
        }
        else if (node.Name == GeoIntersectsFunctionName)
        {
            return BindGeoIntersects(node, context);
        }

        return base.BindSingleValueFunctionCallNode(node, context);
    }

    public override Expression BindConstantNode(ConstantNode constantNode, QueryBinderContext context)
    {
        object value = constantNode.Value;
        // ODL doesn't know NTS types => TypeReference can be null for spatial literals.
        if (constantNode.TypeReference == null && value != null && typeof(NtsGeometry).IsAssignableFrom(value.GetType()))
        {
            Type constantType = value.GetType();

            if (context.QuerySettings.EnableConstantParameterization)
            {
                return LinqParameterizer.Parameterize(constantType, value);
            }
            else
            {
                return Expression.Constant(value, constantType);
            }
        }

        return base.BindConstantNode(constantNode, context);
    }

    private Expression BindGeoDistance(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        // Expect exactly two parameters: (geomA, geomB)
        var arguments = BindArguments(node.Parameters, context);
        
        if (arguments == null || arguments.Length != 2)
        {
            throw new NotSupportedException($"The function '{GeoDistanceFunctionName}' must have exactly two parameters.");
        }

        Expression left = arguments[0];
        Expression right = arguments[1];

        // Emit left.Distance(right)
        return Expression.Call(left, DistanceMethod, right);
    }

    private Expression BindGeoLength(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        // Expect exactly one parameter: (geom)
        var arguments = BindArguments(node.Parameters, context);
        if (arguments == null || arguments.Length != 1)
        {
            throw new NotSupportedException($"The function '{GeoLengthFunctionName}' must have exactly one parameter.");
        }

        Expression geom = arguments[0];
        
        // Emit geom.Length
        return Expression.Property(geom, LengthProp);
    }

    private Expression BindGeoIntersects(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        // Expect exactly two parameters: (geomA, geomB)
        var arguments = BindArguments(node.Parameters, context);
        if (arguments == null || arguments.Length != 2)
        {
            throw new NotSupportedException($"The function '{GeoIntersectsFunctionName}' must have exactly two parameters.");
        }

        Expression left = arguments[0];
        Expression right = arguments[1];

        // Emit left.Intersects(right)
        return Expression.Call(left, IntersectsMethod, right);
    }

    // Lightweight parameterizer for EF Core query translation
    private static class LinqParameterizer
    {
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, (Func<object, object> Ctor, PropertyInfo Prop)> Cache
            = new();

        public static Expression Parameterize(Type type, object value)
        {
            var entry = Cache.GetOrAdd(type, Build);
            var wrapper = entry.Ctor(value);
            // () => new Wrapper<T>(value).TypedProperty
            return Expression.Property(Expression.Constant(wrapper), entry.Prop);
        }

        private static (Func<object, object> Ctor, PropertyInfo Prop) Build(Type t)
        {
            var wrapperType = typeof(Wrapper<>).MakeGenericType(t);
            var ctorInfo = wrapperType.GetConstructor(new[] { t })!;
            var propInfo = wrapperType.GetProperty(nameof(Wrapper<int>.TypedProperty))!;
            // Fast late-bound ctor: object -> object (wrapper instance)
            Func<object, object> ctor = (object v) => ctorInfo.Invoke(new[] { v });
            return (ctor, propInfo);
        }

        // Wrapper used to expose a strongly-typed property for EF parameterization
        private sealed class Wrapper<T>
        {
            public Wrapper(T value) => TypedProperty = value;
            public T TypedProperty { get; }
        }
    }
}
