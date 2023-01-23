//-----------------------------------------------------------------------------
// <copyright file="OrganizationsController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using ODataRoutingSample.Models;

namespace ODataRoutingSample.Controllers.v1
{
    [ODataRouteComponent("v1")]
    [ODataAttributeRouting]
    public class OrganizationsController : Controller
    {
        [EnableQuery]
        public IActionResult Post([FromBody] Organization org)
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
      "Name":"Microsoft"
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

        [HttpPatch]
        [EnableQuery]
        public IActionResult Patch(int key, Delta<Organization> delta)
        {
            /* Send a PATCH request to: http://localhost:5000/v1/Organizations/1
             * using the following payload (v4.01 format, should enable the EnableReadingODataAnnotationWithoutPrefix on ODataSimplifiedOptions)
{
   "Departs@delta": [
    {
     "@removed":{"reason":"deleted" },
     "@id":"Departments(13)"
    },
    {
      "@id":"Departments(42)",
      "Name":"Microsoft"
   }
  ]
}
     */

            // Or using the following payload (v4.0 format)
            /*
{
   "Departs@delta": [
    {
     "@odata.context":"http://localhost:5000/v1/$metadata#Departments/$deletedEntity",
     "id":"Departments(13)",
     "reason":"deleted"
    },
    {
      "@odata.id":"Departments(42)",
      "Name":"Microsoft"
   }
  ]
}

            Be noted: the "id" should go before "reason", otherwise we can't read the "id" value.
            It's a bug in ODL side.
             */

            if (delta != null && delta.TryGetPropertyValue("Departs", out object value))
            {
                if (value is DeltaSet<Department> departs)
                {
                    IList<string> sb = new List<string>();
                    foreach (var setItem in departs)
                    {
                        if (setItem is IDeltaDeletedResource deletedResource)
                        {
                            sb.Add($"   |-> A DeletedResource Id = {deletedResource.Id}");
                        }
                        else if (setItem is IDelta deltaResource)
                        {
                            sb.Add($"   |-> A Delta Resource With ChangedProperties = {string.Join(",", deltaResource.GetChangedPropertyNames())}");
                        }
                        else
                        {
                            sb.Add($"   |-> Not fully supported: {setItem.Kind}");
                        }
                    }

                    return Ok(sb);
                }
            }

            return Ok();
        }

        public IActionResult GetName(int key)
        {
            Organization org = new Organization
            {
                OrganizationId = 9,
                Name = "MyName"
            };

            return Ok(org.Name);
        }

        [HttpGet]
        public IActionResult GetPrice([FromODataUri] string organizationId, [FromODataUri] string partId)
        {
            return Ok($"Calculated the price using {organizationId} and {partId}");
        }

        [HttpGet("v1/Organizations/GetPrice2(organizationId={orgId},partId={parId})")]
        public IActionResult GetMorePrice(string orgId, string parId)
        {
            return Ok($"Calculated the price using {orgId} and {parId}");
        }

        [HttpGet("v1/Organizations/GetPrice2(organizationId={orgId},partId={parId})/GetPrice2(organizationId={orgId2},partId={parId2})")]
        public IActionResult GetMorePrice2(string orgId, string parId, string orgId2, string parId2)
        {
            return Ok($"Calculated the price using {orgId} and {parId} | using {orgId2} and {parId2}");
        }

        [HttpPost("v1/Organizations/GetByAccount(accountId={aId})/MarkAsFavourite")]
        public IActionResult MarkAsFavouriteAfterGetByAccount(string aId)
        {
            /*
             * It works for the following request;
             * POST http://localhost:5000/v1/Organizations/GetByAccount(accountId=99)/MarkAsFavourite
             * */
            return Ok($"MarkAsFavouriteAfterGetByAccount after {aId}");
        }

        [HttpPost("v1/Organizations/GetByAccount2(accountId={aId})/{key}/MarkAsFavourite")]
        // [HttpPost("v1/Organizations/GetByAccount2(accountId={aId})({key})/MarkAsFavourite")] this syntax has ODL problem.
        public IActionResult MarkAsFavouriteAfterGetByAccount2(int key, string aId)
        {
            /*
             * It works for the following request;
             * POST http://localhost:5000/v1/Organizations/GetByAccount2(accountId=99)/4/MarkAsFavourite
             * */
            return Ok($"MarkAsFavourite2AfterGetByAccount2 after {aId} with key={key}");
        }

        /* Conventional routing builds the following two routing templates:
GET ~/v1/Organizations({key})/{navigationProperty}/$ref
GET ~/v1/Organizations/{key}/{navigationProperty}/$ref
         */
        [HttpGet]
        public IActionResult GetRef(int key, string navigationProperty)
        {
            return Ok($"GetRef - {key}: {navigationProperty}");
        }

        /* Conventional routing builds the following two routing templates:
POST,PUT ~/v1/Organizations({ key})/{navigationProperty }/$ref
POST,PUT ~/v1/Organizations/{key }/{navigationProperty}/$ref
        */
        [HttpPost]
        [HttpPut]
        public IActionResult CreateRef(int key, string navigationProperty)
        {
            return Ok($"CreateRef - {key}: {navigationProperty}");
        }

        /* Conventional routing builds the following two routing templates:
DELETE ~/v1/Organizations({key})/{navigationProperty}/$ref
DELETE ~/v1/Organizations/{key}/{navigationProperty}/$ref
        */
        [HttpDelete]
        public IActionResult DeleteRef(int key, string navigationProperty)
        {
            return Ok($"DeleteRef - {key}: {navigationProperty}");
        }

        /* Conventional routing builds the following two routing templates:
DELETE ~/v1/Organizations({key})/{navigationProperty}({relatedKey})/$ref
DELETE ~/v1/Organizations({key})/{navigationProperty}/{relatedKey}/$ref
DELETE ~/v1/Organizations/{key}/{navigationProperty}({relatedKey})/$ref
DELETE ~/v1/Organizations/{key}/{navigationProperty}/{relatedKey}/$ref
        */
        [HttpDelete]
        public IActionResult DeleteRef(int key, int relatedKey, string navigationProperty)
        {
            return Ok($"DeleteRef - {key} - {relatedKey}: {navigationProperty}");
        }
    }
}
