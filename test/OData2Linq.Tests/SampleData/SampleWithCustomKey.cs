namespace OData2Linq.Tests.SampleData
{
    using Microsoft.OData.ModelBuilder;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    public class SampleWithCustomKey
    {
        private static readonly SampleWithCustomKey[] items =
            {
                new SampleWithCustomKey {
                    Name = "n1", DateTime = new DateTime(2018, 1, 26),
                    ExpandableLink = new SimpleClass() { Id=1 },
                    SelectableLink = new SimpleClass() { Id=2 },
                    AutoExpandLink = new SimpleClass() { Id=3 },
                    AutoExpandAndSelectLink = new SimpleClass() { Id=4 },
                    ExpandAndSelectLink = new SimpleClass() { Id=5 },
                    RecursiveLink = new SampleWithCustomKey(){RecursiveLink=new SampleWithCustomKey{RecursiveLink=new SampleWithCustomKey{Name="qwe"}} }
                },
                new SampleWithCustomKey {
                    Name = "n2", DateTime = new DateTime(2001, 1, 26),
                    ExpandableLink = new SimpleClass() { Id=2 },
                    SelectableLink = new SimpleClass() { Id=2 },
                    AutoExpandLink = new SimpleClass() { Id=3 },
                    AutoExpandAndSelectLink = new SimpleClass() { Id=4 },
                    ExpandAndSelectLink = new SimpleClass() { Id=5 },
                    RecursiveLink = new SampleWithCustomKey(){RecursiveLink=new SampleWithCustomKey{RecursiveLink=new SampleWithCustomKey{Name="qwe"}} }
                },
            };

        public static IQueryable<SampleWithCustomKey> CreateQuery()
        {
            return items.AsQueryable();
        }

        [Key]
        public string Name { get; set; }

        public DateTime DateTime { get; set; }

        [NotExpandable]
        public ICollection<SimpleClass> NotExpandableLink { get; set; }

        [Expand(ExpandType = SelectExpandType.Automatic, MaxDepth = 2)]
        public SimpleClass ExpandableLink { get; set; }

        [Select(SelectType = SelectExpandType.Automatic)]
        public SimpleClass SelectableLink { get; set; }

        [AutoExpand(DisableWhenSelectPresent = true)]
        public SimpleClass AutoExpandLink { get; set; }

        [AutoExpand(DisableWhenSelectPresent = true)]
        [Select(SelectType = SelectExpandType.Automatic)]
        public SimpleClass AutoExpandAndSelectLink { get; set; }

        [Expand(ExpandType = SelectExpandType.Automatic, MaxDepth = 2)]
        [Select(SelectType = SelectExpandType.Automatic)]
        public SimpleClass ExpandAndSelectLink { get; set; }

        [Expand(ExpandType = SelectExpandType.Automatic, MaxDepth = 2)]
        public SampleWithCustomKey RecursiveLink { get; set; }
    }
}