using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Issue701_Repro.Models;
using Microsoft.AspNetCore.OData.Query;

namespace Issue701_nextLink.Tests.Controllers
{
    public class SampleController : ODataController
    {
        //[ApiVersion("1.0")]
        //[HttpGet("v{v:apiVersion}/sample")]
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
            return this.Ok(DataSource.GetSampleItems());
        }
    }
}