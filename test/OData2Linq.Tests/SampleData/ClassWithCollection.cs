namespace OData2Linq.Tests.SampleData
{
    using System.Collections.Generic;
    using System.Linq;

    public class ClassWithCollection
    {
        private static readonly ClassWithCollection[] items = new[]
        {
            new ClassWithCollection {Id = 31, Name = "n31", Link2 = new[] { new SimpleClass {Id = 311, Name = "n311"},new SimpleClass {Id = 312, Name = "n312"} }},
            new ClassWithCollection {Id = 32, Name = "n32", Link2 = new[] { new SimpleClass {Id = 321, Name = "n321"} }}
        };

        public static IQueryable<ClassWithCollection> CreateQuery()
        {
            return items.AsQueryable();
        }

        public long Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<SimpleClass> Link2 { get; set; }
    }
}