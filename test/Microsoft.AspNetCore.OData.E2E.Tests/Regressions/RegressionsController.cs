//-----------------------------------------------------------------------------
// <copyright file="RegressionsController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Regressions
{
    public class UsersController : Controller
    {
        private readonly RegressionsDbContext _dbContext;

        public UsersController(RegressionsDbContext dbContext)
        {
            dbContext.Database.EnsureCreated();
            _dbContext = dbContext;
            if (!_dbContext.Users.Any())
            {
                _dbContext.DataFiles.Add(new DataFile
                {
                    FileId = 1,
                    FileName = "76x473626.pdf"
                });

                DataFile dataFile2 = new DataFile
                {
                    FileId = 2,
                    FileName = "uyr65euit5.pdf"
                };
                _dbContext.DataFiles.Add(dataFile2);

                _dbContext.DataFiles.Add(new DataFile
                {
                    FileId = 3,
                    FileName = "hj7x87643.pdf"
                });

                _dbContext.Users.Add(new User
                {
                    UserId = 1,
                    Name = "Alex",
                    Age = 35,
                    DataFileRef = null,
                    Files = null
                });

                _dbContext.Users.Add(new User
                {
                    UserId = 2,
                    Name = "Amanda",
                    Age = 29,
                    DataFileRef = 2,
                    Files = dataFile2
                });

                _dbContext.Users.Add(new User
                {
                    UserId = 3,
                    Name = "Lara",
                    Age = 25,
                    DataFileRef = null,
                    Files = null
                });

                _dbContext.SaveChanges();
            }
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<User> Get()
        {
            IQueryable<User> data = _dbContext.Users.AsQueryable();
            return data;
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get(int key)
        {
            User data = _dbContext.Users.Include(c => c.Files).FirstOrDefault(c => c.UserId == key);
            if (data == null)
            {
                return NotFound();
            }

            return Ok(data);
        }
    }
}