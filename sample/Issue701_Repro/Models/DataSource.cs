/*using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Issue701_Repro.Models
{
    public static class DataSource
    {
        private static Sample _sample { get; set; }

        public static Sample GetSample()
        {
            ensureData();
            return _sample;
        }

        public static IEnumerable<SampleItems> GetSampleItems()
        {
            ensureData();
            return _sample.SItems;
        }

        private static void ensureData()
        {
            if (_sample == null)
            {
                // Create sample item guides
                SampleItemGuide si1 = createSIG(1);
                SampleItemGuide si2 = createSIG(2);
                SampleItemGuide si3 = createSIG(3);

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
                _sample.SItems = SItems;
            }
        }

        private static SampleItemGuide createSIG(int num)
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

/*********************** EXPECTED RESULT ***********************/

// REQUEST: /sample/sampleitems?$expand=SampleItem_guide

/*{
    "@odata.context": "https://localhost:44335/v1.0/$metadata#sample/sampleitems(sampleitem_guide())",
    "value": [
        {
        "uid": "sample.uid",
            
            "sampleitem_guide": [
                {
            "uid": "sampleitem.uid1",
                    "type": "sampletype1"
                },
                {
            "uid": "sampleitem.uid2",
                    "type": "sampletype2"
                }
            ]
        }
		],
    "@odata.nextLink": "https://localhost:44335/v1.0/sample/sampleitems?$skip=1"
}*/





















/************************** PREVIOUS ITERATION ********************************/

/*namespace Issue701_Repro2.Models
{
    public static class DataSource
    {

        private static Sample _sample { get; set; } =  new Sample();

        private static Sample GetSample()
        {
            // Create sample item guides
            SampleItemGuide si1 = new SampleItemGuide
            {
                Uid = "sampleuid1",
                Type = "sampletype1"
            };
            SampleItemGuide si2 = new SampleItemGuide
            {
                Uid = "sampleuid2",
                Type = "sampletype2"
            };
            SampleItemGuide si3 = new SampleItemGuide
            {
                Uid = "sampleuid3",
                Type = "sampletype3"
            };

            // Create sample items
            SampleItems sampleItems1 = new SampleItems();
            sampleItems1.SampleItem_guide.Append(si1);
            sampleItems1.SampleItem_guide.Append(si2);
            SampleItems sampleItems2 = new SampleItems();
            sampleItems2.SampleItem_guide.Append(si3);

            // Create sample
            _sample = new Sample();
            _sample.SItems.Append(sampleItems1);
            _sample.SItems.Append(sampleItems2);

            return _sample;
        }
    }
}*/