//-----------------------------------------------------------------------------
// <copyright file="MockEdmNodesHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Tests.Query.Expressions
{
    public class MockSingleEntityNode : SingleEntityNode
    {
        private readonly IEdmEntityTypeReference typeReference;
        private readonly IEdmEntitySetBase set;

        public MockSingleEntityNode(IEdmEntityTypeReference type, IEdmEntitySetBase set)
        {
            this.typeReference = type;
            this.set = set;
        }

        public override IEdmTypeReference TypeReference
        {
            get { return this.typeReference; }
        }

        public override IEdmNavigationSource NavigationSource
        {
            get { return this.set; }
        }

        public override IEdmStructuredTypeReference StructuredTypeReference
        {
            get { return this.typeReference; }
        }

        public override IEdmEntityTypeReference EntityTypeReference
        {
            get { return this.typeReference; }
        }

        public static MockSingleEntityNode CreateFakeNodeForEmployee()
        {
            var employeeType = HardCodedTestModel.GetEntityType("Microsoft.AspNetCore.OData.Tests.Models.Employee");
            return new MockSingleEntityNode(HardCodedTestModel.GetEntityTypeReference(employeeType), HardCodedTestModel.GetEmployeeSet());
        }
    }

    public class MockCollectionResourceNode : CollectionResourceNode
    {
        private readonly IEdmStructuredTypeReference _typeReference;
        private readonly IEdmNavigationSource _source;
        private readonly IEdmTypeReference _itemType;
        private readonly IEdmCollectionTypeReference _collectionType;

        public MockCollectionResourceNode(IEdmStructuredTypeReference type, IEdmNavigationSource source, IEdmTypeReference itemType, IEdmCollectionTypeReference collectionType)
        {
            _typeReference = type;
            _source = source;
            _itemType = itemType;
            _collectionType = collectionType;
        }

        public override IEdmStructuredTypeReference ItemStructuredType => _typeReference;

        public override IEdmNavigationSource NavigationSource => _source;

        public override IEdmTypeReference ItemType => _itemType;

        public override IEdmCollectionTypeReference CollectionType => _collectionType;

        public static MockCollectionResourceNode CreateFakeNodeForEmployee()
        {
            var singleEntityNode = MockSingleEntityNode.CreateFakeNodeForEmployee();
            return new MockCollectionResourceNode(
                singleEntityNode.EntityTypeReference, singleEntityNode.NavigationSource, singleEntityNode.EntityTypeReference, singleEntityNode.EntityTypeReference.AsCollection());
        }
    }
}
