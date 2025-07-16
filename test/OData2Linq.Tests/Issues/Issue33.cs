// Issue 33 : Recursive loop of complex types

namespace OData2Linq.Tests.Issues33
{
    using System.Linq;
    using Xunit;

    public class RecursiveComplexType
    {
        public RecursiveComplexType SelfReference { get; set; }
    }

    public class ListItem
    {
        public int Id { get; set; }

        public RecursiveComplexType RecursiveComplexType { get; set; }
    }

    public class Issue33
    {
        [Fact]
        public void Recursive_Loops_Must_Be_Allowed_By_Default()
        {
            // arrange

            var queryable = new ListItem[]
            {
                new ListItem { Id = 1, RecursiveComplexType = new RecursiveComplexType() },
                new ListItem { Id = 2, RecursiveComplexType = new RecursiveComplexType() }
            }.AsQueryable();

            // act

            var result = queryable.OData().Filter("Id eq 1").ToArray();

            // assert

            Assert.Single(result);
        }
    }
}
