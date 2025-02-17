namespace OData2Linq.Tests
{
    using OData2Linq.Tests.SampleData;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Xunit;

    public class ODataTests
    {
        [Fact]
        public void CustomKey()
        {
            var result = SampleWithCustomKey.CreateQuery().OData().Filter("Name eq 'n1'").ToArray();

            Assert.Single(result);
            Assert.Equal("n1", result[0].Name);
        }

        [Fact]
        public void WithoutKeyThrowException()
        {
            Assert.Throws<InvalidOperationException>(() => SampleWithoutKey.CreateQuery().OData());
        }

        private static double MemoryUsageInMBStart = default;
        public static IEnumerable<object[]> Iterations() => Enumerable.Range(0, 1000).Select(i => new object[] { i });
        [Theory]
        [MemberData(nameof(Iterations))]
        public void MemoryUsageShouldNotIncrease(int iteration)
        {
            //Arrange
            var items = new List<TestItem>
            {
                new() { Id = Guid.NewGuid(), Name = "Test", Number = 1 },
                new() { Id = Guid.NewGuid(), Name = "Another", Number = 2 }
            };

            //Act
            var odata = items.AsQueryable().OData();
            var filteredItems = odata.Filter("Number eq 2");
            //((IDisposable)odata.ServiceProvider).Dispose(); //Makes no difference in memory usage

            //Assert
            Assert.Equal(1, filteredItems.Count());

            Process currentProc = Process.GetCurrentProcess();
            var bytesInUse = currentProc.PrivateMemorySize64 / (double)1000_000;
            if (MemoryUsageInMBStart == default)
                MemoryUsageInMBStart = bytesInUse;
            Trace.WriteLine($"Private bytes after test run: {Math.Round(bytesInUse, 2)}MB ({MemoryUsageInMBStart}MB at start)(Iteration {iteration})");

            Assert.True(bytesInUse - MemoryUsageInMBStart < 10, "Memory usage should not increase by more than 10MB");
        }
    }
}