//-----------------------------------------------------------------------------
// <copyright file="ODataErrorResult.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData;
using ErrorUtils = Microsoft.AspNetCore.OData.Error;

namespace Microsoft.AspNetCore.OData.Results
{
    /// <summary>
    /// Represents a result that when executed will produce an <see cref="ActionResult"/>.
    /// </summary>
    /// <remarks>This result creates an <see cref="ODataError"/> response.</remarks>
    public class ODataErrorResult : ActionResult, IODataErrorResult
    {
        /// <summary>
        /// OData Error.
        /// </summary>
        public ODataError Error { get; }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public ODataErrorResult(string errorCode, string message)
        {
            if (string.IsNullOrEmpty(errorCode))
            {
                throw ErrorUtils.ArgumentNullOrEmpty(nameof(errorCode));
            }

            if (string.IsNullOrEmpty(message))
            {
                throw ErrorUtils.ArgumentNullOrEmpty(nameof(message));
            }

            Error = new ODataError
            {
                ErrorCode = errorCode,
                Message = message
            };
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public ODataErrorResult(ODataError odataError)
        {
            if (odataError == null)
            {
                throw ErrorUtils.ArgumentNull(nameof(odataError));
            }

            Error = odataError;
        }

        /// <inheritdoc/>
        public async override Task ExecuteResultAsync(ActionContext context)
        {
            ObjectResult objectResult = new ObjectResult(Error)
            {
                StatusCode = Convert.ToInt32(Error.ErrorCode, CultureInfo.InvariantCulture)
            };

            await objectResult.ExecuteResultAsync(context).ConfigureAwait(false);
        }
    }
}
