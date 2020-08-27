// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.Extensions.Primitives;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Test.Batch
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpContent"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ODataHttpContentExtensions
    {
        /// <summary>
        /// Gets the <see cref="ODataMessageReader"/> for the <see cref="HttpContent"/> stream.
        /// </summary>
        /// <param name="content">The <see cref="HttpContent"/>.</param>
        /// <param name="settings">The <see cref="ODataMessageReaderSettings"/>.</param>
        /// <returns>A task object that produces an <see cref="ODataMessageReader"/> when completed.</returns>
        public static Task<ODataMessageReader> GetODataMessageReaderAsync(this HttpContent content, ODataMessageReaderSettings settings)
        {
            return GetODataMessageReaderAsync(content, settings, CancellationToken.None);
        }

        /// <summary>
        /// Gets the <see cref="ODataMessageReader"/> for the <see cref="HttpContent"/> stream.
        /// </summary>
        /// <param name="content">The <see cref="HttpContent"/>.</param>
        /// <param name="settings">The <see cref="ODataMessageReaderSettings"/>.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task object that produces an <see cref="ODataMessageReader"/> when completed.</returns>
        public static async Task<ODataMessageReader> GetODataMessageReaderAsync(this HttpContent content,
            ODataMessageReaderSettings settings, CancellationToken cancellationToken)
        {
            if (content == null)
            {
                throw Error.ArgumentNull("content");
            }

            cancellationToken.ThrowIfCancellationRequested();
            Stream contentStream = await content.ReadAsStreamAsync();

            HeaderDictionary headerDict = new HeaderDictionary();
            foreach (var head in content.Headers)
            {
                headerDict[head.Key] = new StringValues(head.Value.ToArray());
            }

            IODataRequestMessage oDataRequestMessage = ODataMessageWrapperHelper.Create(contentStream, headerDict/*, new MockServiceProvider()*/);
            ODataMessageReader oDataMessageReader = new ODataMessageReader(oDataRequestMessage, settings);
            return oDataMessageReader;
        }
    }
}
