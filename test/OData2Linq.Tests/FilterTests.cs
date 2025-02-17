namespace OData2Linq.Tests
{
    using Microsoft.OData;
    using OData2Linq.Tests.SampleData;
    using System.Linq;
    using Xunit;

    public class FilterTests
    {
        [Fact]
        public void WhereById()
        {
            var result = SimpleClass.CreateQuery().OData().Filter("Id eq 1").ToArray();

            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }

        [Fact]
        public void WhereByName()
        {
            var result = SimpleClass.CreateQuery().OData().Filter("Name eq 'n1'").ToArray();

            Assert.Single(result);
            Assert.Equal("n1", result[0].Name);
        }

        [Fact]
        public void WhereByNameCaseInsensitiveKeyByDefault()
        {
            var result = SimpleClass.CreateQuery().OData().Filter("name eq 'n1'").ToArray();

            Assert.Single(result);
            Assert.Equal("n1", result[0].Name);
        }

        [Fact]
        public void WhereByNameCaseSensitiveKeyByConfigThowException()
        {
            Assert.Throws<ODataException>(
                () => SimpleClass.CreateQuery().OData(s => s.EnableCaseInsensitive = false).Filter("name eq 'n1'"));
        }

        [Fact]
        public void WhereByRandomStringThrowException()
        {
            Assert.Throws<ODataException>(
                () => SimpleClass.CreateQuery().OData().Filter("qwe"));
        }

        [Fact]
        public void WhereByEnumString()
        {
            var result = SimpleClass.CreateQuery().OData().Filter("TestEnum eq 'Item2'").ToArray();

            Assert.Single(result);
            Assert.Equal("n2", result[0].Name);
        }

        [Fact]
        public void WhereByEnumNumber()
        {
            var result = SimpleClass.CreateQuery()
                .OData()
                .Filter("TestEnum eq '2'").ToArray();

            Assert.Single(result);
            Assert.Equal("n2", result[0].Name);
        }

        [Fact]
        public void WhereByEnumStringCaseInsensitiveValueByDefault()
        {
            var result = SimpleClass.CreateQuery().OData().Filter("TestEnum eq 'item2'").ToArray();

            Assert.Single(result);
            Assert.Equal("n2", result[0].Name);
        }

        [Fact]
        public void WhereByEnumStringCaseInsensitiveKeyByDefault()
        {
            var result = SimpleClass.CreateQuery().OData().Filter("testEnum eq 'item2'").ToArray();

            Assert.Single(result);
            Assert.Equal("n2", result[0].Name);
        }

        [Fact]
        public void WhereByNameNotNull()
        {
            var result = SimpleClass.CreateQuery().OData().Filter("Name ne null").ToArray();

            Assert.Equal("n1", result[0].Name);
        }

        [Fact]
        public void WhereByEnumStringWithInKeyword()
        {
            var result = SimpleClass.CreateQuery().OData().Filter($"{nameof(SimpleClass.TestEnum)} in ('{nameof(TestEnum.Item2)}')").ToArray();

            Assert.Single(result);
            Assert.Equal("n2", result[0].Name);
        }

        [Fact]
        public void WhereByNonFilterableThrowException()
        {
            Assert.Throws<ODataException>(
                () => SimpleClass.CreateQuery().OData(s => s.EnableCaseInsensitive = false).Filter($"{nameof(SimpleClass.NameNotFilter)} eq 'nf1'"));
        }

        [Fact]
        public void WhereByNotMappedThrow()
        {
            Assert.Throws<ODataException>(
                () => SimpleClass.CreateQuery().OData(s => s.EnableCaseInsensitive = false).Filter($"{nameof(SimpleClass.NotMapped)} eq 'nf1'"));
        }

        [Fact]
        public void WhereByIgnoreDataMemberThrowException()
        {
            Assert.Throws<ODataException>(
                () => SimpleClass.CreateQuery().OData(s => s.EnableCaseInsensitive = false).Filter($"{nameof(SimpleClass.NameToIgnore)} eq 'ni1'"));
        }
    }
}