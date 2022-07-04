//-----------------------------------------------------------------------------
// <copyright file="BadRequestODataResult.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData;
using ErrorUtils = Microsoft.AspNetCore.OData.Error;

namespace Microsoft.AspNetCore.OData.Results
{
    /// <summary>
    /// Represents a result that when executed will produce a Bad Request (400) response.
    /// </summary>
    /// <remarks>This result creates an <see cref="ODataError"/> with status code: 400.</remarks>
    public class BadRequestODataResult : BadRequestResult, IODataErrorResult
    {
        private const string errorCode = "400";

        /// <summary>
        /// OData error.
        /// </summary>
        public ODataError Error { get; }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="message">Error message.</param>
        public BadRequestODataResult(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw ErrorUtils.ArgumentNullOrEmpty(nameof(message));
            }

            Error = new ODataError
            {
                Message = message,
                ErrorCode = errorCode
            };
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="odataError">An <see cref="ODataError"/> object.</param>
        public BadRequestODataResult(ODataError odataError)
        {
            if (odataError == null)
            {
                throw ErrorUtils.ArgumentNull(nameof(odataError));
            }

            ErrorUtils.ValidateErrorCode(errorCode, odataError);

            Error = odataError;
        }

        /// <inheritdoc/>
        public async override Task ExecuteResultAsync(ActionContext context)
        {
            ObjectResult objectResult = new ObjectResult(Error)
            {
                StatusCode = StatusCodes.Status400BadRequest
            };

            await objectResult.ExecuteResultAsync(context).ConfigureAwait(false);
        }
    }
}
