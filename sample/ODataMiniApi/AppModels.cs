//-----------------------------------------------------------------------------
// <copyright file="AppModels.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.ComponentModel.DataAnnotations.Schema;

namespace ODataMiniApi;

public class EdmModelBuilder
{
    public static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<School>("Schools");
        builder.ComplexType<Address>();
        builder.ComplexType<Student>();

        builder.EntitySet<Customer>("Customers");
        builder.EntitySet<Order>("Orders");
        builder.ComplexType<Info>();
        return builder.GetEdmModel();
    }
}

public class School
{
    public int SchoolId { get; set; }

    public string SchoolName { get; set; }

    public Address MailAddress { get; set; }

    public virtual IList<Student> Students { get; set; }
}

public class HighSchool : School
{
    // Additional properties specific to HighSchool
    public int NumberOfStudents { get; set; }
    public string PrincipalName { get; set; }
}

public class Student
{
    public int StudentId { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string FavoriteSport { get; set; }

    public int Grade { get; set; }

    public int SchoolId { get; set; }

    public DateOnly BirthDay { get; set; }
}

[ComplexType]
public class Address
{
    public int ApartNum { get; set; }

    public string City { get; set; }

    public string Street { get; set; }

    public string ZipCode { get; set; }
}


public class Customer
{
    public int Id { get; set; }

    public string Name { get; set; }

    public Info Info { get; set; }

    public List<Order> Orders { get; set; }
}

public class Order
{
    public int Id { get; set; }

    public int Amount { get; set; }
}

public class Info
{
    public string Email { get; set; }
    public string Phone { get; set; }
}