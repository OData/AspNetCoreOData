using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Routing.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Issue701_Repro.Models;
using Microsoft.OData;
using Microsoft.AspNetCore.OData.Query;

namespace Issue701_nextLink.Tests.Controllers
{
    public class SampleController : ODataController
    {
        //[ApiVersion("1.0")]
        //        [HttpGet("v{v:apiVersion}/sample")]
        [HttpGet("sample")]
        [EnableQuery]
        public IActionResult GetSampleAsync(/*[FromQuery] QueryStringParameters queryString*/)
        {
            return this.Ok(DataSource.GetSample());
        }

        //[ApiVersion("1.0")]
        //[HttpGet("v{v:apiVersion}/sample/sampleitems")]
        [HttpGet("sample/SItems")]
        [EnableQuery(PageSize = 2)]
        public IActionResult GetCatalogExamsAsync(/*[FromQuery] QueryStringParameters queryString*/)
        {
            var sItems = DataSource.GetSampleItems();
            return this.Ok(sItems);
        }
    }
}