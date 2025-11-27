//-----------------------------------------------------------------------------
// <copyright file="DateOnlyAndTimeOnlyController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DateOnlyAndTimeOnly;

public class DCustomersController : ODataController
{
    private static IList<DCustomer> _customers;

    private static void InitCustomers()
    {
        DateTimeOffset dto = new DateTimeOffset(2015, 1, 1, 1, 2, 3, 4, TimeSpan.Zero);
        _customers = Enumerable.Range(1, 5).Select(e =>
            new DCustomer
            {
                Id = e,
                DateTime = dto.AddYears(e).DateTime,
                Offset = e % 2 == 0 ? dto.AddMonths(e) : dto.AddDays(e).AddMilliseconds(10),
                DateOnly = e % 2 == 0 ? DateOnly.FromDateTime(dto.AddDays(e).Date) : DateOnly.FromDateTime(dto.AddDays(-e).Date),
                TimeOnly = e % 3 == 0 ? TimeOnly.FromTimeSpan(dto.AddHours(e).TimeOfDay) : TimeOnly.FromTimeSpan(dto.AddHours(-e).AddMilliseconds(10).TimeOfDay),

                NullableDateTime = e % 2 == 0 ? (DateTime?)null : dto.AddYears(e).DateTime,
                NullableOffset = e % 3 == 0 ? (DateTimeOffset?)null : dto.AddMonths(e),
                NullableDateOnly = e % 2 == 0 ? (DateOnly?)null : DateOnly.FromDateTime(dto.AddDays(e).Date),
                NullableTimeOnly = e % 3 == 0 ? (TimeOnly?)null : TimeOnly.FromTimeSpan(dto.AddHours(e).TimeOfDay),

                DateTimes = new[] { dto.AddYears(e).DateTime, dto.AddMonths(e).DateTime },
                Offsets = new[] { dto.AddMonths(e), dto.AddDays(e) },
                DateOnlys = new[] { DateOnly.FromDateTime(dto.AddYears(e).Date), DateOnly.FromDateTime(dto.AddMonths(e).Date) },
                TimeOnlys = new[] { TimeOnly.FromTimeSpan(dto.AddHours(e).TimeOfDay), TimeOnly.FromTimeSpan(dto.AddMinutes(e).TimeOfDay) },

                NullableDateTimes = new[] { dto.AddYears(e).DateTime, (DateTime?)null, dto.AddMonths(e).DateTime },
                NullableOffsets = new[] { dto.AddMonths(e), (DateTimeOffset?)null, dto.AddDays(e) },
                NullableDateOnlys = new[] { DateOnly.FromDateTime(dto.AddYears(e).Date), (DateOnly?)null, DateOnly.FromDateTime(dto.AddMonths(e).Date) },
                NullableTimeOnlys = new[] { TimeOnly.FromTimeSpan(dto.AddHours(e).TimeOfDay), (TimeOnly?)null, TimeOnly.FromTimeSpan(dto.AddMinutes(e).TimeOfDay) },

            }).ToList();
    }

