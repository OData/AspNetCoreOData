// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Scenarios
{
    // Controller
    public class UntypedSimpleOpenCustomersController : ODataController
    {
        private static EdmEntityObjectCollection _untypedSimpleOpenCustormers;

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(GetCustomers());
        }

        public IActionResult Get(int key)
        {
            return Ok(GetCustomers()[0]);
        }

        [EnableQuery]
        public IActionResult GetDeclaredAddresses(int key)
        {
            object addresses;
            GetCustomers()[0].TryGetPropertyValue("DeclaredAddresses", out addresses);
            return Ok((EdmComplexObjectCollection)addresses);
        }


        [EnableQuery]
        public IActionResult GetColor(int key)
        {
            object color;
            GetCustomers()[0].TryGetPropertyValue("Color", out color);
            return Ok((EdmEnumObject)color);
        }

        [EnableQuery]
        public IActionResult GetDeclaredColors(int key)
        {
            object colors;
            GetCustomers()[0].TryGetPropertyValue("DeclaredColors", out colors);
            return Ok((EdmEnumObjectCollection)colors);
        }

        [EnableQuery]
        public IActionResult GetDeclaredNumbers(int key)
        {
            object numbers;
            GetCustomers()[0].TryGetPropertyValue("DeclaredNumbers", out numbers);
            return Ok(numbers as int[]);
        }

        [HttpPost]
        public IActionResult AddColor(int key, ODataUntypedActionParameters parameters)
        {
            Assert.Equal("0", ((EdmEnumObject)parameters["Color"]).Value);
            return Ok();
        }

        public IActionResult PostUntypedSimpleOpenCustomer(EdmEntityObject customer)
        {
            // Verify there is a string dynamic property in OpenEntityType
            object nameValue;
            customer.TryGetPropertyValue("Name", out nameValue);
            Type nameType;
            customer.TryGetPropertyType("Name", out nameType);

            Assert.NotNull(nameValue);
            Assert.Equal(typeof(String), nameType);
            Assert.Equal("FirstName 6", nameValue);

            // Verify there is a collection of double dynamic property in OpenEntityType
            object doubleListValue;
            customer.TryGetPropertyValue("DoubleList", out doubleListValue);
            Type doubleListType;
            customer.TryGetPropertyType("DoubleList", out doubleListType);

            Assert.NotNull(doubleListValue);
            Assert.Equal(typeof(List<Double>), doubleListType);

            // Verify there is a collection of complex type dynamic property in OpenEntityType
            object addressesValue;
            customer.TryGetPropertyValue("Addresses", out addressesValue);

            Assert.NotNull(addressesValue);

            // Verify there is a complex type dynamic property in OpenEntityType
            object addressValue;
            customer.TryGetPropertyValue("Address", out addressValue);

            Type addressType;
            customer.TryGetPropertyType("Address", out addressType);

            Assert.NotNull(addressValue);
            Assert.Equal(typeof(EdmComplexObject), addressType);

            // Verify there is a collection of enum type dynamic property in OpenEntityType
            object favoriteColorsValue;
            customer.TryGetPropertyValue("FavoriteColors", out favoriteColorsValue);
            EdmEnumObjectCollection favoriteColors = favoriteColorsValue as EdmEnumObjectCollection;

            Assert.NotNull(favoriteColorsValue);
            Assert.NotNull(favoriteColors);
            Assert.Equal(typeof(EdmEnumObject), favoriteColors[0].GetType());

            // Verify there is an enum type dynamic property in OpenEntityType
            object favoriteColorValue;
            customer.TryGetPropertyValue("FavoriteColor", out favoriteColorValue);

            Assert.NotNull(favoriteColorValue);
            Assert.Equal("Red", ((EdmEnumObject)favoriteColorValue).Value);

            Type favoriteColorType;
            customer.TryGetPropertyType("FavoriteColor", out favoriteColorType);

            Assert.Equal(typeof(EdmEnumObject), favoriteColorType);

            // Verify there is a string dynamic property in OpenComplexType
            EdmComplexObject address = addressValue as EdmComplexObject;
            object cityValue;
            address.TryGetPropertyValue("City", out cityValue);
            Type cityType;
            address.TryGetPropertyType("City", out cityType);

            Assert.NotNull(cityValue);
            Assert.Equal(typeof(String), cityType);
            Assert.Equal("City 6", cityValue); // It reads as ODataUntypedValue, and the RawValue is the string with the ""

            return Ok(customer);
        }

        private static EdmEntityObjectCollection GetCustomers()
        {
            if (_untypedSimpleOpenCustormers != null)
            {
                return _untypedSimpleOpenCustormers;
            }

            IEdmModel edmModel = OpenEntityTypeTests.GetUntypedEdmModel();
            IEdmEntityType customerType = edmModel.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "UntypedSimpleOpenCustomer");
            EdmEntityObject customer = new EdmEntityObject(customerType);
            customer.TrySetPropertyValue("CustomerId", 1);

            //Add Numbers primitive collection property
            customer.TrySetPropertyValue("DeclaredNumbers", new[] { 1, 2 });

            //Add Color, Colors enum(collection) property
            IEdmEnumType colorType = edmModel.SchemaElements.OfType<IEdmEnumType>().First(c => c.Name == "Color");
            EdmEnumObject color = new EdmEnumObject(colorType, "Red");
            EdmEnumObject color2 = new EdmEnumObject(colorType, "0");
            EdmEnumObject color3 = new EdmEnumObject(colorType, "Red");
            customer.TrySetPropertyValue("Color", color);

            List<IEdmEnumObject> colorList = new List<IEdmEnumObject>();
            colorList.Add(color);
            colorList.Add(color2);
            colorList.Add(color3);
            IEdmCollectionTypeReference enumCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(colorType.ToEdmTypeReference(false)));
            EdmEnumObjectCollection colors = new EdmEnumObjectCollection(enumCollectionType, colorList);
            customer.TrySetPropertyValue("Colors", colors);
            customer.TrySetPropertyValue("DeclaredColors", colors);

            //Add Addresses complex(collection) property
            EdmComplexType addressType =
                edmModel.SchemaElements.OfType<IEdmComplexType>().First(c => c.Name == "Address") as EdmComplexType;
            EdmComplexObject address = new EdmComplexObject(addressType);
            address.TrySetPropertyValue("Street", "No1");
            EdmComplexObject address2 = new EdmComplexObject(addressType);
            address2.TrySetPropertyValue("Street", "No2");

            List<IEdmComplexObject> addressList = new List<IEdmComplexObject>();
            addressList.Add(address);
            addressList.Add(address2);
            IEdmCollectionTypeReference complexCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(addressType.ToEdmTypeReference(false)));
            EdmComplexObjectCollection addresses = new EdmComplexObjectCollection(complexCollectionType, addressList);
            customer.TrySetPropertyValue("DeclaredAddresses", addresses);

            EdmEntityObjectCollection customers = new EdmEntityObjectCollection(new EdmCollectionTypeReference(new EdmCollectionType(customerType.ToEdmTypeReference(false))));
            customers.Add(customer);
            _untypedSimpleOpenCustormers = customers;
            return _untypedSimpleOpenCustormers;
        }
    }
}