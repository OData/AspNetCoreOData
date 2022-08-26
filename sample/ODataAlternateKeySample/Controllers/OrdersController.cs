//-----------------------------------------------------------------------------
// <copyright file="OrdersController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataAlternateKeySample.Models;

namespace ODataAlternateKeySample.Controllers
{
    public class OrdersController : ODataController
    {
        private readonly IAlternateKeyRepository _repository;

        public OrdersController(IAlternateKeyRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_repository.GetOrders());
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get(int key)
        {
            var c = _repository.GetOrders().FirstOrDefault(c => c.Id == key);
            if (c == null)
            {
                return NotFound();
            }

            return Ok(c);
        }

        // alternate key: Name
        [HttpGet("odata/Orders(Name={orderName})")] // use community alternate key
        [HttpGet("odata/Orders(CoreName={orderName})")] // use core alternate key
        public IActionResult GetOrderByName(string orderName)
        {
            var c = _repository.GetOrders().FirstOrDefault(c => c.Name == orderName);
            if (c == null)
            {
                return NotFound();
            }

            return Ok(c);
        }

        // alternate key: Token
        [HttpGet("odata/Orders(Token={token})")] // use community alternate key
        [HttpGet("odata/Orders(CoreToken={token})")] // use core alternate key
        public IActionResult GetOrderByToken(Guid token)
        {
            var c = _repository.GetOrders().FirstOrDefault(c => c.Token == token);
            if (c == null)
            {
                return NotFound();
            }

            return Ok(c);
        }
    }
}
