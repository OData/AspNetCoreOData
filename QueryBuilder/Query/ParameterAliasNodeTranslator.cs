using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using ODataQueryBuilder.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataQueryBuilder;

namespace ODataQueryBuilder.Query
{
    /// <summary>
    /// This defines a translator to translate parameter alias nodes.
    /// </summary>
    internal class ParameterAliasNodeTranslator : QueryNodeVisitor<QueryNode>
    {
        private IDictionary<string, SingleValueNode> _parameterAliasNode;

        /// <summary>
        /// Initialize a new instance of <see cref="ParameterAliasNodeTranslator"/>.
        /// </summary>
        /// <param name="parameterAliasNodes">Parameter alias nodes mapping.</param>
        public ParameterAliasNodeTranslator(IDictionary<string, SingleValueNode> parameterAliasNodes)
        {
            if (parameterAliasNodes == null)
            {
                throw Error.ArgumentNull(nameof(parameterAliasNodes));
            }

            _parameterAliasNode = parameterAliasNodes;
        }

        /// <summary>
        /// Translate an AllNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(AllNode nodeIn)
        {
            Contract.Assert(nodeIn != null);

            AllNode allNode = new AllNode(nodeIn.RangeVariables, nodeIn.CurrentRangeVariable);

            if (nodeIn.Source != null)
            {
                allNode.Source = (CollectionNode)nodeIn.Source.Accept(this);
            }

            if (nodeIn.Body != null)
            {
                allNode.Body = (SingleValueNode)nodeIn.Body.Accept(this);
            }

            return allNode;
        }

        /// <summary>
        /// Translate an AnyNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(AnyNode nodeIn)
        {
            Contract.Assert(nodeIn != null);

            AnyNode anyNode = new AnyNode(nodeIn.RangeVariables, nodeIn.CurrentRangeVariable);

            if (nodeIn.Source != null)
            {
                anyNode.Source = (CollectionNode)nodeIn.Source.Accept(this);
            }

            if (nodeIn.Body != null)
            {
                anyNode.Body = (SingleValueNode)nodeIn.Body.Accept(this);
            }

            return anyNode;
        }

        /// <summary>
        /// Translate a BinaryOperatorNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(BinaryOperatorNode nodeIn)
        {
            Contract.Assert(nodeIn != null);

            return new BinaryOperatorNode(
                nodeIn.OperatorKind,
                (SingleValueNode)nodeIn.Left.Accept(this),
                (SingleValueNode)nodeIn.Right.Accept(this));
        }

        /// <summary>
        /// Translate an InNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(InNode nodeIn)
        {
            Contract.Assert(nodeIn != null);

            return new InNode(
                (SingleValueNode)nodeIn.Left.Accept(this),
                (CollectionNode)nodeIn.Right.Accept(this));
        }

        /// <summary>
        /// Translate a CollectionFunctionCallNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(CollectionFunctionCallNode nodeIn)
        {
            Contract.Assert(nodeIn != null);

            return new CollectionFunctionCallNode(
                nodeIn.Name,
                nodeIn.Functions,
                nodeIn.Parameters.Select(p => p.Accept(this)),
                nodeIn.CollectionType,
                nodeIn.Source == null ? null : nodeIn.Source.Accept(this));
        }

        /// <summary>
        /// Translate a CollectionNavigationNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(CollectionNavigationNode nodeIn)
        {
            Contract.Assert(nodeIn != null);

            return nodeIn.Source == null ?
                nodeIn :
                new CollectionNavigationNode(
                    (SingleResourceNode)nodeIn.Source.Accept(this),
                    nodeIn.NavigationProperty,
                    nodeIn.BindingPath ?? new EdmPathExpression(nodeIn.NavigationProperty.Name));
        }

        /// <summary>
        /// Translate a CollectionOpenPropertyAccessNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(CollectionOpenPropertyAccessNode nodeIn)
        {
            Contract.Assert(nodeIn != null);

            return new CollectionOpenPropertyAccessNode(
                (SingleValueNode)nodeIn.Source.Accept(this),
                nodeIn.Name);
        }

        /// <summary>
        /// Translate a CollectionComplexNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(CollectionComplexNode nodeIn)
        {
            Contract.Assert(nodeIn != null);

            return new CollectionComplexNode(
                (SingleResourceNode)nodeIn.Source.Accept(this),
                nodeIn.Property);
        }

        /// <summary>
        /// Translate a CollectionPropertyAccessNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(CollectionPropertyAccessNode nodeIn)
        {
            Contract.Assert(nodeIn != null);

            return new CollectionPropertyAccessNode(
                (SingleValueNode)nodeIn.Source.Accept(this),
                nodeIn.Property);
        }

        /// <summary>
        /// Translate a ConstantNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The original node.</returns>
        public override QueryNode Visit(ConstantNode nodeIn)
        {
            return nodeIn;
        }

        /// <summary>
        /// Translate a CollectionConstantNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The original node.</returns>
        public override QueryNode Visit(CollectionConstantNode nodeIn)
        {
            return nodeIn;
        }

        /// <summary>
        /// Translate a ConvertNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(ConvertNode nodeIn)
        {
            Contract.Assert(nodeIn != null);

            return new ConvertNode((SingleValueNode)nodeIn.Source.Accept(this), nodeIn.TypeReference);
        }

        /// <summary>
        /// Translate an CollectionResourceCastNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(CollectionResourceCastNode nodeIn)
        {
            Contract.Assert(nodeIn != null);

            return new CollectionResourceCastNode(
                (CollectionResourceNode)nodeIn.Source.Accept(this),
                (IEdmStructuredType)nodeIn.ItemType.Definition);
        }

