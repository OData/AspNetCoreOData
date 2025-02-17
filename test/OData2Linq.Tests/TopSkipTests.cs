namespace OData2Linq.Tests
{
    using Microsoft.OData;
    using OData2Linq.Tests.SampleData;
    using System.Linq;
    using Xunit;

    public class TopSkipTests
    {
        [Theory]
        [InlineData("1", null, 1, "n1")]
        [InlineData("1", "0", 1, "n1")]
        [InlineData(null, "1", 1, "n2")]
        [InlineData(null, null, 2, "n1")]
        [InlineData("0", "1", 0, null)]
        [InlineData("0", "0", 0, null)]
        public void TopSkip(string top, string skip, int count, string expectedName)
        {
            var result = SimpleClass.CreateQuery().OData().TopSkip(top, skip).ToArray();

            Assert.Equal(count, result.Length);
            if (count > 0)
            {
                Assert.Equal(expectedName, result[0].Name);
            }
        }

        [Theory]
        [InlineData(null, 20)]
        [InlineData(10, 10)]
        [InlineData(30, 30)]
        public void TopSkipDefaultPageSize(int? pageSize, int count)
        {
            var result = Enumerable.Repeat(new SimpleClass(), 1000).AsQueryable().OData(s => s.QuerySettings.PageSize = pageSize ?? s.QuerySettings.PageSize)
                .TopSkip().ToArray();

            Assert.Equal(count, result.Length);
        }

        [Theory]
        [InlineData("-1", "1", "Invalid value '-1' for $top query option found.")]
        [InlineData("1", "-1", "Invalid value '-1' for $skip query option found.")]
        [InlineData("1000000000000000", "1", "The limit of '2147483647' for Top query has been exceeded.")]
        [InlineData("1", "1000000000000000", "The limit of '2147483647' for Skip query has been exceeded.")]
        public void TopSkipValidation(string top, string skip, string expectedMessage)
        {
            var message = Assert.Throws<ODataException>(() => SimpleClass.CreateQuery().OData().TopSkip(top, skip))
                .Message;
            Assert.Contains(expectedMessage, message);
        }

        [Theory]
        [InlineData("11", "1", "The limit of '10' for Top query has been exceeded.")]
        [InlineData("1", "11", "The limit of '10' for Skip query has been exceeded.")]
        public void TopSkipMax(string top, string skip, string expectedMessage)
        {
            var message = Assert.Throws<ODataException>(() => SimpleClass.CreateQuery().OData(s =>
                    {
                        s.ValidationSettings.MaxTop = 10;
                        s.ValidationSettings.MaxSkip = 10;
                    }).TopSkip(top, skip))
                .Message;
            Assert.Contains(expectedMessage, message);
        }
    }
}