    public DCustomersController()
    {
        if (_customers == null)
        {
            InitCustomers();
        }
    }

    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(_customers);
    }

    public IActionResult Get(int key)
    {
        DCustomer customer = _customers.FirstOrDefault(e => e.Id == key);
        if (customer == null)
        {
            return NotFound();
        }

        return Ok(customer);
    }

    [HttpGet]
    public IActionResult BoundFunction(int key, [FromODataUri] DateOnly modifiedDate, [FromODataUri] TimeOnly modifiedTime,
        [FromODataUri] DateOnly? nullableModifiedDate, [FromODataUri] TimeOnly? nullableModifiedTime)
    {
        return Ok(BuildString(modifiedDate, modifiedTime, nullableModifiedDate, nullableModifiedTime));
    }

    [HttpGet("/convention/UnboundFunction(modifiedDate={p1},modifiedTime={p2},nullableModifiedDate={p3},nullableModifiedTime={p4})")]
    [HttpGet("/explicit/UnboundFunction(modifiedDate={p1},modifiedTime={p2},nullableModifiedDate={p3},nullableModifiedTime={p4})")]
    public IActionResult UnboundFunction([FromODataUri] DateOnly p1, [FromODataUri] TimeOnly p2,
        [FromODataUri] DateOnly? p3, [FromODataUri] TimeOnly? p4)
    {
        return Ok(BuildString(p1,p2,p3,p4));
    }

    [HttpPost]
    public IActionResult BoundAction(int key, [FromBody]ODataActionParameters parameters)
    {
        VerifyActionParameters(parameters);
        return Ok(true);
    }

    [HttpPost("convention/UnboundAction")]
    [HttpPost("explicit/UnboundAction")]
    public IActionResult UnboundAction([FromBody]ODataActionParameters parameters)
    {
        VerifyActionParameters(parameters);
        return Ok(true);
    }

    private static void VerifyActionParameters([FromBody]ODataActionParameters parameters)
    {
        Assert.True(parameters.ContainsKey("modifiedDate"));
        Assert.True(parameters.ContainsKey("modifiedTime"));
        Assert.True(parameters.ContainsKey("nullableModifiedDate"));
        Assert.True(parameters.ContainsKey("nullableModifiedTime"));
        Assert.True(parameters.ContainsKey("dates"));

        Assert.Equal(new DateOnly(2015, 3, 1), parameters["modifiedDate"]);
        Assert.Equal(new TimeOnly(1, 5, 6, 8), parameters["modifiedTime"]);

        Assert.Null(parameters["nullableModifiedDate"]);
        Assert.Null(parameters["nullableModifiedTime"]);

        IEnumerable<DateOnly> dates = parameters["dates"] as IEnumerable<DateOnly>;
        Assert.NotNull(dates);
        Assert.Equal(2, dates.Count());
    }

    private static string BuildString(DateOnly modifiedDate, TimeOnly modifiedTime,
        DateOnly? nullableModifiedDate, TimeOnly? nullableModifiedTime)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("modifiedDate:").Append(modifiedDate.ToODataString()).Append(",");
        sb.Append("modifiedTime:").Append(modifiedTime.ToODataString()).Append(",");
        sb.Append("nullableModifiedDate:").Append(nullableModifiedDate == null ? "null" : nullableModifiedDate?.ToODataString()).Append(",");
        sb.Append("nullableModifiedTime:").Append(nullableModifiedTime == null ? "null" : nullableModifiedTime?.ToODataString());
        return sb.ToString();
    }
}

public class EfCustomersController : ODataController
{
    private readonly DateOnlyAndTimeOnlyContext _db;

    public EfCustomersController(DateOnlyAndTimeOnlyContext context)
    {
        // EnsureDeleted and EnsureCreated are called to force the database schema to refresh on every test run.
        // This is necessary because new properties (e.g., 'DateOnly') were added to the model, and EF Core migrations
        // may not automatically update the schema in test scenarios.
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        _db = context;

        if (!context.Customers.Any())
        {
            DateTimeOffset dateTime = new DateTimeOffset(2014, 12, 24, 1, 2, 3, 4, new TimeSpan(-8, 0, 0));
            DateOnly dateOnly = DateOnly.FromDateTime(dateTime.Date);
            TimeOnly timeOnly = TimeOnly.FromTimeSpan(dateTime.TimeOfDay);
            IEnumerable<EfCustomer> customers = Enumerable.Range(1, 5).Select(e =>
                new EfCustomer
                {
                  //  Id = e,
                    DateTime = dateTime.AddYears(e).AddHours(e).AddMilliseconds(e).DateTime,
                    NullableDateTime = e % 2 == 0 ? (DateTime?)null : dateTime.AddHours(e * 5).AddMilliseconds(e * 5).DateTime,
                    Offset = dateTime.AddMonths(e).AddHours(e).AddMilliseconds(e),
                    NullableOffset = e % 3 == 0 ? (DateTimeOffset?)null : dateTime.AddDays(e).AddHours(e * 5)
                }).ToList();

            foreach (EfCustomer customer in customers)
            {
                context.Customers.Add(customer);
            }

            context.SaveChanges();
        }
    }

    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(_db.Customers);
    }

    public IActionResult Get(int key)
    {
        EfCustomer customer = _db.Customers.FirstOrDefault(e => e.Id == key);
        if (customer == null)
        {
            return NotFound();
        }

        return Ok(customer);
    }
}

public class EfPeopleController : ODataController
{
    private readonly EdmDateWithEfContext _db;

    public EfPeopleController(EdmDateWithEfContext context)
    {
        context.Database.EnsureCreated();
        _db = context;

        if (_db.People.Any())
        {
            return;
        }

        var people = Enumerable.Range(1, 5).Select(e => new EfPerson
        {
           // Id = e,
            Birthday = e % 2 == 0 ? (DateTime?)null : new DateTime(2015, 10, e)
        });

        foreach (var person in people)
        {
            _db.People.Add(person);
        }

        _db.SaveChanges();
    }

    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(_db.People);
    }

    [EnableQuery]
    public async Task<SingleResult<EfPerson>> Get(int key)
    {
        return await Task.FromResult(SingleResult.Create(_db.People.Where(c => c.Id == key)));
    }
}
