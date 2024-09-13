using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Tests.Query.Expressions
{
    public class FakeSingleEntityNode : SingleEntityNode
    {
        private readonly IEdmEntityTypeReference typeReference;
        private readonly IEdmEntitySetBase set;

        public FakeSingleEntityNode(IEdmEntityTypeReference type, IEdmEntitySetBase set)
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

        public static FakeSingleEntityNode CreateFakeNodeForPerson()
        {
            var personType = EdmModelBuilder.GetEntityType("Microsoft.FullyQualified.NS.Person");
            return new FakeSingleEntityNode(EdmModelBuilder.GetEntityTypeReference(personType), EdmModelBuilder.GetPeopleSet());
        }
    }

    public class FakeCollectionResourceNode : CollectionResourceNode
    {
        private readonly IEdmStructuredTypeReference _typeReference;
        private readonly IEdmNavigationSource _source;
        private readonly IEdmTypeReference _itemType;
        private readonly IEdmCollectionTypeReference _collectionType;

        public FakeCollectionResourceNode(IEdmStructuredTypeReference type, IEdmNavigationSource source, IEdmTypeReference itemType, IEdmCollectionTypeReference collectionType)
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

        public static FakeCollectionResourceNode CreateFakeNodeForPerson()
        {
            var singleEntityNode = FakeSingleEntityNode.CreateFakeNodeForPerson();
            return new FakeCollectionResourceNode(
                singleEntityNode.EntityTypeReference, singleEntityNode.NavigationSource, singleEntityNode.EntityTypeReference, singleEntityNode.EntityTypeReference.AsCollection());
        }
    }
}
