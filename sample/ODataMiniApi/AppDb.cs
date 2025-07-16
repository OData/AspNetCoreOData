//-----------------------------------------------------------------------------
// <copyright file="AppDb.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;

namespace ODataMiniApi;

public class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> options) : base(options) { }

    public DbSet<School> Schools => Set<School>();

    public DbSet<Student> Students => Set<Student>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<School>().HasKey(x => x.SchoolId);
        modelBuilder.Entity<Student>().HasKey(x => x.StudentId);
        modelBuilder.Entity<School>().OwnsOne(x => x.MailAddress);

        modelBuilder.Entity<Customer>().HasKey(x => x.Id);
        modelBuilder.Entity<Order>().HasKey(x => x.Id);
        modelBuilder.Entity<Customer>().OwnsOne(x => x.Info);
    }
}

static class AppDbExtension
{
    public static void MakeSureDbCreated(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<AppDb>();

            if (context.Schools.Count() == 0)
            {
                #region Students
                var students = new List<Student>
                {
                    // Mercury school
                    new Student { SchoolId = 1, StudentId = 10, FirstName = "Spens", LastName = "Alex", FavoriteSport = "Soccer", Grade = 87, BirthDay = new DateOnly(2009, 11, 15) },
                    new Student { SchoolId = 1, StudentId = 11, FirstName = "Jasial", LastName = "Eaine", FavoriteSport = "Basketball", Grade = 45, BirthDay = new DateOnly(1989, 8, 3) },
                    new Student { SchoolId = 1, StudentId = 12, FirstName = "Niko", LastName = "Rorigo", FavoriteSport = "Soccer", Grade = 78, BirthDay = new DateOnly(2019, 5, 5) },
                    new Student { SchoolId = 1, StudentId = 13, FirstName = "Roy", LastName = "Rorigo", FavoriteSport = "Tennis", Grade = 67, BirthDay = new DateOnly(1975, 11, 4) },
                    new Student { SchoolId = 1, StudentId = 14, FirstName = "Zaral", LastName = "Clak", FavoriteSport = "Basketball", Grade = 54, BirthDay = new DateOnly(2008, 1, 4) },

                    // Venus school
                    new Student { SchoolId = 2, StudentId = 20, FirstName = "Hugh", LastName = "Briana", FavoriteSport = "Basketball", Grade = 78, BirthDay = new DateOnly(1959, 5, 6) },
                    new Student { SchoolId = 2, StudentId = 21, FirstName = "Reece", LastName = "Len", FavoriteSport = "Basketball", Grade = 45, BirthDay = new DateOnly(2004, 2, 5) },
                    new Student { SchoolId = 2, StudentId = 22, FirstName = "Javanny", LastName = "Jay", FavoriteSport = "Soccer", Grade = 87, BirthDay = new DateOnly(2003, 6, 5) },
                    new Student { SchoolId = 2, StudentId = 23, FirstName = "Ketty", LastName = "Oak", FavoriteSport = "Tennis", Grade = 99, BirthDay = new DateOnly(1998, 7, 25) },
                    
                    // Earth School
                    new Student { SchoolId = 3, StudentId = 30, FirstName = "Mike", LastName = "Wat", FavoriteSport = "Tennis", Grade = 93, BirthDay = new DateOnly(1999, 5, 15) },
                    new Student { SchoolId = 3, StudentId = 31, FirstName = "Sam", LastName = "Joshi", FavoriteSport = "Soccer", Grade = 78, BirthDay = new DateOnly(2000, 6, 23) },
                    new Student { SchoolId = 3, StudentId = 32, FirstName = "Kerry", LastName = "Travade", FavoriteSport = "Basketball", Grade = 89, BirthDay = new DateOnly(2001, 2, 6) },
                    new Student { SchoolId = 3, StudentId = 33, FirstName = "Pett", LastName = "Jay", FavoriteSport = "Tennis", Grade = 63, BirthDay = new DateOnly(1998, 11, 7) },

                    // Mars School
                    new Student { SchoolId = 4, StudentId = 40, FirstName = "Mike", LastName = "Wat", FavoriteSport = "Soccer", Grade = 64, BirthDay = new DateOnly(2011, 11, 15) },
                    new Student { SchoolId = 4, StudentId = 41, FirstName = "Sam", LastName = "Joshi", FavoriteSport = "Basketball", Grade = 98, BirthDay = new DateOnly(2005, 6, 6) },
                    new Student { SchoolId = 4, StudentId = 42, FirstName = "Kerry", LastName = "Travade", FavoriteSport = "Soccer", Grade = 88, BirthDay = new DateOnly(2011, 5, 13) },

                    // Jupiter School
                    new Student { SchoolId = 5, StudentId = 50, FirstName = "David", LastName = "Padron", FavoriteSport = "Tennis", Grade = 77, BirthDay = new DateOnly(2015, 12, 3) },
                    new Student { SchoolId = 5, StudentId = 53, FirstName = "Jeh", LastName = "Brook", FavoriteSport = "Basketball", Grade = 69, BirthDay = new DateOnly(2014, 10, 15) },
                    new Student { SchoolId = 5, StudentId = 54, FirstName = "Steve", LastName = "Johnson", FavoriteSport = "Soccer", Grade = 100, BirthDay = new DateOnly(1995, 3, 2) },

                    // Saturn School
                    new Student { SchoolId = 6, StudentId = 60, FirstName = "John", LastName = "Haney", FavoriteSport = "Soccer", Grade = 99, BirthDay = new DateOnly(2008, 12, 1) },
                    new Student { SchoolId = 6, StudentId = 61, FirstName = "Morgan", LastName = "Frost", FavoriteSport = "Tennis", Grade = 17, BirthDay = new DateOnly(2009, 11, 4) },
                    new Student { SchoolId = 6, StudentId = 62, FirstName = "Jennifer", LastName = "Viles", FavoriteSport = "Basketball", Grade = 54, BirthDay = new DateOnly(1989, 3, 15) },

                    // Uranus School
                    new Student { SchoolId = 7, StudentId = 72, FirstName = "Matt", LastName = "Dally", FavoriteSport = "Basketball", Grade = 77, BirthDay = new DateOnly(2011, 11, 4) },
                    new Student { SchoolId = 7, StudentId = 73, FirstName = "Kevin", LastName = "Vax", FavoriteSport = "Basketball", Grade = 93, BirthDay = new DateOnly(2012, 5, 12) },
                    new Student { SchoolId = 7, StudentId = 76, FirstName = "John", LastName = "Clarey", FavoriteSport = "Soccer", Grade = 95, BirthDay = new DateOnly(2008, 8, 8) },

                    // Neptune School
                    new Student { SchoolId = 8, StudentId = 81, FirstName = "Adam", LastName = "Singh", FavoriteSport = "Tennis", Grade = 92, BirthDay = new DateOnly(2006, 6, 23) },
                    new Student { SchoolId = 8, StudentId = 82, FirstName = "Bob", LastName = "Joe", FavoriteSport = "Soccer", Grade = 88, BirthDay = new DateOnly(1978, 11, 15) },
                    new Student { SchoolId = 8, StudentId = 84, FirstName = "Martin", LastName = "Dalton", FavoriteSport = "Tennis", Grade = 77, BirthDay = new DateOnly(2017, 5, 14) },

                    // Pluto School
                    new Student { SchoolId = 9, StudentId = 91, FirstName = "Michael", LastName = "Wu", FavoriteSport = "Soccer", Grade = 97, BirthDay = new DateOnly(2022, 9, 22) },
                    new Student { SchoolId = 9, StudentId = 93, FirstName = "Rachel", LastName = "Wottle", FavoriteSport = "Soccer", Grade = 81, BirthDay = new DateOnly(2022, 10, 5) },
                    new Student { SchoolId = 9, StudentId = 97, FirstName = "Aakash", LastName = "Aarav", FavoriteSport = "Soccer", Grade = 98, BirthDay = new DateOnly(2003, 3, 15) },

                    // Shyline high School
                    new Student { SchoolId = 10, StudentId = 101, FirstName = "Steve", LastName = "Chu", FavoriteSport = "Soccer", Grade = 77, BirthDay = new DateOnly(2002, 11, 12) },
                    new Student { SchoolId = 10, StudentId = 123, FirstName = "Wash", LastName = "Dish", FavoriteSport = "Tennis", Grade = 81, BirthDay = new DateOnly(2002, 12, 5) },
                    new Student { SchoolId = 10, StudentId = 106, FirstName = "Ren", LastName = "Wu", FavoriteSport = "Soccer", Grade = 88, BirthDay = new DateOnly(2003, 3, 15) }
                };

                foreach (var s in students)
                {
                    context.Students.Add(s);
                }
                #endregion

                #region Schools
                var schools = new List<School>
                {
                    new School { SchoolId = 1, SchoolName = "Mercury Middle School", MailAddress = new Address { ApartNum = 241, City = "Kirk", Street = "156TH AVE", ZipCode = "98051" } },
                    new HighSchool { SchoolId = 2, SchoolName = "Venus High School", MailAddress = new Address { ApartNum = 543, City = "AR", Street = "51TH AVE PL", ZipCode = "98043" }, NumberOfStudents = 1187, PrincipalName = "Venus TT" },
                    new School { SchoolId = 3, SchoolName = "Earth University", MailAddress = new Address { ApartNum = 101, City = "Belly", Street = "24TH ST", ZipCode = "98029" } },
                    new School { SchoolId = 4, SchoolName = "Mars Elementary School ", MailAddress = new Address { ApartNum = 123, City = "Issaca", Street = "Mars Rd", ZipCode = "98023" }  },
                    new School { SchoolId = 5, SchoolName = "Jupiter College", MailAddress = new Address { ApartNum = 443, City = "Redmond", Street = "Sky Freeway", ZipCode = "78123" } },
                    new School { SchoolId = 6, SchoolName = "Saturn Middle School", MailAddress = new Address { ApartNum = 11, City = "Moon", Street = "187TH ST", ZipCode = "68133" } },
                    new HighSchool { SchoolId = 7, SchoolName = "Uranus High School", MailAddress = new Address { ApartNum = 123, City = "Greenland", Street = "Sun Street", ZipCode = "88155" }, NumberOfStudents = 886, PrincipalName = "Uranus Sun" },
                    new School { SchoolId = 8, SchoolName = "Neptune Elementary School", MailAddress = new Address  { ApartNum = 77, City = "BadCity", Street = "Moon way", ZipCode = "89155" } },
                    new School { SchoolId = 9, SchoolName = "Pluto University", MailAddress = new Address { ApartNum = 12004, City = "Sahamish", Street = "Universals ST", ZipCode = "10293" } },
                    new HighSchool { SchoolId =10, SchoolName = "Shyline High School", MailAddress = new Address { ApartNum = 4004, City = "Sammamish", Street = "8TH ST", ZipCode = "98029"}, NumberOfStudents = 976, PrincipalName = "Laly Fort" }
                };

                foreach (var s in schools)
                {
                    s.Students = students.Where(std => std.SchoolId == s.SchoolId).ToList();

                    context.Schools.Add(s);
                }
                #endregion

                context.SaveChanges();
            }

            if (context.Customers.Count() == 0)
            {
                #region Customers and Orders

                var customers = new List<Customer>
                {
                    new Customer { Id = 1, Name = "Alice", Info = new Info { Email = "alice@example.com", Phone = "123-456-7819" },
                        Orders = [
                            new Order { Id = 11, Amount = 9},
                            new Order { Id = 12, Amount = 19},
                        ] },
                    new Customer { Id = 2, Name = "Johnson", Info = new Info { Email = "johnson@abc.com", Phone = "233-468-7289" },
                        Orders = [
                            new Order { Id = 21, Amount =8},
                            new Order { Id = 22, Amount = 76},
                        ] },
                    new Customer { Id = 3, Name = "Peter", Info = new Info { Email = "peter@earth.org", Phone = "223-656-7889" },
                        Orders = [
                            new Order { Id = 32, Amount = 7 }
                        ] },

                    new Customer { Id = 4, Name = "Sam", Info = new Info { Email = "sam@ms.edu", Phone = "245-876-0989" },
                        Orders = [
                            new Order { Id = 41, Amount = 5 },
                            new Order { Id = 42, Amount = 32}
                        ] }
                };

                foreach (var s in customers)
                {
                    context.Customers.Add(s);
                    foreach (var o in s.Orders)
                    {
                        context.Orders.Add(o);
                    }
                }
                #endregion

                context.SaveChanges();
            }
        }
    }
}
