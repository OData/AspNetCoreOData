//-----------------------------------------------------------------------------
// <copyright file="DateAndTimeOfDayController.cs" company=".NET Foundation">
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

namespace Microsoft.AspNetCore.OData.E2E.Tests.DateAndTimeOfDay
{
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
                    Date = e % 2 == 0 ? dto.AddDays(e).Date : dto.AddDays(-e).Date,
                    TimeOfDay = e % 3 == 0 ? dto.AddHours(e).TimeOfDay : dto.AddHours(-e).AddMilliseconds(10).TimeOfDay,

                    NullableDateTime = e % 2 == 0 ? (DateTime?)null : dto.AddYears(e).DateTime,
                    NullableOffset = e % 3 == 0 ? (DateTimeOffset?)null : dto.AddMonths(e),
                    NullableDate = e % 2 == 0 ? (Date?)null : dto.AddDays(e).Date,
                    NullableTimeOfDay = e % 3 == 0 ? (TimeOfDay?)null : dto.AddHours(e).TimeOfDay,

                    DateTimes = new [] { dto.AddYears(e).DateTime, dto.AddMonths(e).DateTime },
                    Offsets = new [] { dto.AddMonths(e), dto.AddDays(e) },
                    Dates = new [] { (Date)dto.AddYears(e).Date, (Date)dto.AddMonths(e).Date },
                    TimeOfDays = new [] { (TimeOfDay)dto.AddHours(e).TimeOfDay, (TimeOfDay)dto.AddMinutes(e).TimeOfDay },

                    NullableDateTimes = new [] { dto.AddYears(e).DateTime, (DateTime?)null, dto.AddMonths(e).DateTime },
                    NullableOffsets = new [] { dto.AddMonths(e), (DateTimeOffset?)null, dto.AddDays(e) },
                    NullableDates = new [] { (Date)dto.AddYears(e).Date, (Date?)null, (Date)dto.AddMonths(e).Date },
                    NullableTimeOfDays = new [] { (TimeOfDay)dto.AddHours(e).TimeOfDay, (TimeOfDay?)null, (TimeOfDay)dto.AddMinutes(e).TimeOfDay },

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
        public IActionResult BoundFunction(int key, [FromODataUri]Date modifiedDate, [FromODataUri]TimeOfDay modifiedTime,
            [FromODataUri]Date? nullableModifiedDate, [FromODataUri]TimeOfDay? nullableModifiedTime)
        {
            return Ok(BuildString(modifiedDate, modifiedTime, nullableModifiedDate, nullableModifiedTime));
        }

        [HttpGet("/convention/UnboundFunction(modifiedDate={p1},modifiedTime={p2},nullableModifiedDate={p3},nullableModifiedTime={p4})")]
        [HttpGet("/explicit/UnboundFunction(modifiedDate={p1},modifiedTime={p2},nullableModifiedDate={p3},nullableModifiedTime={p4})")]
        public IActionResult UnboundFunction([FromODataUri]Date p1, [FromODataUri]TimeOfDay p2,
            [FromODataUri]Date? p3, [FromODataUri]TimeOfDay? p4)
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

            Assert.Equal(new Date(2015, 3, 1), parameters["modifiedDate"]);
            Assert.Equal(new TimeOfDay(1, 5, 6, 8), parameters["modifiedTime"]);

            Assert.Null(parameters["nullableModifiedDate"]);
            Assert.Null(parameters["nullableModifiedTime"]);

            IEnumerable<Date> dates = parameters["dates"] as IEnumerable<Date>;
            Assert.NotNull(dates);
            Assert.Equal(2, dates.Count());
        }

        private static string BuildString(Date modifiedDate, TimeOfDay modifiedTime,
            Date? nullableModifiedDate, TimeOfDay? nullableModifiedTime)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("modifiedDate:").Append(modifiedDate).Append(",");
            sb.Append("modifiedTime:").Append(modifiedTime).Append(",");
            sb.Append("nullableModifiedDate:").Append(nullableModifiedDate == null ? "null" : nullableModifiedDate.ToString()).Append(",");
            sb.Append("nullableModifiedTime:").Append(nullableModifiedTime == null ? "null" : nullableModifiedTime.ToString());
            return sb.ToString();
        }
    }

    public class EfCustomersController : ODataController
    {
        private readonly DateAndTimeOfDayContext _db;

        public EfCustomersController(DateAndTimeOfDayContext context)
        {
            context.Database.EnsureCreated();
            _db = context;

            if (!context.Customers.Any())
            {
                DateTimeOffset dateTime = new DateTimeOffset(2014, 12, 24, 1, 2, 3, 4, new TimeSpan(-8, 0, 0));
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
}
