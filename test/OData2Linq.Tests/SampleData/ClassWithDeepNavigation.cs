using Microsoft.OData.ModelBuilder;

namespace OData2Linq.Tests.SampleData
{
    public class ClassWithDeepNavigation
    {
        private static readonly ClassWithDeepNavigation[] items = new[]
        {
            new ClassWithDeepNavigation { D1 = new ClassWithDeepNavigation {D2 = new ClassWithDeepNavigation{D3 = new ClassWithDeepNavigation{Id = 11, Name = "n1123",}} } },
            new ClassWithDeepNavigation { D1 = new ClassWithDeepNavigation {D2 = new ClassWithDeepNavigation{D3 = new ClassWithDeepNavigation{Id = 22, Name = "n2123"} } } }
        };

        public static IQueryable<ClassWithDeepNavigation> CreateQuery()
        {
            return items.AsQueryable();
        }

        public long Id { get; set; }

        public string Name { get; set; }

        [Expand(MaxDepth = 3)]
        public virtual ClassWithDeepNavigation D1 { get; set; }
        public virtual ClassWithDeepNavigation D2 { get; set; }
        public virtual ClassWithDeepNavigation D3 { get; set; }
    }
}
