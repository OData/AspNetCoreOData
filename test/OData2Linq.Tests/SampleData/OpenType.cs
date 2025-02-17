namespace OData2Linq.Tests.SampleData
{
    public class OpenType
    {
        public int Id { get; set; }

        public IDictionary<string, object> DynamicProperties { get; set; }

        private static readonly OpenType[] items = new[]
        {
            new OpenType {Id = 1, DynamicProperties = new Dictionary<string,object>(){ {"Name1","n1" } } },
            new OpenType {Id = 2, DynamicProperties = new Dictionary<string,object>(){ {"Name2","n2" } } },
            new OpenType {Id = 3},
        };

        private static readonly OpenType[] itemsWithNotNullDictionary = new[]
        {
            new OpenType {Id = 1, DynamicProperties = new Dictionary<string,object>(){ {"Name1","n1" } } },
            new OpenType {Id = 2, DynamicProperties = new Dictionary<string,object>(){ {"Name2","n2" } } },
        };

        private static readonly OpenType[] itemsWithAllProperties = new[]
        {
            new OpenType {Id = 1, DynamicProperties = new Dictionary<string,object>(){ {"Name1","n1" } } },
            new OpenType {Id = 2, DynamicProperties = new Dictionary<string,object>(){ {"Name1","n2" } } },
        };

        public static IQueryable<OpenType> CreateQuery()
        {
            return items.AsQueryable();
        }

        public static IQueryable<OpenType> CreateQueryWithNotNullDictionary()
        {
            return itemsWithNotNullDictionary.AsQueryable();
        }

        public static IQueryable<OpenType> CreateQueryWithAllProperties()
        {
            return itemsWithAllProperties.AsQueryable();
        }
    }
}
