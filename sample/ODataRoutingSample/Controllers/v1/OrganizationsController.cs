// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using ODataRoutingSample.Models;

namespace ODataRoutingSample.Controllers.v1
{
    [ODataModel("v1")]
    public class OrganizationsController : Controller
    {
        [EnableQuery]
        public IActionResult Post([FromBody]Organization org)
        {
            /*
             * You can send a Post request with the request body as follows (v4.0):
            {
              "@odata.context":"http://localhost:5000/v1/$metadata#Organizations/$entity",
              "Name": "Peter",
              "Departs@odata.bind":[ "Departments(4)", "Departments(5)"]
            }

            or 4.01 request body as follows:

            {
              "@odata.context":"http://localhost:5000/v1/$metadata#Organizations/$entity",
              "Name": "Peter",
              "Departs": [{
                 "@odata.id": "Departments(4)"
              },
              {
                  "@odata.id": "Departments(5)"
                  "Alias": "No2"
              }
              ]
            }

            */

            org.OrganizationId = 99; // 99 is just for testing
            return Ok(org);
        }

        [HttpPatch]
        [EnableQuery]
       // public IActionResult Patch(EdmChangedObjectCollection changes)
        public IActionResult Patch(DeltaSet<Organization> changes)
        {
            /*
{
  "@odata.context":"http://localhost/$metadata#Organizations/$delta",
  "value":[
   {
      "@odata.id":"Organizations(42)",
      "Name":"Micrsoft"
   },
   {
     "@odata.context":"http://localhost/$metadata#Organizations/$deletedLink",
     "source":"Organizations(32)",
     "relationship":"Departs",
     "target":"Departs(12)"
   },
   {
     "@odata.context":"http://localhost/$metadata#Organizations/$link",
     "source":"Organizations(22)",
     "relationship":"Departs",
     "target":"Departs(2)"
   },
   {
     "@odata.context":"http://localhost/$metadata#Organizations/$deletedEntity",
     "id":"Organizations(12)",
     "reason":"deleted"
   }
  ]
} 
             */

            //changes.ApplyDeleteLink = (l) => { };

            //IList<Organization> originalSet = new List<Organization>();

            // changes.Patch(originalSet);

            return Ok();
        }

        [HttpGet]
        public IActionResult GetPrice([FromODataUri]string organizationId, [FromODataUri] string partId)
        {
            return Ok($"Caculated the price using {organizationId} and {partId}");
        }

        [ODataRouting]
        [HttpGet("v1/Organizations/GetPrice2(organizationId={orgId},partId={parId})")]
        public IActionResult GetMorePrice(string orgId, string parId)
        {
            return Ok($"Caculated the price using {orgId} and {parId}");
        }

        [ODataRouting]
        [HttpGet("v1/Organizations/GetPrice2(organizationId={orgId},partId={parId})/GetPrice2(organizationId={orgId2},partId={parId2})")]
        public IActionResult GetMorePrice2(string orgId, string parId, string orgId2, string parId2)
        {
            return Ok($"Caculated the price using {orgId} and {parId} | using {orgId2} and {parId2}");
        }

        [ODataRouting]
        [HttpPost("v1/Organizations/GetByAccount(accountId={aId})/MarkAsFavourite")]
        public IActionResult MarkAsFavouriteAfterGetByAccount(string aId)
        {
            /*
             * It works for the following request;
             * POST http://localhost:5000/v1/Organizations/GetByAccount(accountId=99)/MarkAsFavourite
             * */
            return Ok($"MarkAsFavouriteAfterGetByAccount after {aId}");
        }
    }
}
