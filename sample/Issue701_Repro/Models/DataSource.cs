using System.Collections.Generic;

namespace Issue701_Repro.Models
{
    public static class DataSource
    {
        private static Sample _sample { get; set; }

        public static Sample GetSample()
        {
            EnsureData();
            return _sample;
        }

        public static IEnumerable<SampleItems> GetSampleItems()
        {
            EnsureData();
            return _sample.SItems;
        }

        private static void EnsureData()
        {
            if (_sample == null)
            {
                // Create sample item guides
                SampleItemGuide si1 = CreateSIG(1);
                SampleItemGuide si2 = CreateSIG(2);
                SampleItemGuide si3 = CreateSIG(3);

                // Create sample items
                SampleItems sampleItems1 = new SampleItems
                {
                    Uid = "sampleitems1"
                };
                var items1 = new List<SampleItemGuide>();
                items1.Add(si1);
                items1.Add(si2);
                items1.Add(si3);
                sampleItems1.SampleItem_guide = items1;

                SampleItems sampleItems2 = new SampleItems
                {
                    Uid = "sampleitems2"
                };
                var items2 = new List<SampleItemGuide>();
                items2.Add(si3);
                sampleItems2.SampleItem_guide = items2;

                // Create sample
                _sample = new Sample();
                var SItems = new List<SampleItems>();
                SItems.Add(sampleItems1);
                SItems.Add(sampleItems2);
                SItems.Add(sampleItems2);
                SItems.Add(sampleItems2);
                _sample.SItems = SItems;
            }
        }

        private static SampleItemGuide CreateSIG(int num)
        {
            SampleItemGuide sig = new SampleItemGuide
            {
                Uid = "sampleuid" + num,
                Type = "sampletype" + num
            };
            return sig;
        }
    }
}