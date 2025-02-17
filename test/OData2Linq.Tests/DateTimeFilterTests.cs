using OData2Linq.Tests.SampleData;
using Xunit;
using Xunit.Abstractions;

namespace OData2Linq.Tests
{
    public class DateTimeFilterTests
    {
        private readonly ITestOutputHelper output;
        private static readonly DateTime dtUtc = new DateTime(2018, 1, 26, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime dtLocal = new DateTime(2018, 1, 26, 0, 0, 0, DateTimeKind.Local);
        private static readonly DateTime dt = new DateTime(2018, 1, 26, 0, 0, 0);
        private static readonly DateTimeOffset dto = new DateTimeOffset(dt, TimeZoneInfo.Local.BaseUtcOffset);
        private static readonly DateOnly do1 = new(2018, 1, 26);
        private static readonly TimeOnly to1 = new(12, 34, 56);
        //private readonly DateTimeOffset dtoUtc = new DateTimeOffset(new DateTime(2018, 1, 26).ToUniversalTime());

        public DateTimeFilterTests(ITestOutputHelper output)
        {
            this.output = output ?? throw new ArgumentNullException(nameof(output));
        }

        [Fact]
        public void LocalDateTimeEqualCorrectUtcOffset()
        {
            string value = dto.ToString("u").Replace(" ", "T");
            string filter = $"DateTime eq {value}";
            output.WriteLine(filter);

            var result = SimpleClass.CreateQuery().OData().Filter(filter).ToArray();
            Assert.Single(result);
            output.WriteLine(result.Single().DateTime.ToString());
        }

        [Fact]
        public void LocalDateTimeEqualCorrectLocalOffset()
        {
            string value = dto.ToString("s") + dto.ToString("zzz");
            string filter = $"DateTime eq {value}";
            output.WriteLine(filter);

            var result = SimpleClass.CreateQuery().OData().Filter(filter).ToArray();
            Assert.Single(result);
            output.WriteLine(result.Single().DateTime.ToString());
        }

        [Fact]
        public void LocalDateTimetNotEquaIncorrectUtcOffset()
        {
            string value = dto.ToString("s") + "Z";
            string filter = $"DateTime eq {value}";
            output.WriteLine(filter);

            var result = SimpleClass.CreateQuery().OData().Filter(filter).ToArray();
            if (TimeZoneInfo.Local.Equals(TimeZoneInfo.Utc))
                Assert.Single(result);
            else
                Assert.Empty(result);
        }

        [Fact]
        public void DateTimeOffsetEqualCorrectUtcOffset()
        {
            string value = dto.ToString("u").Replace(" ", "T");
            string filter = $"DateTimeOffset eq {value}";
            output.WriteLine(filter);

            var result = SimpleClass.CreateQuery().OData().Filter(filter).ToArray();
            Assert.Single(result);
            output.WriteLine(result.Single().DateTimeOffset.ToString());

        }

        [Fact]
        public void DateTimeOffsetEqualCorrectLocalOffset()
        {
            string value = dto.ToString("s") + dto.ToString("zzz");
            string filter = $"DateTimeOffset eq {value}";
            output.WriteLine(filter);

            var result = SimpleClass.CreateQuery().OData().Filter(filter).ToArray();
            Assert.Single(result);
            output.WriteLine(result.Single().DateTimeOffset.ToString());
        }

        [Fact]
        public void DateTimeOffsetNotEqualIncorrectUtcOffset()
        {
            string value = dto.ToString("s") + "Z";
            string filter = $"DateTimeOffset eq {value}";
            output.WriteLine(filter);

            var result = SimpleClass.CreateQuery().OData().Filter(filter).ToArray();
            if (TimeZoneInfo.Local.Equals(TimeZoneInfo.Utc))
                Assert.Single(result);
            else
                Assert.Empty(result);
        }

        [Fact]
        public void UtcDateTimeEqualCorrectUtcOffset()
        {
            string value = dto.ToString("s") + "Z";
            //string value = dto.ToString("u").Replace(" ", "T");
            string filter = $"DateTime eq {value}";
            output.WriteLine(filter);

            var result = SimpleClass.CreateQuery().OData(c => c.QuerySettings.TimeZone = TimeZoneInfo.Utc).Filter(filter).ToArray();
            Assert.Single(result);
            output.WriteLine(result.Single().DateTime.ToString() + "UTC");

        }

        [Fact]
        public void UtcDateTimeEqualCorrectLocalOffset()
        {
            string value = dto.DateTime.ToLocalTime().ToString("s").Replace(" ", "T") + dto.ToString("zzz");
            string filter = $"DateTime eq {value}";
            output.WriteLine(filter);

            var result = SimpleClass.CreateQuery().OData(c => c.QuerySettings.TimeZone = TimeZoneInfo.Utc).Filter(filter).ToArray();
            Assert.Single(result);
            output.WriteLine(result.Single().DateTime.ToString() + "UTC");
        }

        [Fact]
        public void UtcDateTimeNotEqualIncorrectLocalOffset()
        {
            string value = dto.ToString("u").Replace(" ", "T").Replace("Z", dto.ToString("zzz"));
            string filter = $"DateTime eq {value}";
            output.WriteLine(filter);

            var result = SimpleClass.CreateQuery().OData(c => c.QuerySettings.TimeZone = TimeZoneInfo.Utc).Filter(filter).ToArray();
            if (TimeZoneInfo.Local.Equals(TimeZoneInfo.Utc))
                Assert.Single(result);
            else
                Assert.Empty(result);
        }

        [Fact]
        public void UtcDateTimeNotEqualIncorrectLocalOffset2()
        {
            string value = dto.ToString("s") + dto.ToString("zzz");
            string filter = $"DateTime eq {value}";
            output.WriteLine(filter);

            var result = SimpleClass.CreateQuery().OData(c => c.QuerySettings.TimeZone = TimeZoneInfo.Utc).Filter(filter).ToArray();
            if (TimeZoneInfo.Local.Equals(TimeZoneInfo.Utc))
                Assert.Single(result);
            else
                Assert.Empty(result);
        }

        [Fact]
        public void UtcDateTimetNotEqualIncorrectUtcOffset()
        {
            string value = dto.ToString("u").Replace(" ", "T");
            string filter = $"DateTime eq {value}";
            output.WriteLine(filter);

            var result = SimpleClass.CreateQuery().OData(c => c.QuerySettings.TimeZone = TimeZoneInfo.Utc).Filter(filter).ToArray();

            if (TimeZoneInfo.Local.Equals(TimeZoneInfo.Utc))
                Assert.Single(result);
            else
                Assert.Empty(result);
        }

        [Fact]
        public void DateTimeCompare()
        {
            // Compare method compares the Ticks property of t1 and t2 but ignores their Kind property.
            // Before comparing DateTime objects, ensure that the objects represent times in the same time zone.
            Assert.Equal(dt, dtLocal);
            Assert.Equal(dt, dtUtc);
            Assert.Equal(dtUtc, dtLocal);

            if (!TimeZoneInfo.Local.Equals(TimeZoneInfo.Utc))
            {
                Assert.NotEqual(TimeZoneInfo.Local, TimeZoneInfo.Utc);
                Assert.NotEqual(TimeZoneInfo.Local.GetHashCode(), TimeZoneInfo.Utc.GetHashCode());
            }
            Assert.Equal(TimeZoneInfo.Utc, TimeZoneInfo.Utc);
            Assert.Equal(TimeZoneInfo.Utc.GetHashCode(), TimeZoneInfo.Utc.GetHashCode());
        }

        [Fact]
        public void DateOnlyFilterWorks()
        {
            string value = do1.ToString("yyyy-MM-dd");
            string filter = $"{nameof(SimpleClass.DateOnly)} eq {value}";
            output.WriteLine(filter);

            var result = SimpleClass.CreateQuery().OData().Filter(filter).ToArray();
            Assert.Single(result);
            Assert.True(result.First().DateOnly == do1);
        }

        [Fact]
        public void TimeOnlyFilterWorks()
        {
            string value = to1.ToString("HH:mm:ss");
            string filter = $"{nameof(SimpleClass.TimeOnly)} eq {value}";
            output.WriteLine(filter);

            var result = SimpleClass.CreateQuery().OData().Filter(filter).ToArray();
            Assert.Single(result);
            Assert.True(result.First().TimeOnly == to1);
        }

    }
}