        /// <summary>
        /// Translate an CollectionResourceFunctionCallNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(CollectionResourceFunctionCallNode nodeIn)
        {
            Contract.Assert(nodeIn != null);

            return new CollectionResourceFunctionCallNode(
                nodeIn.Name,
                nodeIn.Functions,
                nodeIn.Parameters.Select(p => p.Accept(this)),
                nodeIn.CollectionType,
                (IEdmEntitySetBase)nodeIn.NavigationSource,
                nodeIn.Source == null ? null : nodeIn.Source.Accept(this));
        }

        /// <summary>
        /// Translate an ResourceRangeVariableReferenceNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The original node.</returns>
        public override QueryNode Visit(ResourceRangeVariableReferenceNode nodeIn)
        {
            return nodeIn;
        }

        /// <summary>
        /// Translate a NamedFunctionParameterNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(NamedFunctionParameterNode nodeIn)
        {
            Contract.Assert(nodeIn != null);

            return new NamedFunctionParameterNode(
                nodeIn.Name,
                nodeIn.Value == null ? null : nodeIn.Value.Accept(this));
        }

        /// <summary>
        /// Translate a NonResourceRangeVariableReferenceNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The original node.</returns>
        public override QueryNode Visit(NonResourceRangeVariableReferenceNode nodeIn)
        {
            return nodeIn;
        }

        /// <summary>
        /// Translate a ParameterAliasNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(ParameterAliasNode nodeIn)
        {
            SingleValueNode node = ODataPathSegmentTranslator.TranslateParameterAlias(nodeIn, _parameterAliasNode);

            if (node == null)
            {
                return new ConstantNode(null);
            }
            else
            {
                return node.Accept(this);
            }
        }

        /// <summary>
        /// Translate a SearchTermNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The original node.</returns>
        public override QueryNode Visit(SearchTermNode nodeIn)
        {
            return nodeIn;
        }

        /// <summary>
        /// Translate a SingleResourceCastNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(SingleResourceCastNode nodeIn)
        {
            Contract.Assert(nodeIn != null);

            return nodeIn.Source == null ?
                nodeIn :
                new SingleResourceCastNode(
                    (SingleResourceNode)nodeIn.Source.Accept(this),
                    (IEdmStructuredType)nodeIn.TypeReference.Definition);
        }

        /// <summary>
        /// Translate a SingleResourceFunctionCallNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(SingleResourceFunctionCallNode nodeIn)
        {
            Contract.Assert(nodeIn != null);

            return new SingleResourceFunctionCallNode(
                nodeIn.Name,
                nodeIn.Functions,
                nodeIn.Parameters.Select(p => p.Accept(this)),
                nodeIn.StructuredTypeReference,
                nodeIn.NavigationSource,
                nodeIn.Source == null ? null : nodeIn.Source.Accept(this));
        }

        /// <summary>
        /// Translate a SingleNavigationNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(SingleNavigationNode nodeIn)
        {
            Contract.Assert(nodeIn != null);

            return nodeIn.Source == null ?
                nodeIn :
                new SingleNavigationNode(
                    (SingleResourceNode)nodeIn.Source.Accept(this),
                    nodeIn.NavigationProperty,
                    nodeIn.BindingPath ?? new EdmPathExpression(nodeIn.NavigationProperty.Name));
        }

        /// <summary>
        /// Translate a SingleValueFunctionCallNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(SingleValueFunctionCallNode nodeIn)
        {
            Contract.Assert(nodeIn != null);

            return new SingleValueFunctionCallNode(
                nodeIn.Name,
                nodeIn.Functions,
                nodeIn.Parameters.Select(p => p.Accept(this)),
                nodeIn.TypeReference,
                nodeIn.Source == null ? null : nodeIn.Source.Accept(this));
        }

        /// <summary>
        /// Translate a SingleValueOpenPropertyAccessNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(SingleValueOpenPropertyAccessNode nodeIn)
        {
            Contract.Assert(nodeIn != null);

            return new SingleValueOpenPropertyAccessNode(
                (SingleValueNode)nodeIn.Source.Accept(this),
                nodeIn.Name);
        }

        /// <summary>
        /// Translate a SingleValuePropertyAccessNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(SingleValuePropertyAccessNode nodeIn)
        {
            Contract.Assert(nodeIn != null);

            return new SingleValuePropertyAccessNode(
                (SingleValueNode)nodeIn.Source.Accept(this),
                nodeIn.Property);
        }

        /// <summary>
        /// Translate a SingleComplexNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(SingleComplexNode nodeIn)
        {
            Contract.Assert(nodeIn != null);

            return new SingleComplexNode(
                (SingleResourceNode)nodeIn.Source.Accept(this),
                nodeIn.Property);
        }

        /// <summary>
        /// Translate an UnaryOperatorNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(UnaryOperatorNode nodeIn)
        {
            Contract.Assert(nodeIn != null);

            return new UnaryOperatorNode(nodeIn.OperatorKind, (SingleValueNode)nodeIn.Operand.Accept(this));
        }

        /// <summary> 
        /// Translate a CountNode. 
        /// </summary> 
        /// <param name="nodeIn">The node to be translated.</param> 
        /// <returns>The translated node.</returns> 
        public override QueryNode Visit(CountNode nodeIn)
        {
            Contract.Assert(nodeIn != null);

            return new CountNode((CollectionNode)nodeIn.Source.Accept(this), nodeIn.FilterClause, nodeIn.SearchClause);
        }
    }
}
