// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Typeless
{
    public class TypelessCustomersController : ODataController
    {
        private static IEdmEntityObject postedCustomer = null;

        public IEdmEntityType CustomerType
        {
            get
            {
                return Request.GetModel().FindType("Microsoft.AspNetCore.OData.E2E.Tests.Typeless.TypelessCustomer") as IEdmEntityType;
            }
        }

        public IEdmEntityType OrderType
        {
            get
            {
                return Request.GetModel().FindType("Microsoft.AspNetCore.OData.E2E.Tests.Typeless.TypelessOrder") as IEdmEntityType;
            }
        }

        public IEdmComplexType AddressType
        {
            get
            {
                return Request.GetModel().FindType("Microsoft.AspNetCore.OData.E2E.Tests.Typeless.TypelessAddress") as IEdmComplexType;
            }
        }

        public IActionResult Get()
        {
            IEdmEntityObject[] typelessCustomers = new EdmEntityObject[20];
            for (int i = 0; i < 20; i++)
            {
                dynamic typelessCustomer = new EdmEntityObject(CustomerType);
                typelessCustomer.Id = i;
                typelessCustomer.Name = string.Format("Name {0}", i);
                typelessCustomer.Orders = CreateOrders(i);
                typelessCustomer.Addresses = CreateAddresses(i);
                typelessCustomer.FavoriteNumbers = Enumerable.Range(0, i).ToArray();
                typelessCustomers[i] = typelessCustomer;
            }

            IEdmCollectionTypeReference entityCollectionType =
                new EdmCollectionTypeReference(
                    new EdmCollectionType(
                        new EdmEntityTypeReference(CustomerType, isNullable: false)));

            return Ok(new EdmEntityObjectCollection(entityCollectionType, typelessCustomers.ToList()));
        }

        public IActionResult Get([FromODataUri] int key)
        {
            object id;
            if (postedCustomer == null || !postedCustomer.TryGetPropertyValue("Id", out id) || key != (int)id)
            {
                return BadRequest("The key isn't the one posted to the customer");
            }

            ODataQueryContext context = new ODataQueryContext(Request.GetModel(), CustomerType, path: null);
            ODataQueryOptions query = new ODataQueryOptions(context, Request);
            if (query.SelectExpand != null)
            {
                Request.ODataFeature().SelectExpandClause = query.SelectExpand.SelectExpandClause;
            }

            return Ok(postedCustomer);
        }

        public IActionResult Post([FromBody]IEdmEntityObject customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("customer is null");
            }
            postedCustomer = customer;
            object id;
            customer.TryGetPropertyValue("Id", out id);

            IEdmEntitySet entitySet = Request.GetModel().EntityContainer.FindEntitySet("TypelessCustomers");
            return Created(Request.CreateODataLink(new EntitySetSegment(entitySet),
                new KeySegment(new[] { new KeyValuePair<string, object>("Id", id) }, entitySet.EntityType(), null)), customer);
        }

        [HttpPost]
        public IActionResult PrimitiveCollection()
        {
            return Ok(Enumerable.Range(1, 10));
        }

        [HttpPost]
        public IActionResult ComplexObjectCollection()
        {
            return Ok(CreateAddresses(10));
        }

        [HttpPost]
        public IActionResult EntityCollection()
        {
            return Ok(CreateOrders(10));
        }

        [HttpPost]
        public IActionResult SinglePrimitive()
        {
            return Ok(10);
        }

        [HttpPost]
        public IActionResult SingleComplexObject()
        {
            return Ok(CreateAddress(10));
        }

        [HttpPost]
        public IActionResult SingleEntity()
        {
            return Ok(CreateOrder(10));
        }

        public IActionResult EnumerableOfIEdmObject()
        {
            IList<IEdmEntityObject> result = Enumerable.Range(0, 10).Select(i => (IEdmEntityObject)CreateOrder(i)).ToList();
            return Ok(result);
        }

        [HttpPost]
        public IActionResult TypelessParameters(ODataUntypedActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("parameters is null");
            }
            object address;
            object addresses;
            object value;
            object values;
            if (!parameters.TryGetValue("address", out address) || address as IEdmComplexObject == null ||
                !parameters.TryGetValue("addresses", out addresses) || addresses as IEnumerable == null ||
                !parameters.TryGetValue("value", out value) || (int)value != 5 ||
                !parameters.TryGetValue("values", out values) || values as IEnumerable == null ||
                !(values as IEnumerable).Cast<int>().SequenceEqual(Enumerable.Range(0, 5)))
            {
                return BadRequest("Address is not present or is not a complex object");
            }
            return Ok(address as IEdmComplexObject);
        }

        private dynamic CreateAddresses(int i)
        {
            EdmComplexObject[] addresses = new EdmComplexObject[i];
            for (int j = 0; j < i; j++)
            {
                dynamic complexObject = CreateAddress(j);
                addresses[j] = complexObject;
            }
            var collection = new EdmComplexObjectCollection(new EdmCollectionTypeReference(new EdmCollectionType(new EdmComplexTypeReference(AddressType, false))), addresses);
            return collection;
        }

        private dynamic CreateOrders(int i)
        {
            EdmEntityObject[] orders = new EdmEntityObject[i];
            for (int j = 0; j < i; j++)
            {
                dynamic order = new EdmEntityObject(OrderType);
                order.Id = j;
                order.ShippingAddress = CreateAddress(j);
                orders[j] = order;
            }
            var collection = new EdmEntityObjectCollection(new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(OrderType, false))), orders);
            return collection;
        }

        private dynamic CreateOrder(int j)
        {
            dynamic order = new EdmEntityObject(OrderType);
            order.Id = j;
            order.ShippingAddress = CreateAddress(j);
            return order;
        }

        private dynamic CreateAddress(int j)
        {
            dynamic address = new EdmComplexObject(AddressType);
            address.FirstLine = "First line " + j;
            address.SecondLine = "Second line " + j;
            address.ZipCode = j;
            address.City = "City " + j;
            address.State = "State " + j;
            return address;
        }
    }
}
