using Microsoft.OData.ModelBuilder;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

namespace Issue701_Repro.Models
{
    public class Sample
    {
        [Contained]
        [ForeignKey("Uid")]
        [AutoExpand]
        public IEnumerable<SampleItems> SItems { get; set; } = Enumerable.Empty<SampleItems>();
    }

    public class SampleItems
    {
        [Key]
        public string Uid { get; set; }

        [AutoExpand]
        [Contained]
        public IEnumerable<SampleItemGuide> SampleItem_guide { get; set; } = System.Array.Empty<SampleItemGuide>();
    }

    public class SampleItemGuide
    {
        [Key]
        public string Uid { get; set; }
        public string Type { get; set; }
    }
}
