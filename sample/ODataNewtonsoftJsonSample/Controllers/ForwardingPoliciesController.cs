//-----------------------------------------------------------------------------
// <copyright file="OrganizationsController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Policies;
using ODataRoutingSample.Models;

namespace ODataRoutingSample.Controllers.v1
{
    [ODataRouteComponent("v1")]
    [ODataAttributeRouting]
    public class ForwardingPoliciesController : Controller
    {
        private static readonly Dictionary<string, ForwardingPolicy> policies = new Dictionary<string, ForwardingPolicy>();

        [EnableQuery]
        public IActionResult Post([FromBody] ForwardingPolicy policy)
        {
            //// TODO update this to use forwardingpolicies instead, then see if delta patch works


            var random = new Random();

            ForwardingPolicy createdPolicy;
            do
            {
                createdPolicy = new ForwardingPolicy()
                {
                    Id = random.Next().ToString(),
                };
            }
            while (!policies.TryAdd(createdPolicy.Id, createdPolicy));

            

            return Ok(createdPolicy);
        }

        [HttpGet]
        public IActionResult Get(string key)
        {
            if (!policies.TryGetValue(key, out var policy))
            {
                return NotFound();
            }

            return Ok(policy);
        }

        /*[HttpPatch]
        [EnableQuery]
        // public IActionResult Patch(EdmChangedObjectCollection changes)
        public IActionResult Patch(DeltaSet<Organization> changes)
        {
            

            //changes.ApplyDeleteLink = (l) => { };

            //IList<Organization> originalSet = new List<Organization>();

            // changes.Patch(originalSet);

            return Ok();
        }*/

        [HttpPatch]
        [EnableQuery]
        public IActionResult Patch(int key, Delta<ForwardingPolicy> delta)
        {
            /*
PATCH http://localhost:12197/v1/ForwardingPolicies/320343815
{
   "PolicyRules@delta": [
    {
        "@odata.type": "Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Rules.m365ForwardingRule",
      "Name":"Microsoft"
   }
  ]
}
            */

            // Or using the following payload (v4.0 format)


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

        /*public IActionResult GetName(int key)
        {
            if (!organizations.TryGetValue(key, out var organization))
            {
                return NotFound();
            }

            return Ok(organization.Name);
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
            
            return Ok($"MarkAsFavouriteAfterGetByAccount after {aId}");
        }

        [HttpPost("v1/Organizations/GetByAccount2(accountId={aId})/{key}/MarkAsFavourite")]
        // [HttpPost("v1/Organizations/GetByAccount2(accountId={aId})({key})/MarkAsFavourite")] this syntax has ODL problem.
        public IActionResult MarkAsFavouriteAfterGetByAccount2(int key, string aId)
        {
            
            return Ok($"MarkAsFavourite2AfterGetByAccount2 after {aId} with key={key}");
        }*/

    }
}
