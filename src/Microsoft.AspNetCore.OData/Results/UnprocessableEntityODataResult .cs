//-----------------------------------------------------------------------------
// <copyright file="UnprocessableEntityODataResult .cs" company=".NET Foundation">
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
    /// Represents a result that when executed will produce an UnprocessableEntity (422) response.
    /// </summary>
    /// <remarks>This result creates an <see cref="ODataError"/> with status code: 422.</remarks>
    public class UnprocessableEntityODataResult : UnprocessableEntityResult, IODataErrorResult
    {
        private const string errorCode = "422";

        /// <summary>
        /// OData error.
        /// </summary>
        public ODataError Error { get; }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="message">Error message.</param>
        public UnprocessableEntityODataResult(string message)
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
        public UnprocessableEntityODataResult(ODataError odataError)
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
                StatusCode = StatusCodes.Status422UnprocessableEntity
            };

            await objectResult.ExecuteResultAsync(context).ConfigureAwait(false);
        }
    }
}
