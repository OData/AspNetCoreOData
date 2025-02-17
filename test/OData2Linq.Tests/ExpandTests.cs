namespace OData2Linq.Tests
{
    using Microsoft.AspNetCore.OData.Query.Wrapper;
    using Microsoft.OData;
    using OData2Linq.Tests.SampleData;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class ExpandTests
    {
        [Fact]
        public void EmptyExpand()
        {
            ISelectExpandWrapper[] result = ClassWithLink.CreateQuery().OData().SelectExpand().ToArray();

            IDictionary<string, object> metadata = result[0].ToDictionary();

            // Not expanded by default except auto expand attribute
            Assert.Equal(2, metadata.Count);
        }

        [Fact]
        public void EmptyExpandSelectAll()
        {
            ISelectExpandWrapper[] result = ClassWithLink.CreateQuery().OData().SelectExpand("*").ToArray();

            IDictionary<string, object> metadata = result[0].ToDictionary();

            // Not expanded by default except auto expand attribute
            Assert.Equal(2, metadata.Count);
        }

        [Fact]
        public void ExpandLink()
        {
            ISelectExpandWrapper[] result = ClassWithLink.CreateQuery().OData().SelectExpand(null, "Link1").ToArray();

            IDictionary<string, object> metadata = result[0].ToDictionary();

            // Not expanded by default
            Assert.Equal(3, metadata.Count);

            Assert.Equal(SimpleClass.NumberOfProperties, (metadata["Link1"] as ISelectExpandWrapper).ToDictionary().Count);
        }

        [Fact]
        public void ExpandSelect()
        {
            ISelectExpandWrapper[] result = ClassWithLink.CreateQuery().OData().SelectExpand("Name,Link1", "Link1").ToArray();

            IDictionary<string, object> metadata = result[0].ToDictionary();

            // Not expanded by default
            Assert.Equal(2, metadata.Count);
            Assert.Equal(SimpleClass.NumberOfProperties, (metadata["Link1"] as ISelectExpandWrapper).ToDictionary().Count);
        }

        [Fact]
        public void ExpandLinkSelect()
        {
            ISelectExpandWrapper[] result = ClassWithLink.CreateQuery().OData().SelectExpand("Name", "Link1($select=Name)").ToArray();

            IDictionary<string, object> metadata = result[0].ToDictionary();

            // Not expanded by default
            Assert.Equal(2, metadata.Count);
            Assert.Equal(1, (metadata["Link1"] as ISelectExpandWrapper).ToDictionary().Count);
        }

        [Fact]
        public void ExpandCollection()
        {
            ISelectExpandWrapper[] result = ClassWithCollection.CreateQuery().OData().SelectExpand("Name", "Link2").ToArray();

            IDictionary<string, object> metadata = result[0].ToDictionary();

            // Not expanded by default
            Assert.Equal(2, metadata.Count);
            Assert.Equal(2, (metadata["Link2"] as IEnumerable<ISelectExpandWrapper>).Count());
        }

        [Fact]
        public void ExpandCollectionWithTop()
        {
            ISelectExpandWrapper[] result = ClassWithCollection.CreateQuery().OData().SelectExpand("Name", "Link2($top=1)").ToArray();

            IDictionary<string, object> metadata = result[0].ToDictionary();

            // Not expanded by default
            Assert.Equal(2, metadata.Count);
            Assert.Single((metadata["Link2"] as IEnumerable<ISelectExpandWrapper>));
        }

        [Fact]
        public void ExpandCollectionWithTopDefaultPageSize()
        {
            ISelectExpandWrapper[] result = ClassWithCollection.CreateQuery().OData(s => s.QuerySettings.PageSize = 1).SelectExpand("Name", "Link2").ToArray();

            IDictionary<string, object> metadata = result[0].ToDictionary();

            // Not expanded by default
            Assert.Equal(2, metadata.Count);
            Assert.Single((metadata["Link2"] as IEnumerable<ISelectExpandWrapper>));
        }

        [Fact]
        public void ExpandCollectionWithTop21()
        {
            ISelectExpandWrapper[] result = ClassWithCollection.CreateQuery().OData().SelectExpand("Name", "Link2($top=21)").ToArray();

            IDictionary<string, object> metadata = result[0].ToDictionary();

            // Not expanded by default
            Assert.Equal(2, metadata.Count);
            Assert.Equal(2, (metadata["Link2"] as IEnumerable<ISelectExpandWrapper>).Count());
        }

        [Fact]
        public void ExpandCollectionWithTopExceedLimit()
        {
            Assert.Throws<ODataException>(
               () => ClassWithCollection.CreateQuery().OData().SelectExpand("Name", "Link2($top=101)"));
        }

        [Fact]
        public void ExpandCollectionWithFilterAndSelect()
        {
            ISelectExpandWrapper[] result = ClassWithCollection.CreateQuery().OData().SelectExpand("Name", "Link2($filter=Id eq 311;$select=Name)").ToArray();

            IDictionary<string, object> metadata = result[0].ToDictionary();

            // Not expanded by default
            Assert.Equal(2, metadata.Count);
            IEnumerable<ISelectExpandWrapper> collection = metadata["Link2"] as IEnumerable<ISelectExpandWrapper>;
            Assert.Single(collection);

            Assert.Equal(1, collection.Single().ToDictionary().Count);
        }

        [Fact]
        public void ExpandCollectionWithNotExpandable()
        {
            Assert.Throws<ODataException>(
               () => SampleWithCustomKey.CreateQuery().OData().SelectExpand(nameof(SampleWithCustomKey.Name), "NotExpandableLink"));
        }

        [Fact]
        public void ExpandWithAttributes()
        {
            ISelectExpandWrapper[] result = SampleWithCustomKey.CreateQuery().OData().SelectExpand().ToArray();

            IDictionary<string, object> metadata = result[0].ToDictionary();

            // Not expanded by default
            Assert.Equal(4, metadata.Count);

            Assert.Equal(SimpleClass.NumberOfProperties, (metadata["AutoExpandLink"] as ISelectExpandWrapper).ToDictionary().Count);
            Assert.Equal(SimpleClass.NumberOfProperties, (metadata["AutoExpandAndSelectLink"] as ISelectExpandWrapper).ToDictionary().Count);
        }

        [Fact]
        public void ExpandWithAttributesAndExplicit()
        {
            ISelectExpandWrapper[] result = SampleWithCustomKey.CreateQuery().OData().SelectExpand("*", "AutoExpandAndSelectLink").ToArray();

            IDictionary<string, object> metadata = result[0].ToDictionary();

            // Not expanded by default
            Assert.Equal(3, metadata.Count);

            Assert.Equal(SimpleClass.NumberOfProperties, (metadata["AutoExpandAndSelectLink"] as ISelectExpandWrapper).ToDictionary().Count);
        }

        [Fact]
        public void ExpandMaxDeepNotExceed()
        {
            ISelectExpandWrapper[] result = SampleWithCustomKey.CreateQuery().OData().SelectExpand(null, "RecursiveLink($expand=RecursiveLink)").ToArray();

            IDictionary<string, object> metadata = result[0].ToDictionary();

            // Not expanded by default
            Assert.Equal(5, metadata.Count);

            Assert.NotNull(metadata["RecursiveLink"]);
        }

        [Fact]
        public void ExpandMaxDeepExceed()
        {
            Assert.Throws<ODataException>(
               () => SampleWithCustomKey.CreateQuery().OData().SelectExpand(null, "RecursiveLink($expand=RecursiveLink($expand=RecursiveLink))"));
        }

        [Fact]
        public void ExpandMaxDeepSetInValidationSettings()
        {
            ISelectExpandWrapper[] result = ClassWithDeepNavigation.CreateQuery().OData(settings => settings.ValidationSettings.MaxExpansionDepth = 3).SelectExpand(null, "D1($expand=D2($expand=D3))").ToArray();

            IDictionary<string, object> metadata = result[0].ToDictionary();

            Assert.Equal(3, metadata.Count);
            Assert.Equal("n1123", (((metadata["D1"] as ISelectExpandWrapper).ToDictionary()["D2"] as ISelectExpandWrapper).ToDictionary()["D3"] as ISelectExpandWrapper).ToDictionary()["Name"]);
        }
    }
}