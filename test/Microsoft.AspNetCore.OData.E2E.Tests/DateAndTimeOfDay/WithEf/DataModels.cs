using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DateAndTimeOfDay
{

    public class DateAndTimeOfDayModel
    {

        public int Id { get; set; }

        [Column(TypeName = "date")]
        public DateTime Birthday { get; set; }

        [Column(TypeName = "DaTe")]
        public DateTime? PublishDay { get; set; }

        public DateTime EndDay { get; set; } // will use the Fluent API

        public DateTime? DeliverDay { get; set; } // will use the Fluent API

        [Column(TypeName = "time")]
        public TimeSpan CreatedTime { get; set; }

        [Column(TypeName = "tIme")]
        public TimeSpan? EndTime { get; set; }

        public TimeSpan ResumeTime { get; set; } // will use the Fluent API

    }

}
