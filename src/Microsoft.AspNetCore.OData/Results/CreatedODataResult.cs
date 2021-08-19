//-----------------------------------------------------------------------------
// <copyright file="CreatedODataResult.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Results
{
    /// <summary>
    /// Represents an action result that is a response to a create operation that adds an entity to an entity set.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <remarks>This action result handles content negotiation and the HTTP prefer header. It generates a location
    /// header containing the edit link of the created entity and, if response has status code: NoContent, also
    /// generates an OData-EntityId header.</remarks>
    public class CreatedODataResult<T> : ActionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreatedODataResult{T}"/> class.
        /// </summary>
        /// <param name="entity">The created entity.</param>
        public CreatedODataResult(T entity)
        {
            Entity = entity ?? throw Error.ArgumentNull(nameof(entity));
        }

        /// <summary>
        /// Gets the entity that was created.
        /// </summary>
        public virtual T Entity { get; }

        /// <inheritdoc/>
        public async override Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            HttpRequest request = context.HttpContext.Request;
            HttpResponse response = context.HttpContext.Response;
            IActionResult result = GetInnerActionResult(request);
            Uri location = GenerateLocationHeader(request);

            response.Headers["Location"] = location.AbsoluteUri;
            // Since AddEntityId relies on the response, make sure to execute the result
            // before calling AddEntityId() to ensure the response code is set correctly.
            await result.ExecuteResultAsync(context).ConfigureAwait(false);
            ResultHelpers.AddEntityId(response, () => GenerateEntityId(request));
            ResultHelpers.AddServiceVersion(response, () => ODataUtils.ODataVersionToString(request.GetODataVersion()));
        }

        // internal just for unit test.
        internal IActionResult GetInnerActionResult(HttpRequest request)
        {
            if (RequestPreferenceHelpers.RequestPrefersReturnNoContent(request.Headers))
            {
                return new StatusCodeResult((int)HttpStatusCode.NoContent);
            }
            else
            {
                ObjectResult objectResult = new ObjectResult(Entity)
                {
                    StatusCode = StatusCodes.Status201Created
                };

                return objectResult;
            }
        }

        // internal just for unit test.
        internal Uri GenerateEntityId(HttpRequest request)
        {
            return ResultHelpers.GenerateODataLink(request, Entity, isEntityId: true);
        }

        // internal just for unit test.
        internal Uri GenerateLocationHeader(HttpRequest request)
        {
            return ResultHelpers.GenerateODataLink(request, Entity, isEntityId: false);
        }
    }
}
