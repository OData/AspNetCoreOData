namespace OData2Linq.Tests
{
    using Microsoft.AspNetCore.OData.Query;
    using OData2Linq.Tests.SampleData;
    using System;
    using System.Linq;
    using Xunit;

    public class OpenTypesTests
    {
        // Dynamic properties dictionary is null for one item

        [Fact]
        public void FilterDefault()
        {
            Assert.Throws<NullReferenceException>(() => OpenType.CreateQuery().OData().Filter("Name1 eq 'n1'").ToArray());
        }

        [Fact]
        public void FilterNullPropagationTrue()
        {
            var result = OpenType.CreateQuery().OData(c => c.QuerySettings.HandleNullPropagation = HandleNullPropagationOption.True).Filter("Name1 eq 'n1'").ToArray();

            Assert.Single(result);
        }

        [Fact]
        public void FilterNullPropagationFalse()
        {
            Assert.Throws<NullReferenceException>(() => OpenType.CreateQuery().OData(c => c.QuerySettings.HandleNullPropagation = HandleNullPropagationOption.False).Filter("Name1 eq 'n1'").ToArray());
        }

        // Dynamic properties dictionary is not null, however properties with different names

        [Fact]
        public void FilterDefaultWithNotNullDictionary()
        {
            var result = OpenType.CreateQueryWithNotNullDictionary().OData().Filter("Name1 eq 'n1'").ToArray();

            Assert.Single(result);
        }

        [Fact]
        public void FilterNullPropagationTrueWithNotNullDictionary()
        {
            var result = OpenType.CreateQueryWithNotNullDictionary().OData(c => c.QuerySettings.HandleNullPropagation = HandleNullPropagationOption.True).Filter("Name1 eq 'n1'").ToArray();

            Assert.Single(result);
        }

        [Fact]
        public void FilterNullPropagationFalseWithNotNullDictionary()
        {
            var result = OpenType.CreateQueryWithNotNullDictionary().OData(c => c.QuerySettings.HandleNullPropagation = HandleNullPropagationOption.False).Filter("Name1 eq 'n1'").ToArray();

            Assert.Single(result);
        }

        [Fact]
        public void FilterNullPropagationFalseWithNotNullDictionaryPropertyNotExists()
        {
            var result = OpenType.CreateQueryWithNotNullDictionary().OData(c => c.QuerySettings.HandleNullPropagation = HandleNullPropagationOption.False).Filter("Name3 eq 'n1'").ToArray();

            Assert.Empty(result);
        }

        // Dynamic properties dictionary is not null, all properties with same name

        [Fact]
        public void FilterDefaultAllProperties()
        {
            var result = OpenType.CreateQueryWithAllProperties().OData().Filter("Name1 eq 'n1'").ToArray();

            Assert.Single(result);
        }

        [Fact]
        public void FilterNullPropagationTrueAllProperties()
        {
            var result = OpenType.CreateQueryWithAllProperties().OData(c => c.QuerySettings.HandleNullPropagation = HandleNullPropagationOption.True).Filter("Name1 eq 'n1'").ToArray();

            Assert.Single(result);
        }

        [Fact]
        public void FilterNullPropagationFalseAllProperties()
        {
            var result = OpenType.CreateQueryWithAllProperties().OData(c => c.QuerySettings.HandleNullPropagation = HandleNullPropagationOption.False).Filter("Name1 eq 'n1'").ToArray();

            Assert.Single(result);
        }

        [Fact]
        public void FilterNullPropagationFalseAllPropertiesNotExistingProperty()
        {
            var result = OpenType.CreateQueryWithAllProperties().OData(c => c.QuerySettings.HandleNullPropagation = HandleNullPropagationOption.False).Filter("Name3 eq 'n3'").ToArray();

            Assert.Empty(result);
        }

        //[Fact]
        //public void SelectDefaultWithNotNullDictionary()
        //{
        //    var result = OpenType.CreateQueryWithNotNullDictionary().OData().Filter("Name1 eq 'n1'").SelectExpand("Name1").Single();

        //    var metadata = result.ToDictionary();

        //    Assert.Single(metadata);

        //    Assert.Equal("DynamicProperties", metadata.Single().Key);
        //    Assert.Equal("Name1", (metadata.Single().Value as IDictionary<string, Object>).Single().Key);
        //    Assert.Equal("n1", (metadata.Single().Value as IDictionary<string,Object>).Single().Value);
        //}

        //[Fact]
        //public void SelectJsonDefaultWithNotNullDictionary()
        //{
        //    var result = OpenType.CreateQueryWithNotNullDictionary().OData().SelectExpandJsonToken("Name1");

        //    Assert.Equal("{Nmae1:n1}", result.ToString());
        //}
    }
}
