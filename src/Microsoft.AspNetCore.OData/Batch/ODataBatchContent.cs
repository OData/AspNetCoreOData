//-----------------------------------------------------------------------------
// <copyright file="ODataBatchContent.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Batch
{
    /// <summary>
    /// Encapsulates a collection of OData batch responses.
    /// </summary>
    public class ODataBatchContent
    {
        private IServiceProvider _requestContainer;
        private ODataMessageWriterSettings _writerSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataBatchContent"/> class.
        /// </summary>
        /// <param name="responses">The batch responses.</param>
        /// <param name="requestContainer">The dependency injection container for the request.</param>
        public ODataBatchContent(IEnumerable<ODataBatchResponseItem> responses, IServiceProvider requestContainer)
            : this(responses, requestContainer, null/*contentType*/)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataBatchContent"/> class.
        /// </summary>
        /// <param name="responses">The batch responses.</param>
        /// <param name="requestContainer">The dependency injection container for the request.</param>
        /// <param name="contentType">The response content type.</param>
        public ODataBatchContent(IEnumerable<ODataBatchResponseItem> responses, IServiceProvider requestContainer, string contentType)
        {
            Responses = responses ?? throw Error.ArgumentNull(nameof(responses));

            _requestContainer = requestContainer;
            _writerSettings = requestContainer.GetService<ODataMessageWriterSettings>() ?? new ODataMessageWriterSettings();

            // Set the Content-Type header for existing batch formats
            if (contentType == null)
            {
                contentType = string.Format(
                    CultureInfo.InvariantCulture, "multipart/mixed;boundary=batchresponse_{0}", Guid.NewGuid());
            }

            Headers = new HeaderDictionary();
            Headers["Content-Type"] = contentType;
            ODataVersion version = _writerSettings.Version ?? ODataVersionConstraint.DefaultODataVersion;
            Headers.Add(ODataVersionConstraint.ODataServiceVersionHeader, ODataUtils.ODataVersionToString(version));
        }

        /// <summary>
        /// Gets the batch responses.
        /// </summary>
        public IEnumerable<ODataBatchResponseItem> Responses { get; }

        /// <summary>
        /// Gets the Headers for the batch content.
        /// </summary>
        public IHeaderDictionary Headers { get; }

        /// <summary>
        /// Serialize the batch content to a stream.
        /// </summary>
        /// <param name="stream">The stream to serialize to.</param>
        /// <returns>A <see cref="Task"/> that can be awaited.</returns>
        /// <remarks>This function uses types that are AspNetCore-specific.</remarks>
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        public Task SerializeToStreamAsync(Stream stream)
        {
            // how to dispose it?
            IODataResponseMessage responseMessage = ODataMessageWrapperHelper.Create(stream, Headers, _requestContainer);
            return WriteToResponseMessageAsync(responseMessage);
        }

        /// <summary>
        ///  Serialize the batch responses to an <see cref="IODataResponseMessage"/>.
        /// </summary>
        /// <param name="responseMessage">The response message.</param>
        /// <returns></returns>
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        private async Task WriteToResponseMessageAsync(IODataResponseMessage responseMessage)
        {
            // how to dispose it?
            ODataMessageWriter messageWriter = new ODataMessageWriter(responseMessage, _writerSettings);

            ODataBatchWriter writer = await messageWriter.CreateODataBatchWriterAsync().ConfigureAwait(false);

            await writer.WriteStartBatchAsync().ConfigureAwait(false);

            foreach (ODataBatchResponseItem response in Responses)
            {
                await response.WriteResponseAsync(writer).ConfigureAwait(false);
            }

            await writer.WriteEndBatchAsync().ConfigureAwait(false);
        }
    }
}
