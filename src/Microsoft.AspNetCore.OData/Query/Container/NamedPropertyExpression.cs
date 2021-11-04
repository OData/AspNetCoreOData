//-----------------------------------------------------------------------------
// <copyright file="NamedPropertyExpression.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace Microsoft.AspNetCore.OData.Query.Container
{
    /// <summary>
    /// Represents a container that captures a named property that is a part of the select expand query.
    /// </summary>
    public class NamedPropertyExpression
    {
        public NamedPropertyExpression(Expression name, Expression value)
        {
            Contract.Assert(name != null);
            Contract.Assert(value != null);

            Name = name;
            Value = value;
        }

        public Expression Name { get; private set; }

        public Expression Value { get; private set; }

        internal Expression TotalCount { get; set; }

        // Checks whether this property is null or not. This is required for expanded navigation properties that are null as entityframework cannot
        // create null's of type SelectExpandWrapper<ExpandedProperty> i.e. an expression like 
        //       => new NamedProperty<Customer> { Value = order.Customer == null : null : new SelectExpandWrapper<Customer> { .... } } 
        // cannot be translated by EF. So, we generate the following expression instead,
        //       => new ExpandProperty<Customer> { Value = new SelectExpandWrapper<Customer> { .... }, IsNull = nullCheck }
        // and use Value only if IsNull is false.
        internal Expression NullCheck { get; set; }

        internal int? PageSize { get; set; }

        internal bool AutoSelected { get; set; }

        internal bool? CountOption { get; set; }
    }
}
