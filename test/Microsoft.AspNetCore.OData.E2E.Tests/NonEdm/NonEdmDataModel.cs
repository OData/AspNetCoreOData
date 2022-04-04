namespace Microsoft.AspNetCore.OData.E2E.Tests.NonEdm
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Gender Gender { get; set; }
    }

    public enum Gender
    {
        Male = 1,
        Female = 2
    }
}
