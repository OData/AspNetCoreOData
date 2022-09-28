using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Singleton
{
    /// <summary>
    /// Present a sample situation for Issue #701
    /// </summary>
    [Route("odata/Sample")]
    public class SampleController : ODataController
    {
        public static Sample _sample;

        static SampleController()
        {
            InitData();
        }

        private static void InitData()
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

        private static SampleItemGuide CreateSIG(int num)
        {
            SampleItemGuide sig = new SampleItemGuide
            {
                Uid = "sampleuid" + num,
                Type = "sampletype" + num
            };
            return sig;
        }

        #region Query
        [HttpGet("")]
        [EnableQuery]
        public IActionResult GetSampleAsync()
        {
            return Ok(_sample);
        }

        [HttpGet("SItems")]
        [EnableQuery(PageSize = 2)]
        public IActionResult GetCatalogExamsAsync()
        {
            return Ok(_sample.SItems);
        }
        #endregion
    }
}
