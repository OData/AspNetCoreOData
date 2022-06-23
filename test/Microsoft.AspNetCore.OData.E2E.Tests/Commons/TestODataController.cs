//-----------------------------------------------------------------------------
// <copyright file="TestODataController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Commons
{
    public class TestODataController : ODataController
    {
        [NonAction]
        public new TestOkResult Ok() => new TestOkResult(base.Ok());

        [NonAction]
        public new TestOkObjectResult Ok(object value) => new TestOkObjectResult(value);

        [NonAction]
        public TestCreatedODataResult<T> Created<T>(T entity) => new TestCreatedODataResult<T>(entity);

        [NonAction]
        public new TestCreatedResult Created(string uri, object entity) => new TestCreatedResult(base.Created(uri, entity));

        [NonAction]
        public TestUpdatedODataResult<T> Updated<T>(T entity) => new TestUpdatedODataResult<T>(entity);

    }

    /// <summary>
    /// Wrapper for BadRequestResult
    /// </summary>
    public class TestBadRequestResult : TestActionResult
    {
        public TestBadRequestResult(BadRequestResult innerResult)
            : base(innerResult)
        {
        }
    }

    /// <summary>
    /// Wrapper for platform-specific version of object result.
    /// </summary>
    public class TestObjectResult : ObjectResult, IActionResult
    {
        public TestObjectResult(object innerResult)
            : base(innerResult)
        {
        }
    }

    /// <summary>
    /// Wrapper for platform-specific version of status code result.
    /// </summary>
    public class TestStatusCodeResult : StatusCodeResult, IActionResult
    {
        private StatusCodeResult innerResult;

        public TestStatusCodeResult(StatusCodeResult innerResult)
            : base(innerResult.StatusCode)
        {
            this.innerResult = innerResult;
        }
    }

    /// <summary>
    /// Wrapper for platform-specific version of action result.
    /// </summary>
    public class TestActionResult : ActionResult, IActionResult
    {
        private IActionResult innerResult;

        public TestActionResult(IActionResult innerResult)
        {
            this.innerResult = innerResult;
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            return innerResult.ExecuteResultAsync(context);
        }
    }

    public class TestNotFoundResult : TestStatusCodeResult
    {
        public TestNotFoundResult(NotFoundResult innerResult)
            : base(innerResult)
        {
        }
    }

    public class TestNotFoundObjectResult : TestObjectResult
    {
        public TestNotFoundObjectResult(NotFoundObjectResult innerResult)
            : base(innerResult)
        {
        }
    }

    public class TestOkResult : TestStatusCodeResult
    {
        public TestOkResult(OkResult innerResult)
            : base(innerResult)
        {
        }
    }

    public class TestOkObjectResult : TestObjectResult
    {
        public TestOkObjectResult(object innerResult)
            : base(innerResult)
        {
            this.StatusCode = 200;
        }
    }

    public class TestOkObjectResult<T> : TestObjectResult
    {
        public TestOkObjectResult(object innerResult)
            : base(innerResult)
        {
            this.StatusCode = 200;
        }

        public TestOkObjectResult(T content, TestODataController controller)
            : base(content)
        {
            // Controller is unused.
            this.StatusCode = 200;
        }
    }

    public class TestCreatedResult : TestActionResult
    {
        public TestCreatedResult(CreatedResult innerResult)
            : base(innerResult)
        {
        }
    }

    public class TestCreatedODataResult<T> : CreatedODataResult<T>, IActionResult
    {
        public TestCreatedODataResult(T entity)
            : base(entity)
        {
        }

        public TestCreatedODataResult(string uri, T entity)
            : base(entity)
        {
        }
    }

    public class TestUpdatedODataResult<T> : UpdatedODataResult<T>, IActionResult
    {
        public TestUpdatedODataResult(T entity)
            : base(entity)
        {
        }

        public TestUpdatedODataResult(string uri, T entity)
            : base(entity)
        {
        }
    }
}
