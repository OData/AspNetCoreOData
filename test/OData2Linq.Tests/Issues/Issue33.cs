// Issue 33 : Recursive loop of complex types

namespace OData2Linq.Tests.Issues33
{
    using OData2Linq.Settings;
    using System;
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
        [Fact(Skip = "Not supported anymore")]
        public void Recursive_Loops_Must_Not_Be_Allowed_By_Default()
        {
            // arrange

            var queryable = new ListItem[]
            {
                new ListItem { Id = 1, RecursiveComplexType = new RecursiveComplexType() },
                new ListItem { Id = 2, RecursiveComplexType = new RecursiveComplexType() }
            }.AsQueryable();

            // act

            var exception = Assert.Throws<ArgumentException>(() => queryable.OData().Filter("Id eq 1").ToArray());
            
            // assert

            Assert.Contains("recursive loop of complex types is not allowed", exception.Message);
        }

        [Fact]
        public void Recursive_Loops_Must_Be_Allowed_If_Opted_Into_By_Inline_Configuration()
        {
            // arrange

            var queryable = new ListItem[]
            {
                new ListItem { Id = 1, RecursiveComplexType = new RecursiveComplexType() },
                new ListItem { Id = 2, RecursiveComplexType = new RecursiveComplexType() }
            }.AsQueryable();

            // act

            var result = queryable.OData(x =>
            {
                x.AllowRecursiveLoopOfComplexTypes = true;

            }).Filter("Id eq 1").ToArray();

            // assert

            Assert.Single(result);
        }

        [Fact]
        public void Recursive_Loops_Must_Be_Allowed_If_Opted_Into_By_Global_Configuration()
        {
            // arrange

            var queryable = new ListItem[]
            {
                new ListItem { Id = 1, RecursiveComplexType = new RecursiveComplexType() },
                new ListItem { Id = 2, RecursiveComplexType = new RecursiveComplexType() }
            }.AsQueryable();

            ODataSettings.SetInitializer(x =>
            {
                x.AllowRecursiveLoopOfComplexTypes = true;

            });

            // act

            var result = queryable.OData().Filter("Id eq 1").ToArray();

            // assert

            Assert.Single(result);
        }
    }
}
