//-----------------------------------------------------------------------------
// <copyright file="StudentEndpoints.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace ODataMiniApi.Students;

/// <summary>
/// Add student endpoint to support CRUD operations.
/// The codes are implemented as simple as possible. Please file issues to us for any issues.
/// </summary>
public static class StudentEndpoints
{
    public static async Task<IResult> GetStudentByIdAsync(int id, AppDb db, ODataQueryOptions<Student> options)
    {
        Student student = await db.Students.FirstOrDefaultAsync(s => s.StudentId == id);
        if (student == null)
        {
            return Results.NotFound($"Cannot find student with id '{id}'");
        }
        else
        {
            return Results.Ok(options.ApplyTo(student, new ODataQuerySettings()));
        }
    }

    public static async Task<IResult> PatchStudentByIdAsync(int id, AppDb db, [FromBody] IDictionary<string, object> properties)
    {
        // TODO: need to support Delta<Student> to replace IDictionary<string, object> in the next step
        Student oldStudent = await db.Students.FirstOrDefaultAsync(s => s.StudentId == id);
        if (oldStudent == null)
        {
            return Results.NotFound($"Cannot find student with id '{id}'");
        }

        int oldSchoolId = oldStudent.SchoolId;

        var studentProperties = typeof(Student).GetProperties();
        foreach (var property in properties)
        {
            PropertyInfo propertyInfo = studentProperties.FirstOrDefault(p => string.Equals(p.Name, property.Key, StringComparison.OrdinalIgnoreCase));
            if (propertyInfo == null)
            {
                return Results.BadRequest($"Cannot find property '{property.Key}' on student");
            }

            // For simplicity
            if (propertyInfo.PropertyType == typeof(string))
            {
                propertyInfo.SetValue(oldStudent, property.Value.ConvertToString());
            }
            else if (propertyInfo.PropertyType == typeof(int))
            {
                propertyInfo.SetValue(oldStudent, property.Value.ConvertToInt());
            }
            else if (propertyInfo.PropertyType == typeof(DateOnly))
            {
                propertyInfo.SetValue(oldStudent, property.Value.ConvertToDateOnly());
            }
        }

        if (oldSchoolId != oldStudent.SchoolId)
        {
            School school = await db.Schools.Include(c => c.Students).FirstOrDefaultAsync(c => c.SchoolId == oldSchoolId);
            school.Students.Remove(oldStudent);

            school = await db.Schools.Include(c => c.Students).FirstOrDefaultAsync(c => c.SchoolId == oldStudent.SchoolId);
            if (school == null)
            {
                return Results.NotFound($"Cannot find school using the school Id '{oldStudent.SchoolId}' that the new student provides.");
            }
            else
            {
                school.Students.Add(oldStudent);
            }
        }

        await db.SaveChangesAsync();

        return Results.Ok(oldStudent);
    }

    public static async Task<IResult> DeleteStudentByIdAsync(int id, AppDb db)
    {
        Student student = await db.Students.FirstOrDefaultAsync(s => s.StudentId == id);
        if (student == null)
        {
            return Results.NotFound($"Cannot find student with id '{id}'");
        }
        else
        {
            db.Students.Remove(student);
            School school = await db.Schools.Include(c => c.Students).FirstOrDefaultAsync(c => c.SchoolId == student.SchoolId);
            school.Students.Remove(student);
            await db.SaveChangesAsync();
            return Results.Ok();
        }
    }

    public static IEndpointRouteBuilder MapStudentEndpoints(this IEndpointRouteBuilder app)
    {
        // Let's use the group to build the student endpoints
        var students = app.MapGroup("/odata");

        // GET http://localhost:5177/odata/students?$select=lastName&$top=3
        students.MapGet("/students", async (AppDb db, ODataQueryOptions<Student> options) =>
        {
            await db.Students.ToListAsync();
            return options.ApplyTo(db.Students);
        });

        //GET http://localhost:5177/odata/students/12?select=lastName
        students.MapGet("/students/{id}", GetStudentByIdAsync);

        //GET http://localhost:5177/odata/students(12)?select=lastName
        students.MapGet("/students({id})", GetStudentByIdAsync);

        // POST http://localhost:5177/odata/students
        // Content-Type: application/json
        // BODY:
        /*
{

"firstName": "Soda",
"lastName": "Yu",
"favoriteSport": "Soccer",
"grade": 7,
"schoolId": 3,
"birthDay": "1977-11-04"
}
        */
        students.MapPost("/students", async (Student student, AppDb db) =>
        {
            int studentId = db.Students.Max(s => s.StudentId) + 1;
            student.StudentId = studentId;
            School school = await db.Schools.Include(c => c.Students).FirstOrDefaultAsync(c => c.SchoolId == student.SchoolId);
            if (school == null)
            {
                return Results.NotFound($"Cannot find school using the school Id '{student.SchoolId}' that the new student provides.");
            }
            else
            {
                school.Students.Add(student);
            }

            db.Students.Add(student);
            await db.SaveChangesAsync();

            return Results.Created($"/odata/students/{studentId}", student);
        });

        // PATCH http://localhost:5177/odata/students/10
        // Content-Type: application/json
        // BODY:
        /*
{
    "firstName": "Sokuda",
    "lastName": "Yu",
    "schoolId": 4
}
         */
        students.MapPatch("/students({id})", PatchStudentByIdAsync);
        students.MapPatch("/students/{id}", PatchStudentByIdAsync);

        // DELETE http://localhost:5177/odata/students/10
        students.MapDelete("/students({id})", DeleteStudentByIdAsync);
        students.MapDelete("/students/{id}", DeleteStudentByIdAsync);
        return app;
    }

    private static string ConvertToString(this object value)
    {
        if (value is JsonElement json && json.ValueKind == JsonValueKind.String)
        {
            return json.GetString();
        }

        throw new InvalidCastException($"Cannot convert '{value}' to string");
    }

    private static int ConvertToInt(this object value)
    {
        if (value is JsonElement json && json.ValueKind == JsonValueKind.Number)
        {
            return json.GetInt32();
        }

        throw new InvalidCastException($"Cannot convert '{value}' to int");
    }

    private static DateOnly ConvertToDateOnly(this object value)
    {
        if (value is JsonElement json && json.ValueKind == JsonValueKind.String)
        {
            string str = json.GetString();
            return DateOnly.Parse(str);
        }

        throw new InvalidCastException($"Cannot convert '{value}' to DateOnly");
    }
}
