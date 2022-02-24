//-----------------------------------------------------------------------------
// <copyright file="CustomersController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataAlternateKeySample.Models;

namespace ODataAlternateKeySample.Controllers
{
    public class CustomersController : ODataController
    {
        private readonly IAlternateKeyRepository _repository;

        public CustomersController(IAlternateKeyRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_repository.GetCustomers());
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get(int key)
        {
            var c = _repository.GetCustomers().FirstOrDefault(c => c.Id == key);
            if (c == null)
            {
                return NotFound();
            }

            return Ok(c);
        }

        // Alternate key: SSN
        [HttpGet("odata/Customers(SSN={ssn})")]
        public IActionResult GetCustomerBySSN(string ssn)
        {
            var c = _repository.GetCustomers().FirstOrDefault(c => c.SSN == ssn);
            if (c == null)
            {
                return NotFound();
            }

            return Ok(c);
        }

        [HttpPatch("odata/Customers(SSN={ssnKey})")]
        public IActionResult PatchCustomerBySSN(string ssnKey, Delta<Customer> delta)
        {
            var originalCustomer = _repository.GetCustomers().FirstOrDefault(c => c.SSN == ssnKey);
            if (originalCustomer == null)
            {
                return NotFound();
            }

            delta.Patch(originalCustomer);
            return Updated(originalCustomer);
        }
    }
}
