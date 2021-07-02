namespace Microsoft.AspNetCore.OData.Tests.Query
{
    public class LevelsBaseEntity
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class LevelsEntity : LevelsBaseEntity
    {
        public LevelsEntity Parent { get; set; }
        public LevelsBaseEntity[] BaseEntities { get; set; }
        public LevelsDerivedEntity[] DerivedAncestors { get; set; }
    }

    public class LevelsDerivedEntity : LevelsEntity
    {
        public string DerivedName { get; set; }
        public LevelsEntity[] AncestorsInDerivedEntity { get; set; }
    }

}
