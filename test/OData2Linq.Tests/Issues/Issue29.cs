//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Linq;
//using System.Text;
//using Xunit;
//using Community.OData.Linq.Json;

//namespace OData2Linq.Tests.Issues29
//{
//    using Microsoft.AspNetCore.OData.Query.Wrapper;
//    using System.Diagnostics;

//    class MyClassA
//    {
//        [Key]
//        public string String { get; set; }

//        public List<MyClassB> Subs { get; set; }
//    }

//    class MyClassB
//    {
//        [Key]
//        public int Integer { get; set; }
//    }

//    public class Issue29
//    {
//        [Fact]
//        public void SubLevelFilterTest()
//        {
//            ODataQuery<MyClassA> odataQuery = GetSampleData().OData();
//            odataQuery = odataQuery.Filter("String eq 'A'");

//            // This is what I used until now; good result
//            string serializeCorrect = JsonConvert.SerializeObject(odataQuery);

//            IEnumerable<ISelectExpandWrapper> collection = GetSampleData().OData().Filter("String eq 'A'").SelectExpand("String", "Subs($filter=Integer eq 1)");            
//            string result1 = collection.ToJson().ToString();

//            MyClassA[] array = GetSampleData().OData().Filter("String eq 'A'").ToArray();
//            foreach (MyClassA item in array)
//            {
//                item.Subs = item.Subs.AsQueryable().OData().Filter("Integer eq 1").ToList();
//            }

//            string result2 = JsonConvert.SerializeObject(array);
//        }

//        private static IQueryable<MyClassA> GetSampleData()
//        {
//            return new List<MyClassA>()
//            {
//                new MyClassA()
//                {
//                    String = "A",
//                    Subs = new List<MyClassB>() {{new MyClassB() {Integer = 1}}, {new MyClassB() {Integer = 2}}, {new MyClassB() {Integer = 3}}, {new MyClassB() {Integer = 4}}}
//                },
//                new MyClassA()
//                {
//                    String = "B",
//                    Subs = new List<MyClassB>() {{new MyClassB() {Integer = 1}}, {new MyClassB() {Integer = 2}}, {new MyClassB() {Integer = 3}}, {new MyClassB() {Integer = 4}}}
//                }
//            }.AsQueryable();
//        }

//        private static IQueryable<MyClassA> GetBigSampleData()
//        {
//            var result = Enumerable.Range(1, 2000).Select(
//                i => new MyClassA
//                         {
//                             String = i <= 1000 ? "A" : "B",
//                             Subs = Enumerable.Range(1, 1000).Select(j => new MyClassB { Integer = j })
//                                 .ToList()
//                         });

//            return result.AsQueryable();
//        }

//        [Fact]
//        public void FilterMany1()
//        {
//            Stopwatch stopwatch = new Stopwatch();
//            stopwatch.Start();
//            ISelectExpandWrapper[] a1000b1 = GetBigSampleData().OData().Filter("String eq 'A'").SelectExpand("String", "Subs($filter=Integer eq 1)").ToArray();

//            Assert.True(stopwatch.ElapsedMilliseconds < 5000, "query performance");
//            Assert.Equal(1000, a1000b1.Length);


//            string a1000b1Json = a1000b1.ToJson().ToString();
//            Assert.NotNull(a1000b1Json);

//            Assert.True(stopwatch.ElapsedMilliseconds < 10000, "to json performance");
//            stopwatch.Stop();
//        }

//        [Fact]
//        public void FilterMany3()
//        {
//            Stopwatch stopwatch = new Stopwatch();
//            stopwatch.Start();
//            IEnumerable<ISelectExpandWrapper> a1000b1 = GetBigSampleData().OData().Filter("String eq 'A'").SelectExpand("String", "Subs($filter=Integer eq 1)");

//            Assert.True(stopwatch.ElapsedMilliseconds < 5000, "query performance");            

//            string a1000b1Json = a1000b1.ToJson().ToString();
//            Assert.NotNull(a1000b1Json);

//            Assert.True(stopwatch.ElapsedMilliseconds < 10000, "to json performance");
//            stopwatch.Stop();
//        }

//        [Fact]
//        public void FilterMany2()
//        {
//            Stopwatch stopwatch = new Stopwatch();
//            stopwatch.Start();
//            ISelectExpandWrapper[] a1000b1000 = GetBigSampleData().OData().SelectExpand("String", "Subs").ToArray();

//            Assert.True(stopwatch.ElapsedMilliseconds < 5000, "query performance");
//            Assert.Equal(2000, a1000b1000.Length);            


//            string a1000b1000Json = a1000b1000.ToJson().ToString();
//            Assert.NotNull(a1000b1000Json);
//            Assert.True(stopwatch.ElapsedMilliseconds < 10000, "to json performance");
//            stopwatch.Stop();
//        }
//    }
//}
