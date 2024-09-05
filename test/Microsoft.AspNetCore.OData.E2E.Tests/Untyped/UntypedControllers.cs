//-----------------------------------------------------------------------------
// <copyright file="UntypedController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Untyped;

public class UntypedController : ODataController
{
    [EnableQuery]
    [HttpGet("/odata/managers")]
    public IActionResult GetManagers()
    {
        return Ok(UntypedDataSource.Managers);
    }

    [EnableQuery]
    [HttpGet("odata/people")]
    public IActionResult GetPeople()
    {
        return Ok(UntypedDataSource.GetAllPeople());
    }

    [EnableQuery]
    [HttpGet("odata/people/{id}")]
    public IActionResult Get(int id)
    {
        InModelPerson person = UntypedDataSource.GetAllPeople().FirstOrDefault(p => p.Id == id);
        return Ok(person);
    }

    [EnableQuery]
    [HttpGet("odata/people/{id}/data")]
    public IActionResult GetData(int id)
    {
        InModelPerson person = UntypedDataSource.GetAllPeople().FirstOrDefault(p => p.Id == id);
        return Ok(person.Data);
    }

    [EnableQuery]
    [HttpGet("odata/people/{id}/infos")]
    public IActionResult GetInfos(int id)
    {
        InModelPerson person = UntypedDataSource.GetAllPeople().FirstOrDefault(p => p.Id == id);
        return Ok(person.Infos);
    }

    [EnableQuery]
    [HttpPost("odata/people")]
    public IActionResult Post([FromBody] InModelPerson person)
    {
        Assert.NotNull(person);

        if (person.Id == 90) // primitive
        {
            Assert.IsType<Guid>(person.Data);
            Assert.Equal(new Guid("40EE4E85-C443-41B2-9611-C55F97D80E84"), person.Data);
            return Created(person);
        }

        if (person.Id == 91) // enum
        {
            Assert.Equal(InModelColor.Blue, person.Data);
            return Created(person);
        }

        if (person.Id == 92) // resource
        {
            InModelAddress address = Assert.IsType<InModelAddress>(person.Data);
            Assert.Equal("Redmond", address.City);
            Assert.Equal("156TH AVE", address.Street);
            return Created(person);
        }

        if (person.Id == 93) // collection of primitive
        {
            IEnumerable<int> enumerable = person.Data as IEnumerable<int>;
            Assert.Collection(enumerable,
                e => Assert.Equal(4, e),
                e => Assert.Equal(5, e));
            return Created(person);
        }

        if (person.Id == 94) // collection of mix
        {
            EdmUntypedCollection untypedcoll = Assert.IsType<EdmUntypedCollection>(person.Data);
            Assert.Equal(2, untypedcoll.Count);
            Assert.Equal(4, untypedcoll.ElementAt(0));
            InModelAddress address = Assert.IsType<InModelAddress>(untypedcoll.ElementAt(1));
            Assert.Equal("Earth", address.City);
            Assert.Equal("Min AVE", address.Street);
            return Created(person);
        }

        if (person.Id == 98)
        {
            // 98 is special created
            Assert.Equal("Sam", person.Name);

            // Data
            EdmUntypedObject data = Assert.IsType<EdmUntypedObject>(person.Data);
            Assert.Equal(3, data.Count); // three properties
            Assert.Equal("LineString", data["type"]);
            EdmUntypedCollection coordinates = Assert.IsType<EdmUntypedCollection>(data["coordinates"]);
            Assert.Equal(4, coordinates.Count);

            EdmUntypedObject crs = Assert.IsType<EdmUntypedObject>(data["crs"]);

            // Infos
            Assert.Equal(2, person.Infos.Count);

            string personJson = JsonSerializer.Serialize(person);

            Assert.Equal("{\"Id\":98,\"Name\":\"Sam\",\"Data\":{\"type\":\"LineString\",\"coordinates\":[[1.0,1.0],[3.0,3.0],[4.0,4.0],[0.0,0.0]],\"crs\":{\"type\":\"name\",\"properties\":{\"name\":\"EPSG:4326\"}}},\"Infos\":[[42],{\"k1\":\"abc\",\"k2\":42,\"k:3\":{\"a1\":2,\"b2\":null},\"k/4\":[null,42]}],\"Containers\":{\"dynamic_p\":[null,{\"X1\":\"Red\",\"Data\":{\"D1\":42}},\"finance\",\"hr\",\"legal\",43]}}", personJson);
            return Created(person);
        }

        return Ok(true);
    }

    [EnableQuery]
    public IActionResult Patch(int key, Delta<InModelPerson> delta)
    {
        Assert.Equal(2, key);

        return Ok(false);
    }
}
