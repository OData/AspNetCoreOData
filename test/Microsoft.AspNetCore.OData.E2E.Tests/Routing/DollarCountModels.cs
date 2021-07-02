using System;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Routing
{

    [Flags]
    public enum DollarColor
    {
        Red = 1,
        Green = 2,
        Blue = 4
    }

    public class DollarCountEntity
    {
        public int Id { get; set; }
        public string[] StringCollectionProp { get; set; }
        public DollarColor[] EnumCollectionProp { get; set; }
        public TimeSpan[] TimeSpanCollectionProp { get; set; }
        public DollarCountComplex[] ComplexCollectionProp { get; set; }
        public DollarCountEntity[] EntityCollectionProp { get; set; }
        public int[] DollarCountNotAllowedCollectionProp { get; set; }
    }

    public class DerivedDollarCountEntity : DollarCountEntity
    {
        public string DerivedProp { get; set; }
    }

    public class DollarCountComplex
    {
        public string StringProp { get; set; }
    }

}
