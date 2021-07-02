using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DateAndTimeOfDay
{

    public class DateAndTimeOfDayModelsController : ODataController
    {

        private EfDateAndTimeOfDayModelContext _db;

        public DateAndTimeOfDayModelsController(EfDateAndTimeOfDayModelContext context)
        {
            context.Database.EnsureCreated();
            if (!context.DateTimes.Any())
            {
                DateTime dt = new DateTime(2015, 12, 22);

                IList<DateAndTimeOfDayModel> dateTimes = Enumerable.Range(1, 5).Select(i =>
                    new DateAndTimeOfDayModel
                    {
                        // Id = i,
                        Birthday = dt.AddYears(i),
                        EndDay = dt.AddDays(i),
                        DeliverDay = i % 2 == 0 ? (DateTime?)null : dt.AddYears(5 - i),
                        PublishDay = i % 2 != 0 ? (DateTime?)null : dt.AddMonths(5 - i),
                        CreatedTime = new TimeSpan(0, i, 3, 5, 79),
                        EndTime = i % 2 == 0 ? (TimeSpan?)null : new TimeSpan(0, 10 + i, 3 + i, 5 + i, 79 + i),
                        ResumeTime = new TimeSpan(0, 8, 6, 4, 3)
                    }).ToList();

                foreach (var efDateTime in dateTimes)
                {
                    context.DateTimes.Add(efDateTime);
                }

                context.SaveChanges();
            }

            _db = context;
        }

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_db.DateTimes);
        }

        [EnableQuery]
        public IActionResult Get(int key)
        {
            DateAndTimeOfDayModel dtm = _db.DateTimes.FirstOrDefault(e => e.Id == key);
            if (dtm == null)
            {
                return NotFound();
            }

            return Ok(dtm);
        }

        public IActionResult Post([FromBody] DateAndTimeOfDayModel dt)
        {
            Assert.NotNull(dt);

            Assert.Equal(99, dt.Id);
            Assert.Equal(new DateTime(2099, 1, 1), dt.Birthday);
            Assert.Equal(new TimeSpan(0, 14, 13, 15, 179), dt.CreatedTime);
            Assert.Equal(new DateTime(1990, 12, 22), dt.EndDay);

            return Created(dt);
        }

        public IActionResult Put(int key, [FromBody] Delta<DateAndTimeOfDayModel> dt)
        {
            Assert.Equal(new[] { "Birthday", "CreatedTime" }, dt.GetChangedPropertyNames());

            // Birthday
            object value;
            bool success = dt.TryGetPropertyValue("Birthday", out value);
            Assert.True(success);
            DateTime dateTime = Assert.IsType<DateTime>(value);
            Assert.Equal(DateTimeKind.Unspecified, dateTime.Kind);
            Assert.Equal(new DateTime(2199, 1, 2), dateTime);

            // CreatedTime
            success = dt.TryGetPropertyValue("CreatedTime", out value);
            Assert.True(success);
            TimeSpan timeSpan = Assert.IsType<TimeSpan>(value);
            Assert.Equal(new TimeSpan(0, 14, 13, 15, 179), timeSpan);
            return Updated(dt);
        }

    }

}
