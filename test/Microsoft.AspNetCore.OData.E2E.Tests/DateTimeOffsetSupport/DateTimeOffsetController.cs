//-----------------------------------------------------------------------------
// <copyright file="DateTimeOffsetController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DateTimeOffsetSupport
{
    public class FilesController : ODataController
    {
        private readonly FilesContext _db;

         public FilesController(FilesContext context)
        {
            context.Database.EnsureCreated();
            if (!context.Files.Any())
            {
                foreach (var file in CreateFiles())
                {
                    context.Files.Add(file);
                }

                context.SaveChanges();
            }

            _db = context;
        }

        [EnableQuery]
        public IQueryable<File> Get()
        {
            return _db.Files;
        }

        [EnableQuery]
        public IActionResult Get(int key)
        {
            File file = _db.Files.FirstOrDefault(c => c.FileId == key);
            if (file == null)
            {
                return NotFound();
            }

            return Ok(file);
        }

        [HttpPost]
        public IActionResult Post([FromBody]File file)
        {
            _db.Files.Add(file);
            _db.SaveChanges();

            return Created(file);
        }

        public IActionResult Patch(int key, Delta<File> patch)
        {
            var file = _db.Files.SingleOrDefault(c => c.FileId == key);
            if (file == null)
            {
                return NotFound();
            }

            patch.Patch(file);
            _db.SaveChanges();

            return Updated(file);
        }

        public IActionResult Delete(int key)
        {
            File original = _db.Files.FirstOrDefault(c => c.FileId == key);
            if (original == null)
            {
                return NotFound();
            }

            _db.Files.Remove(original);
            _db.SaveChanges();

            return StatusCode(StatusCodes.Status204NoContent);
        }

        public IActionResult GetCreatedDate(int key)
        {
            File file = _db.Files.FirstOrDefault(c => c.FileId == key);
            if (file == null)
            {
                return NotFound();
            }

            return Ok(file.CreatedDate);
        }

        [HttpPost("/ResetDataSource")]
        public IActionResult ResetDataSource()
        {
            _db.Files.RemoveRange(_db.Files);
            _db.SaveChanges();

            _db.Database.EnsureDeleted();
            _db.Database.EnsureCreated();

            var files = CreateFiles();
            _db.Files.AddRange(files);
            _db.SaveChanges();

            return StatusCode(StatusCodes.Status204NoContent);
        }

        public static IEnumerable<File> CreateFiles()
        {
            DateTimeOffset dateTime = new DateTimeOffset(2021, 4, 15, 16, 24, 8, TimeSpan.FromHours(-8));

            // #2 is used for update/get round trip, its value will be changed every test running.
            // #3 is used for get, will never change its value.
            // #4 is used  to select date time property, will never change its value.
            // #6 is used for create/get round trip, it will create, get, delete
            return Enumerable.Range(1, 5).Select(e =>
                new File
                {
                    // FileId = e,
                    Name = "File #" + e,
                    CreatedDate = dateTime.AddMonths(3 - e).AddYears(e % 2 == 0 ? e : -e),
                    DeleteDate = dateTime.AddMonths(e % 2 == 0 ? e : -e),
                });
        }
    }
}
