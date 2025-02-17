namespace OData2Linq.Tests
{
    using Microsoft.AspNetCore.OData.Query.Wrapper;
    using Microsoft.OData;
    using OData2Linq.Tests.SampleData;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class SelectTests
    {
        [Fact]
        public void SelectDefault()
        {
            ISelectExpandWrapper[] result = SimpleClass.CreateQuery().OData().SelectExpand().ToArray();

            IDictionary<string, object> metadata = result[0].ToDictionary();

            Assert.Equal(SimpleClass.NumberOfProperties, metadata.Count);
        }

        [Fact]
        public void SelectAllt()
        {
            ISelectExpandWrapper[] result = SimpleClass.CreateQuery().OData().SelectExpand("*").ToArray();

            IDictionary<string, object> metadata = result[0].ToDictionary();

            Assert.Equal(SimpleClass.NumberOfProperties, metadata.Count);
        }

        [Fact]
        public void SelectName()
        {
            ISelectExpandWrapper[] result = SimpleClass.CreateQuery().OData().SelectExpand("Name").ToArray();

            IDictionary<string, object> metadata = result[0].ToDictionary();

            // Expect Name to be selected
            Assert.Equal(1, metadata.Count);
            Assert.Equal("Name", metadata.Single().Key);
            Assert.Equal("n1", metadata.Single().Value);
            Assert.IsType<string>(metadata.Single().Value);
        }

        [Fact]
        public void SelectDataMember()
        {
            ISelectExpandWrapper[] result = SimpleClassDataContract.CreateQuery().OData().SelectExpand("nameChanged").ToArray();

            IDictionary<string, object> metadata = result[0].ToDictionary();

            // Expect Name to be selected
            Assert.Equal(1, metadata.Count);
            Assert.Equal("nameChanged", metadata.Single().Key);
            Assert.Equal("n1", metadata.Single().Value);
            Assert.IsType<string>(metadata.Single().Value);
        }

        [Fact]
        public void SelectId()
        {
            ISelectExpandWrapper[] result = SimpleClass.CreateQuery().OData().SelectExpand("Id").ToArray();

            IDictionary<string, object> metadata = result[0].ToDictionary();

            Assert.Equal(1, metadata.Count);
            Assert.Equal("Id", metadata.Single().Key);
            Assert.IsType<int>(metadata.Single().Value);
            Assert.Equal(1, metadata.Single().Value);
        }

        [Fact]
        public void SelectIdCaseInsensitive()
        {
            ISelectExpandWrapper[] result = SimpleClass.CreateQuery().OData().SelectExpand("id").ToArray();

            IDictionary<string, object> metadata = result[0].ToDictionary();

            Assert.Equal(1, metadata.Count);
            Assert.Equal("Id", metadata.Single().Key, StringComparer.Ordinal);
            Assert.IsType<int>(metadata.Single().Value);
            Assert.Equal(1, metadata.Single().Value);
        }

        [Fact]
        public void SelectCaseSensitiveOnDemand()
        {
            Assert.Throws<ODataException>(
                () => SimpleClass.CreateQuery().OData(s => s.EnableCaseInsensitive = false).SelectExpand("id"));
        }

        [Fact]
        public void SelectNotExistingProperty()
        {
            Assert.Throws<ODataException>(
                () => SimpleClass.CreateQuery().OData().SelectExpand("asdcaefacfawrcfwrfaw4"));
        }

        [Fact]
        public void SelectNameToIgnore()
        {
            Assert.Throws<ODataException>(
                () => SimpleClass.CreateQuery().OData().SelectExpand("NameToIgnore"));
        }

        [Fact]
        public void SelectDisabled()
        {
            ISelectExpandWrapper[] result = ClassWithLink.CreateQuery().OData().SelectExpand("Link4").ToArray();

            IDictionary<string, object> metadata = result[0].ToDictionary();

            Assert.Equal(0, metadata.Count);
        }

        [Fact]
        public void SelectLinkWithoutExpandNotWorking()
        {
            ISelectExpandWrapper[] result = ClassWithLink.CreateQuery().OData().SelectExpand("Link1").ToArray();

            IDictionary<string, object> metadata = result[0].ToDictionary();

            Assert.Equal(0, metadata.Count);
        }
    }
}