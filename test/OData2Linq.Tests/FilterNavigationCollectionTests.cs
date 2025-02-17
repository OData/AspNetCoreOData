namespace OData2Linq.Tests
{
    using OData2Linq.Tests.SampleData;
    using System.Collections;
    using System.Linq;
    using Xunit;

    public class FilterNavigationCollectionTests
    {
        [Fact]
        public void WhereCol1()
        {
            var result = ClassWithCollection.CreateQuery().OData().Filter("Link2/any(s: s/Id eq 311)").ToArray();

            Assert.Single((IEnumerable)result);
            Assert.Equal(31, result[0].Id);
        }
    }
}