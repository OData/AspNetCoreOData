using System.Runtime.Serialization;

namespace OData2Linq.Tests.SampleData
{
    using Microsoft.OData.ModelBuilder;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public class SimpleClass
    {
        public const int NumberOfProperties = 9;

        private static readonly SimpleClass[] items =
        {
            new SimpleClass {
                Id = 1,
                Name = "n1",
                DateTimeOffset = new DateTimeOffset(new DateTime(2018, 1, 26), TimeZoneInfo.Local.BaseUtcOffset),
                DateTime = new DateTime(2018, 1, 26),
                DateOnly = new DateOnly(2018, 1, 26),
                TimeOnly = new TimeOnly(12, 34, 56),
                TestEnum = TestEnum.Item1,
                NameToIgnore = "ni1",
                NameNotFilter="nf1"},
            new SimpleClass {
                Id = 2,
                Name = "n2",
                DateTimeOffset = new DateTimeOffset(new DateTime(2001, 1, 26), TimeZoneInfo.Local.BaseUtcOffset),
                DateTime = new DateTime(2001, 1, 26),
                DateOnly = new DateOnly(2001, 1, 26),
                TimeOnly = new TimeOnly(1, 2, 3),
                TestEnum = TestEnum.Item2,
                NameToIgnore = "ni1",
                NameNotFilter="nf2"}
        };

        public static IQueryable<SimpleClass> CreateQuery()
        {
            return items.AsQueryable();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime DateTime { get; set; }

        public DateTimeOffset DateTimeOffset { get; set; }

        public DateOnly DateOnly { get; set; }

        public TimeOnly TimeOnly { get; set; }

        public TestEnum TestEnum { get; set; }

        [IgnoreDataMember]
        public string NameToIgnore { get; set; }

        [NonFilterable]
        public string NameNotFilter { get; set; }

        [NotSortable]
        public int NotOrderable { get; set; }

        [NotMapped]
        public int NotMapped { get; set; }
    }
}
