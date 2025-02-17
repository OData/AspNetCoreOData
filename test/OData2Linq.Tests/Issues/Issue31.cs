
namespace OData2Linq.Tests.Issues
{
    using OData2Linq.Tests.SampleData;
    using System.Linq;
    using Xunit;

    public class Issue31
    {
        [Fact]
        public void WhereWithInThrowException()
        {
            var result = SimpleClass.CreateQuery().OData().Filter($"{nameof(SimpleClass.Id)} in (1,100)").ToArray();

            Assert.Collection(result,
                e => Assert.InRange(e.Id, 1, 100));
        }
    }
}
