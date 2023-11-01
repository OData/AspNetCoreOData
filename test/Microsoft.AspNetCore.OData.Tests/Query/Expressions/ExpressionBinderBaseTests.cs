//-----------------------------------------------------------------------------
// <copyright file="ExpressionBinderBaseTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Expressions
{
    /// <summary>
    /// Tests to ExpressionBinderBase binder.
    /// </summary>
    public class ExpressionBinderBaseTests
    {
        [Fact]
        public void RetrieveClrTypeForConstant_WorksForEnum()
        {
            var builder = new ODataConventionModelBuilder();
            builder.ComplexType<Address>();
            var model = builder.GetEdmModel();
            var enumType = model.SchemaElements.OfType<IEdmEnumType>().First();
            var enumTypeRef = new EdmEnumTypeReference(enumType, true);

            MyExpressionBinder binder = new MyExpressionBinder(model, new ODataQuerySettings());
            object enumValue = new ODataEnumValue("low");
            Type type = binder.RetrieveClrTypeForConstant(enumTypeRef, ref enumValue);
            Assert.NotNull(type);
            Assert.Equal(typeof(Level), type);
            Assert.Equal(Level.Low, enumValue);
        }

        public class Address
        {
            public Level Level { get; set; }
        }

        [DataContract(Name = "level")]
        public enum Level
        {
            [EnumMember(Value = "low")]
            Low,

            [EnumMember(Value = "veryhigh")]
            High
        }
    }

    public class MyExpressionBinder : ExpressionBinderBase
    {
        public MyExpressionBinder(IEdmModel model, ODataQuerySettings querySettings) : base(model, querySettings)
        {
        }

        protected override ParameterExpression Parameter => throw new NotImplementedException();

        public override Expression Bind(QueryNode node)
        {
            throw new NotImplementedException();
        }
    }
}
