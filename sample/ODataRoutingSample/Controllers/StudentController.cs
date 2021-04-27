// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using ODataRoutingSample.Models;

namespace ODataRoutingSample.Controllers
{
    public class StudentController : ControllerBase
    {
        private static IList<Student> _students = new List<Student>
        {
            new Student
            {
                Id = "abc",
                Name = "Zhangg",
                Score = 8,
                Test = new Dictionary<string, object>
                {
                    { "DName", "abc" }
                }
            },
            new Student
            {
                Id = "efg",
                Name = "Jingchan",
                Score = 8,
                Test = new Dictionary<string, object>
                {
                    { "DName", "efg" }
                }
            },
            new Student
            {
                Id = "xyz",
                Name = "Hollewye",
                Score = 8,
                Test = new Dictionary<string, object>
                {
                    { "DName", "xzg" }
                }
            },
        };

        [HttpGet]
        [EnableQuery]
        public IActionResult Get(CancellationToken token)
        {
            return Ok(_students);
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get(string key)
        {
            var student = _students.FirstOrDefault(p => p.Id == key);
            if (student == null)
            {
                return NotFound($"Not found student with Id = {key}");
            }

            return Ok(student);
        }
    }
}
