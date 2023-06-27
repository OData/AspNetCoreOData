using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace ODataQueryBuilder.Query.Container
{
    /// <summary>
    /// Represents a container that captures a named property that is a part of the select expand query.
    /// </summary>
    public class NamedPropertyExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPropertyExpression" /> class.
        /// </summary>
        /// <param name="name">Property name expression.</param>
        /// <param name="value">Property value expression.</param>
        public NamedPropertyExpression(Expression name, Expression value)
        {
            Contract.Assert(name != null);
            Contract.Assert(value != null);

            Name = name;
            Value = value;
        }

        /// <summary>
        /// Property name expression.
        /// </summary>
        public Expression Name { get; private set; }

        /// <summary>
        /// Property value expression.
        /// </summary>
        public Expression Value { get; private set; }

        /// <summary>
        /// The total count expression.
        /// </summary>
        public Expression TotalCount { get; set; }

        // Checks whether this property is null or not. This is required for expanded navigation properties that are null as entityframework cannot
        // create null's of type SelectExpandWrapper<ExpandedProperty> i.e. an expression like 
        //       => new NamedProperty<Customer> { Value = order.Customer == null : null : new SelectExpandWrapper<Customer> { .... } } 
        // cannot be translated by EF. So, we generate the following expression instead,
        //       => new ExpandProperty<Customer> { Value = new SelectExpandWrapper<Customer> { .... }, IsNull = nullCheck }
        // and use Value only if IsNull is false.
        /// <summary>
        /// The null check expression.
        /// </summary>
        public Expression NullCheck { get; set; }

        /// <summary>
        /// The page size value.
        /// </summary>
        public int? PageSize { get; set; }

        // Option that indicates if we are creating this expression
        //  Next = new AutoSelectedNamedProperty()
        //      {
        //          Name = "...",
        //          Value = ...
        //      }
        /// <summary>
        /// The bool option to indicate if we are creating an AutoSelectedNamedProperty expression.
        /// </summary>
        public bool AutoSelected { get; set; }

        /// <summary>
        /// Bool option that indicates if we have count value.
        /// </summary>
        public bool? CountOption { get; set; }
    }
}
