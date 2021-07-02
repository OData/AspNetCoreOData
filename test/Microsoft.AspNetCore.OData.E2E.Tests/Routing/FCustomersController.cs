// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Routing
{
    public class FCustomersController : ODataController
    {
        [EnableQuery]
        public IActionResult Get()
        {
            IEdmModel model = Request.GetModel();
            IEdmEntityType customerType = model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "Customer");

            EdmEntityObject customer = new EdmEntityObject(customerType);

            customer.TrySetPropertyValue("Id", 1);
            customer.TrySetPropertyValue("Tony", 1);

            EdmEntityObjectCollection customers =
                new EdmEntityObjectCollection(
                    new EdmCollectionTypeReference(new EdmCollectionType(customerType.ToEdmTypeReference(false))));
            customers.Add(customer);
            return Ok(customers);
        }

        #region Bound Function using attribute routing

        [HttpGet("attribute/FCustomers({key})/NS.IntCollectionFunction(intValues={intValues})")]
        public bool IntCollectionFunctionOnAttriubte(int key, [FromODataUri] IEnumerable<int?> intValues)
        {
            return IntCollectionFunction(key, intValues);
        }

        [HttpGet("attribute/FCustomers({key})/NS.EnumFunction(color={color})")]
        public bool EnumFunctionOnAttribute(int key, [FromODataUri] EdmEnumObject color)
        {
            return EnumFunction(key, color);
        }

        [HttpGet("attribute/FCustomers({key})/NS.EnumCollectionFunction(colors={colors})")]
        public bool EnumCollectionFunctionOnAttribute(int key, [FromODataUri] EdmEnumObjectCollection colors)
        {
            return EnumCollectionFunction(key, colors);
        }

        [HttpGet("attribute/FCustomers({key})/NS.ComplexFunction(address={address})")]
        public bool ComplexFunctionOnAttribute(int key, [FromODataUri] EdmComplexObject address)
        {
            return ComplexFunction(key, address);
        }

        [HttpGet("attribute/FCustomers({key})/NS.ComplexCollectionFunction(addresses={addresses})")]
        public bool ComplexCollectionFunctionOnAttribute(int key, [FromODataUri] EdmComplexObjectCollection addresses)
        {
            return ComplexCollectionFunction(key, addresses);
        }

        [HttpGet("attribute/FCustomers({key})/NS.EntityFunction(customer={customer})")]
        public bool EntityFunctionOnAttribute(int key, [FromODataUri] EdmEntityObject customer)
        {
            return EntityFunction(key, customer);
        }

        [HttpGet("attribute/FCustomers({key})/NS.CollectionEntityFunction(customers={customers})")]
        public bool CollectionEntityFunctionOnAttribute(int key, [FromODataUri] EdmEntityObjectCollection customers)
        {
            return CollectionEntityFunction(key, customers);
        }

        #endregion

        #region Bound function using convention routing & Unbound function using Attribute routing

        // Here's the note:
        // [HttpGet] & [ODataModel] will create an odata convention routing for this method.
        // [HttpGet("odata/....")] will create an attribute routing.
        [HttpGet]
        [ODataModel("odata")]
        [HttpGet("odata/UnboundIntCollectionFunction(key={key},intValues={intValues})")]
        public bool IntCollectionFunction(int key, [FromODataUri] IEnumerable<int?> intValues)
        {
            Assert.NotNull(intValues);

            IList<int?> values = intValues.ToList();
            Assert.Equal(1, values[0]);
            Assert.Equal(2, values[1]);
            Assert.Null(values[2]);
            Assert.Equal(7, values[3]);
            Assert.Equal(8, values[4]);

            return true;
        }

        [HttpGet]
        [ODataModel("odata")]
        [HttpGet("odata/UnboundEnumFunction(key={key},color={color})")]
        public bool EnumFunction(int key, [FromODataUri] EdmEnumObject color)
        {
            Assert.NotNull(color);
            Assert.Equal("NS.Color", color.GetEdmType().FullName());
            Assert.Equal("0", color.Value);
            return true;
        }

        [HttpGet]
        [ODataModel("odata")]
        [HttpGet("odata/UnboundEnumCollectionFunction(key={key},colors={colors})")]
        public bool EnumCollectionFunction(int key, [FromODataUri] EdmEnumObjectCollection colors)
        {
            Assert.NotNull(colors);
            IList<IEdmEnumObject> results = colors.ToList();

            Assert.Equal(2, results.Count);

            // #1
            EdmEnumObject color = results[0] as EdmEnumObject;
            Assert.NotNull(color);
            Assert.Equal("NS.Color", color.GetEdmType().FullName());
            Assert.Equal("Red", color.Value);

            // #2
            EdmEnumObject color2 = results[1] as EdmEnumObject;
            Assert.NotNull(color2);
            Assert.Equal("NS.Color", color2.GetEdmType().FullName());
            Assert.Equal("Green", color2.Value);
            return true;
        }

        [HttpGet]
        [ODataModel("odata")]
        [HttpGet("odata/UnboundComplexFunction(key={key},address={address})")]
        public bool ComplexFunction(int key, [FromODataUri] EdmComplexObject address)
        {
            if (key == 99)
            {
                Assert.Null(address);
                return false;
            }

            Assert.NotNull(address);
            dynamic result = address;
            Assert.Equal("NS.Address", address.GetEdmType().FullName());
            Assert.Equal("NE 24th St.", result.Street);
            Assert.Equal("Redmond", result.City);
            return true;
        }

        [HttpGet]
        [ODataModel("odata")]
        [HttpGet("odata/UnboundComplexCollectionFunction(key={key},addresses={addresses})")]
        public bool ComplexCollectionFunction(int key, [FromODataUri] EdmComplexObjectCollection addresses)
        {
            Assert.NotNull(addresses);
            IList<IEdmComplexObject> results = addresses.ToList();

            Assert.Equal(2, results.Count);

            // #1
            EdmComplexObject complex = results[0] as EdmComplexObject;
            Assert.Equal("NS.Address", complex.GetEdmType().FullName());

            dynamic address = results[0];
            Assert.NotNull(address);
            Assert.Equal("NE 24th St.", address.Street);
            Assert.Equal("Redmond", address.City);

            // #2
            complex = results[1] as EdmComplexObject;
            Assert.Equal("NS.SubAddress", complex.GetEdmType().FullName());

            address = results[1];
            Assert.NotNull(address);
            Assert.Equal("LianHua Rd.", address.Street);
            Assert.Equal("Shanghai", address.City);
            Assert.Equal(9.9, address.Code);
            return true;
        }

        [HttpGet]
        [ODataModel("odata")]
        [HttpGet("odata/UnboundEntityFunction(key={key},customer={customer})")]
        public bool EntityFunction(int key, [FromODataUri] EdmEntityObject customer)
        {
            Assert.NotNull(customer);
            dynamic result = customer;
            Assert.Equal("NS.Customer", customer.GetEdmType().FullName());

            // entity call
            if (key == 9)
            {
                Assert.Equal(91, result.Id);
                Assert.Equal("John", result.Name);

                dynamic address = result.Location;
                EdmComplexObject addressObj = Assert.IsType<EdmComplexObject>(address);
                Assert.Equal("NS.Address", addressObj.GetEdmType().FullName());
                Assert.Equal("NE 24th St.", address.Street);
                Assert.Equal("Redmond", address.City);
            }
            else
            {
                // entity reference call
                Assert.Equal(8, result.Id);
                Assert.Equal("Id", String.Join(",", customer.GetChangedPropertyNames()));

                Assert.Equal("Name,Location", String.Join(",", customer.GetUnchangedPropertyNames()));
            }

            return true;
        }

        [HttpGet]
        [ODataModel("odata")]
        [HttpGet("odata/UnboundCollectionEntityFunction(key={key},customers={customers})")]
        public bool CollectionEntityFunction(int key, [FromODataUri] EdmEntityObjectCollection customers)
        {
            Assert.NotNull(customers);
            IList<IEdmEntityObject> results = customers.ToList();
            Assert.Equal(2, results.Count);

            // entities call
            if (key == 9)
            {
                // #1
                EdmEntityObject entity = results[0] as EdmEntityObject;
                Assert.NotNull(entity);
                Assert.Equal("NS.Customer", entity.GetEdmType().FullName());

                dynamic customer = results[0];
                Assert.Equal(91, customer.Id);
                Assert.Equal("John", customer.Name);

                dynamic address = customer.Location;
                EdmComplexObject addressObj = Assert.IsType<EdmComplexObject>(address);
                Assert.Equal("NS.Address", addressObj.GetEdmType().FullName());
                Assert.Equal("NE 24th St.", address.Street);
                Assert.Equal("Redmond", address.City);

                // #2
                entity = results[1] as EdmEntityObject;
                Assert.Equal("NS.SpecialCustomer", entity.GetEdmType().FullName());

                customer = results[1];
                Assert.Equal(92, customer.Id);
                Assert.Equal("Mike", customer.Name);

                address = customer.Location;
                addressObj = Assert.IsType<EdmComplexObject>(address);
                Assert.Equal("NS.SubAddress", addressObj.GetEdmType().FullName());
                Assert.Equal("LianHua Rd.", address.Street);
                Assert.Equal("Shanghai", address.City);
                Assert.Equal(9.9, address.Code);

                Assert.Equal(new Guid("883F50C5-F554-4C49-98EA-F7CACB41658C"), customer.Title);
            }
            else
            {
                // entity references call
                int id = 81;
                foreach (IEdmEntityObject edmObj in results)
                {
                    EdmEntityObject entity = edmObj as EdmEntityObject;
                    Assert.NotNull(entity);
                    Assert.Equal("NS.Customer", entity.GetEdmType().FullName());

                    dynamic customer = entity;
                    Assert.Equal(id++, customer.Id);
                    Assert.Equal("Id", String.Join(",", customer.GetChangedPropertyNames()));
                    Assert.Equal("Name,Location", String.Join(",", customer.GetUnchangedPropertyNames()));
                }
            }

            return true;
        }

        #endregion

    }

}