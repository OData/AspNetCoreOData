//-----------------------------------------------------------------------------
// <copyright file="ODataPathSegmentHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// A handler used to calculate some values based on the odata path.
    /// </summary>
    public class ODataPathSegmentHandler : PathSegmentHandler
    {
        private readonly IList<string> _pathUriLiteral;
        private IEdmNavigationSource _navigationSource; // used to record the navigation source in the last segment.

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathSegmentHandler"/> class.
        /// </summary>
        public ODataPathSegmentHandler()
        {
            _navigationSource = null;
            _pathUriLiteral = new List<string>();
        }

        /// <summary>
        /// Gets the path navigation source.
        /// </summary>
        public IEdmNavigationSource NavigationSource => _navigationSource;

        /// <summary>
        /// Gets the path Uri literal.
        /// </summary>
        public string PathLiteral => string.Join("/", _pathUriLiteral);

        /// <summary>
        /// Handle an <see cref="EntitySetSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(EntitySetSegment segment)
        {
            Contract.Assert(segment != null);

            _navigationSource = segment.EntitySet;
            _pathUriLiteral.Add(segment.EntitySet.Name);
        }

        /// <summary>
        /// Handle a <see cref="KeySegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(KeySegment segment)
        {
            Contract.Assert(segment != null);

            _navigationSource = segment.NavigationSource;

            string value = ConvertKeysToString(segment.Keys, segment.EdmType);

            // update the previous segment Uri literal
            if (!_pathUriLiteral.Any())
            {
                _pathUriLiteral.Add("(" + value + ")");
                return;
            }

            if (_pathUriLiteral.Last() == ODataSegmentKinds.Ref)
            {
                _pathUriLiteral[_pathUriLiteral.Count - 2] =
                    _pathUriLiteral[_pathUriLiteral.Count - 2] + "(" + value + ")";
            }
            else
            {
                _pathUriLiteral[_pathUriLiteral.Count - 1] =
                    _pathUriLiteral[_pathUriLiteral.Count - 1] + "(" + value + ")";
            }
        }

        /// <summary>
        /// Handle a <see cref="NavigationPropertyLinkSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(NavigationPropertyLinkSegment segment)
        {
            Contract.Assert(segment != null);
            _navigationSource = segment.NavigationSource;

            _pathUriLiteral.Add(segment.NavigationProperty.Name);
            _pathUriLiteral.Add(ODataSegmentKinds.Ref);
        }

        /// <summary>
        /// Handle a <see cref="NavigationPropertySegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(NavigationPropertySegment segment)
        {
            Contract.Assert(segment != null);
            _navigationSource = segment.NavigationSource;

            _pathUriLiteral.Add(segment.NavigationProperty.Name);
        }

        /// <summary>
        /// Handle a <see cref="DynamicPathSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(DynamicPathSegment segment)
        {
            Contract.Assert(segment != null);
            _navigationSource = null;

            _pathUriLiteral.Add(segment.Identifier);
        }

        /// <summary>
        /// Handle a <see cref="OperationImportSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(OperationImportSegment segment)
        {
            Contract.Assert(segment != null);

            _navigationSource = segment.EntitySet;

            IEdmActionImport actionImport = segment.OperationImports.Single() as IEdmActionImport;

            if (actionImport != null)
            {
                _pathUriLiteral.Add(actionImport.Name);
            }
            else
            {
                // Translate the nodes in ODL path to string literals as parameter of UnboundFunctionPathSegment.
                Dictionary<string, string> parameterValues = segment.Parameters.ToDictionary(
                    parameterValue => parameterValue.Name,
                    parameterValue => TranslateNode(parameterValue.Value));

                IEdmFunctionImport function = (IEdmFunctionImport)segment.OperationImports.Single();

                IEnumerable<string> parameters = parameterValues.Select(v => String.Format(CultureInfo.InvariantCulture, "{0}={1}", v.Key, v.Value));
                string literal = string.Format(CultureInfo.InvariantCulture, "{0}({1})", function.Name, String.Join(",", parameters));

                _pathUriLiteral.Add(literal);
            }
        }

        /// <summary>
        /// Handle an <see cref="OperationSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(OperationSegment segment)
        {
            Contract.Assert(segment != null);
            _navigationSource = segment.EntitySet;

            IEdmAction action = segment.Operations.Single() as IEdmAction;

            if (action != null)
            {
                _pathUriLiteral.Add(action.FullName());
            }
            else
            {
                // Translate the nodes in ODL path to string literals as parameter of BoundFunctionPathSegment.
                Dictionary<string, string> parameterValues = segment.Parameters.ToDictionary(
                    parameterValue => parameterValue.Name,
                    parameterValue => TranslateNode(parameterValue.Value));

                // TODO: refactor the function literal for parameter alias
                IEdmFunction function = (IEdmFunction)segment.Operations.Single();

                IEnumerable<string> parameters = parameterValues.Select(v => String.Format(CultureInfo.InvariantCulture, "{0}={1}", v.Key, v.Value));
                string literal = String.Format(CultureInfo.InvariantCulture, "{0}({1})", function.FullName(), String.Join(",", parameters));

                _pathUriLiteral.Add(literal);
            }
        }

        /// <summary>
        /// Handle a <see cref="PathTemplateSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(PathTemplateSegment segment)
        {
            Contract.Assert(segment != null);
            _navigationSource = null;

            _pathUriLiteral.Add(segment.LiteralText);
        }

        /// <summary>
        /// Handle a <see cref="PropertySegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(PropertySegment segment)
        {
            Contract.Assert(segment != null);
            // Not setting navigation source to null as the relevant navigation source for the path will be the previous navigation source.

            _pathUriLiteral.Add(segment.Property.Name);
        }

        /// <summary>
        /// Handle a <see cref="SingletonSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(SingletonSegment segment)
        {
            Contract.Assert(segment != null);
            _navigationSource = segment.Singleton;

            _pathUriLiteral.Add(segment.Singleton.Name);
        }

        /// <summary>
        /// Handle a <see cref="TypeSegment"/>, we use "cast" for type segment.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(TypeSegment segment)
        {
            Contract.Assert(segment != null);
            _navigationSource = segment.NavigationSource;

            // Uri literal does not use the collection type.
            IEdmType elementType = segment.EdmType;
            if (segment.EdmType.TypeKind == EdmTypeKind.Collection)
            {
                elementType = ((IEdmCollectionType)segment.EdmType).ElementType.Definition;
            }

            _pathUriLiteral.Add(elementType.FullTypeName());
        }

        /// <summary>
        /// Handle a <see cref="ValueSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(ValueSegment segment)
        {
            // do nothing for the navigation source for $value.
            // It means to use the previous the navigation source
            Contract.Assert(segment != null);

            _pathUriLiteral.Add(ODataSegmentKinds.Value);
        }

        /// <summary>
        /// Handle a <see cref="CountSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(CountSegment segment)
        {
            Contract.Assert(segment != null);
            _navigationSource = null;

            _pathUriLiteral.Add(ODataSegmentKinds.Count);
        }

        /// <summary>
        /// Handle a <see cref="BatchSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(BatchSegment segment)
        {
            Contract.Assert(segment != null);
            _navigationSource = null;

            _pathUriLiteral.Add(ODataSegmentKinds.Batch);
        }

        /// <summary>
        /// Handle a <see cref="MetadataSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(MetadataSegment segment)
        {
            Contract.Assert(segment != null);
            _navigationSource = null;

            _pathUriLiteral.Add(ODataSegmentKinds.Metadata);
        }

        // Convert the objects of keys in ODL path to string literals.
        internal static string ConvertKeysToString(IEnumerable<KeyValuePair<string, object>> keys, IEdmType edmType)
        {
            Contract.Assert(keys != null);

            IEdmEntityType entityType = edmType as IEdmEntityType;
            Contract.Assert(entityType != null);

            IList<KeyValuePair<string, object>> keyValuePairs = keys as IList<KeyValuePair<string, object>> ?? keys.ToList();
            if (keyValuePairs.Count < 1)
            {
                return string.Empty;
            }

            if (keyValuePairs.Count < 2)
            {
                var keyValue = keyValuePairs.First();
                bool isDeclaredKey = entityType.Key().Any(k => k.Name == keyValue.Key);

                // To support the alternate key
                if (isDeclaredKey)
                {
                    return string.Join(
                        ",",
                        keyValuePairs.Select(keyValuePair =>
                            TranslateNode(keyValuePair.Value)).ToArray());
                }
            }

            return string.Join(
                ",",
                keyValuePairs.Select(keyValuePair =>
                    (keyValuePair.Key +
                     "=" +
                     TranslateNode(keyValuePair.Value))).ToArray());
        }

        internal static string TranslateNode(object node)
        {
            ConstantNode constantNode = node as ConstantNode;
            if (constantNode != null)
            {
                UriTemplateExpression uriTemplateExpression = constantNode.Value as UriTemplateExpression;
                if (uriTemplateExpression != null)
                {
                    return uriTemplateExpression.LiteralText;
                }

                // Make the enum prefix free to work.
                ODataEnumValue enumValue = constantNode.Value as ODataEnumValue;
                if (enumValue != null)
                {
                    return ODataUriUtils.ConvertToUriLiteral(enumValue, ODataVersion.V4);
                }

                return constantNode.LiteralText;
            }

            ConvertNode convertNode = node as ConvertNode;
            if (convertNode != null)
            {
                return TranslateNode(convertNode.Source);
            }

            ParameterAliasNode parameterAliasNode = node as ParameterAliasNode;
            if (parameterAliasNode != null)
            {
                return parameterAliasNode.Alias;
            }

            return ODataUriUtils.ConvertToUriLiteral(node, ODataVersion.V4);
        }
    }
}
