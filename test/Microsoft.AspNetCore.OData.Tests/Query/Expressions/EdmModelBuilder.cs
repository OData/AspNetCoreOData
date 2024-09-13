using Microsoft.FullyQualified.NS;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Tests.Query.Expressions
{
    internal static class EdmModelBuilder
    {
        private static readonly IEdmModel Model = BuildAndGetEdmModel();

        public static IEdmModel TestModel
        {
            get { return Model; }
        }

        public static IEdmEntityType GetEntityType(string entityQualifiedName)
        {
            return TestModel.FindDeclaredType(entityQualifiedName) as IEdmEntityType;
        }

        public static IEdmComplexType GetEdmComplexType(string complexTypeQualifiedName)
        {
            return TestModel.FindDeclaredType(complexTypeQualifiedName) as IEdmComplexType;
        }

        public static IEdmEntityTypeReference GetEntityTypeReference(IEdmEntityType entityType)
        {
            return new EdmEntityTypeReference(entityType, false);
        }

        public static IEdmComplexTypeReference GetComplexTypeReference(IEdmComplexType complexType)
        {
            // Create a complex type reference using the EdmCoreModel
            return new EdmComplexTypeReference(complexType, isNullable: false);
        }

        public static IEdmProperty GetPersonLocationProperty()
        {
            return GetEntityType("Microsoft.FullyQualified.NS.Person").FindProperty("Location");
        }

        /// <summary>
        /// Get the entity set from the model
        /// </summary>
        /// <returns>People Set</returns>
        public static IEdmEntitySet GetPeopleSet()
        {
            return TestModel.EntityContainer.FindEntitySet("People");
        }

        public static IEdmModel BuildAndGetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.Namespace = "Microsoft.FullyQualified.NS";
            builder.EntitySet<Person>("People");
            builder.ComplexType<MyAddress>();
            builder.ComplexType<WorkAddress>();
            builder.EntitySet<Employee>("Employees");

            return builder.GetEdmModel();
        }
    }
}

namespace Microsoft.FullyQualified.NS
{
    public class Person
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public MyAddress Location { get; set; }
    }

    public class Employee : Person
    {
        public string EmployeeNumber { get; set; }
    }

    public class MyAddress
    {
        public string Street { get; set; }
        public AddressType AddressType { get; set; }
    }

    public class WorkAddress : MyAddress
    {
        public string OfficeNumber { get; set; }
    }

    public enum AddressType
    {
        Home,
        Work,
        Other
    }
}
