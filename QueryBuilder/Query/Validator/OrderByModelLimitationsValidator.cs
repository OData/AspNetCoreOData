using QueryBuilder.Edm;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace QueryBuilder.Query.Validator
{
    internal class OrderByModelLimitationsValidator : QueryNodeVisitor<SingleValueNode>
    {
        private readonly IEdmModel _model;
        private readonly bool _enableOrderBy;
        private IEdmProperty _property;
        private IEdmStructuredType _structuredType;

        public OrderByModelLimitationsValidator(ODataQueryContext2 context, bool enableOrderBy)
        {
            _model = context.Model;
            _enableOrderBy = enableOrderBy;

            if (context.Path != null)
            {
                _property = context.TargetProperty;
                _structuredType = context.TargetStructuredType;
            }
        }

        public bool TryValidate(IEdmProperty property, IEdmStructuredType structuredType, OrderByClause orderByClause,
            bool explicitPropertiesDefined)
        {
            _property = property;
            _structuredType = structuredType;
            return TryValidate(orderByClause, explicitPropertiesDefined);
        }

        // Visits the expression to find the first node if any, that is not sortable and throws
        // an exception only if no explicit properties have been defined in AllowedOrderByProperties
        // on the ODataValidationSettings instance associated with this OrderByValidator.
        public bool TryValidate(OrderByClause orderByClause, bool explicitPropertiesDefined)
        {
            SingleValueNode invalidNode = orderByClause.Expression.Accept(this);
            if (invalidNode != null && !explicitPropertiesDefined)
            {
                throw new ODataException(Error.Format(SRResources.NotSortablePropertyUsedInOrderBy,
                    GetPropertyName(invalidNode)));
            }
            return invalidNode == null;
        }

        public override SingleValueNode Visit(SingleValuePropertyAccessNode nodeIn)
        {
            if (nodeIn.Source != null)
            {
                if (nodeIn.Source.Kind == QueryNodeKind.SingleNavigationNode)
                {
                    SingleNavigationNode singleNavigationNode = nodeIn.Source as SingleNavigationNode;
                    if (EdmHelpers.IsNotSortable(nodeIn.Property, singleNavigationNode.NavigationProperty,
                        singleNavigationNode.NavigationProperty.ToEntityType(), _model, _enableOrderBy))
                    {
                        return nodeIn;
                    }
                }
                else if (nodeIn.Source.Kind == QueryNodeKind.SingleComplexNode)
                {
                    SingleComplexNode singleComplexNode = nodeIn.Source as SingleComplexNode;
                    if (EdmHelpers.IsNotSortable(nodeIn.Property, singleComplexNode.Property,
                        nodeIn.Property.DeclaringType, _model, _enableOrderBy))
                    {
                        return nodeIn;
                    }
                }
                else if (EdmHelpers.IsNotSortable(nodeIn.Property, _property, _structuredType, _model, _enableOrderBy))
                {
                    return nodeIn;
                }
            }

            if (nodeIn.Source != null)
            {
                return nodeIn.Source.Accept(this);
            }

            return null;
        }

        public override SingleValueNode Visit(SingleValueOpenPropertyAccessNode nodeIn)
        {
            return null;
        }

        public override SingleValueNode Visit(SingleComplexNode nodeIn)
        {
            if (EdmHelpers.IsNotSortable(nodeIn.Property, _property, _structuredType, _model, _enableOrderBy))
            {
                return nodeIn;
            }

            if (nodeIn.Source != null)
            {
                return nodeIn.Source.Accept(this);
            }

            return null;
        }

        public override SingleValueNode Visit(SingleNavigationNode nodeIn)
        {
            if (EdmHelpers.IsNotSortable(nodeIn.NavigationProperty, _property, _structuredType, _model,
                _enableOrderBy))
            {
                return nodeIn;
            }

            if (nodeIn.Source != null)
            {
                return nodeIn.Source.Accept(this);
            }

            return null;
        }

        public override SingleValueNode Visit(ResourceRangeVariableReferenceNode nodeIn)
        {
            return null;
        }

        public override SingleValueNode Visit(NonResourceRangeVariableReferenceNode nodeIn)
        {
            return null;
        }

        private static string GetPropertyName(SingleValueNode node)
        {
            if (node.Kind == QueryNodeKind.SingleNavigationNode)
            {
                return ((SingleNavigationNode)node).NavigationProperty.Name;
            }
            else if (node.Kind == QueryNodeKind.SingleValuePropertyAccess)
            {
                return ((SingleValuePropertyAccessNode)node).Property.Name;
            }
            else if (node.Kind == QueryNodeKind.SingleComplexNode)
            {
                return ((SingleComplexNode)node).Property.Name;
            }
            return null;
        }
    }
}
