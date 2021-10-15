//-----------------------------------------------------------------------------
// <copyright file="ODataPathNavigationSourceHandler.cs" company=".NET Foundation">
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
    public class ODataPathNavigationSourceHandler : PathSegmentHandler
    {
        private readonly IList<string> _path;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathNavigationSourceHandler"/> class.
        /// </summary>
        public ODataPathNavigationSourceHandler()
        {
            _path = new List<string>();
        }

        /// <summary>
        /// Gets the path navigation source.
        /// </summary>
        public IEdmNavigationSource NavigationSource { get; private set; }

        /// <summary>
        /// Gets the path template.
        /// </summary>
        public string Path
        {
            get { return string.Join("/", _path); }
        }

        /// <summary>
        /// Handle an <see cref="EntitySetSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(EntitySetSegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = segment.EntitySet;
            _path.Add(segment.EntitySet.Name);
        }

        /// <summary>
        /// Handle a <see cref="KeySegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(KeySegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = segment.NavigationSource;
        }

        /// <summary>
        /// Handle a <see cref="NavigationPropertyLinkSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle</param>
        public override void Handle(NavigationPropertyLinkSegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = segment.NavigationSource;

            _path.Add(segment.NavigationProperty.Name);
            _path.Add(ODataSegmentKinds.Ref);
        }

        /// <summary>
        /// Handle a <see cref="NavigationPropertySegment"/>.
        /// </summary>
        /// <param name="segment">the segment to Handle</param>
        public override void Handle(NavigationPropertySegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = segment.NavigationSource;
            _path.Add(segment.NavigationProperty.Name);
        }

        /// <summary>
        /// Handle a <see cref="DynamicPathSegment"/>.
        /// </summary>
        /// <param name="segment">the segment to Handle</param>
        public override void Handle(DynamicPathSegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = null;
            _path.Add(segment.Identifier);
        }

        /// <summary>
        /// Handle an <see cref="OperationImportSegment"/>.
        /// </summary>
        /// <param name="segment">the segment to Handle</param>
        public override void Handle(OperationImportSegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = segment.EntitySet;

            IEdmActionImport actionImport = segment.OperationImports.Single() as IEdmActionImport;

            if (actionImport != null)
            {
                _path.Add(actionImport.Name);
            }
            else
            {
                IEdmFunctionImport function = (IEdmFunctionImport)segment.OperationImports.Single();

                IList<string> parameterValues = new List<string>();
                foreach (var parameter in segment.Parameters)
                {
                    var functionParameter = function.Function.Parameters.FirstOrDefault(p => p.Name == parameter.Name);
                    if (functionParameter == null)
                    {
                        continue;
                    }

                    parameterValues.Add(functionParameter.Type.FullName());
                }

                string literal = string.Format(CultureInfo.InvariantCulture, "{0}({1})", function.Name, string.Join(",", parameterValues));

                _path.Add(literal);
            }
        }

        /// <summary>
        /// Handle an <see cref="OperationSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(OperationSegment segment)
        {
            Contract.Assert(segment != null);
            NavigationSource = segment.EntitySet;

            IEdmAction action = segment.Operations.Single() as IEdmAction;

            if (action != null)
            {
                _path.Add(action.FullName());
            }
            else
            {
                IEdmFunction function = (IEdmFunction)segment.Operations.Single();

                IList<string> parameterValues = new List<string>();
                foreach (var parameter in segment.Parameters)
                {
                    var functionParameter = function.Parameters.FirstOrDefault(p => p.Name == parameter.Name);
                    if (functionParameter == null)
                    {
                        continue;
                    }

                    parameterValues.Add(functionParameter.Type.FullName());
                }

                string literal = string.Format(CultureInfo.InvariantCulture, "{0}({1})", function.FullName(), string.Join(",", parameterValues));

                _path.Add(literal);
            }
        }

        /// <summary>
        /// Handle a <see cref="PathTemplateSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(PathTemplateSegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = null;
            _path.Add(segment.LiteralText);
        }

        /// <summary>
        /// Handle a <see cref="PropertySegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(PropertySegment segment)
        {
            Contract.Assert(segment != null);

            // Not set navigation source to null as the relevant navigation source for the path will be the previous navigation source.

            _path.Add(segment.Property.Name);
        }

        /// <summary>
        /// Handle a <see cref="SingletonSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(SingletonSegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = segment.Singleton;
            _path.Add(segment.Singleton.Name);
        }

        /// <summary>
        /// Handle a <see cref="TypeSegment"/>, we use "cast" for type segment.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(TypeSegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = segment.NavigationSource;

            // Uri literal does not use the collection type.
            IEdmType elementType = segment.EdmType;
            if (segment.EdmType.TypeKind == EdmTypeKind.Collection)
            {
                elementType = ((IEdmCollectionType)segment.EdmType).ElementType.Definition;
            }

            _path.Add(elementType.FullTypeName());
        }

        /// <summary>
        /// Handle a <see cref="ValueSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(ValueSegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = null;
            _path.Add(ODataSegmentKinds.Value);
        }

        /// <summary>
        /// Handle a <see cref="CountSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(CountSegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = null;
            _path.Add(ODataSegmentKinds.Count);
        }

        /// <summary>
        /// Handle a <see cref="BatchSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(BatchSegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = null;
            _path.Add(ODataSegmentKinds.Batch);
        }

        /// <summary>
        /// Handle a <see cref="MetadataSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(MetadataSegment segment)
        {
            Contract.Assert(segment != null);

            NavigationSource = null;
            _path.Add(ODataSegmentKinds.Metadata);
        }

        /// <summary>
        /// Handle a <see cref="FilterSegment"/>.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(FilterSegment segment)
        {
            Contract.Assert(segment != null);

            _path.Add("$filter");
        }

        /// <summary>
        /// Handle a general path segment.
        /// </summary>
        /// <param name="segment">The segment to handle.</param>
        public override void Handle(ODataPathSegment segment)
        {
            // ODL doesn't provide the handle function for general path segment
            Contract.Assert(segment != null);

            NavigationSource = null;
            _path.Add(segment.ToString());
        }
    }
}
