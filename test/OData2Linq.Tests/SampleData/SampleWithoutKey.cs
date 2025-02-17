namespace OData2Linq.Tests.SampleData
{
    using System;
    using System.Linq;

    public class SampleWithoutKey
    {
        private static readonly SampleWithoutKey[] items =
            {
                new SampleWithoutKey { Name = "n1", DateTime = new DateTime(2018, 1, 26)},
                new SampleWithoutKey { Name = "n2", DateTime = new DateTime(2001, 1, 26)}
            };

        public static IQueryable<SampleWithoutKey> CreateQuery()
        {
            return items.AsQueryable();
        }

        public string Name { get; set; }

        public DateTime DateTime { get; set; }
    }
}